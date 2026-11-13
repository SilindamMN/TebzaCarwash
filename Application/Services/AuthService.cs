namespace Application.Services
{
    using Application.Interfaces;
    using Domain;
    using Domain.Dtos.Auth;
    using Domain.Dtos.General;
    using Domain.Entities;
    using Domain.Entities.Supporting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides authentication and user-management services, including registration, login, token
    /// generation, role updates, and user information retrieval.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogService _logService;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager, ILogService logService, IConfiguration configuration)
        {
            this.applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _logService = logService;
            _configuration = configuration;
        }

        #region RegisterAsync
        public async Task<GeneralServiceResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var isExistsUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (isExistsUser is not null)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 409,
                    Message = "Email Already Exists"
                };
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createUserResult = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (!createUserResult.Succeeded)
            {
                var errorString = "User Creation failed because: ";
                foreach (var error in createUserResult.Errors)
                {
                    errorString += " # " + error.Description;
                }
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 400,
                    Message = errorString
                };
            }

            await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);
            await _logService.SaveNewLog(newUser.UserName, "Registered to Website");

            return new GeneralServiceResponseDto()
            {
                IsSucceed = true,
                StatusCode = 201,
                Message = "User Created Successfully"
            };
        }
        #endregion

        #region LoginAsync
        public async Task<LoginServiceResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user is null)
            {
                return null;
            }

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordCorrect)
            {
                return null;
            }

            var newToken = await GenerateJWTTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var userInfo = GenerateUserInfoObject(user, roles);
            await _logService.SaveNewLog(user.UserName, "New Login");

            return new LoginServiceResponseDto()
            {
                NewToken = newToken,
                UserInfo = userInfo
            };
        }
        #endregion

        #region UpdateRoleAsync
        public async Task<GeneralServiceResponseDto> UpdateRoleAsync(ClaimsPrincipal User, UpdateRoleDto updateRoleDto)
        {
            var user = await _userManager.FindByNameAsync(updateRoleDto.UserName);
            if (user is null)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 404,
                    Message = "Invalid UserName"
                };
            }

            if (!User.IsInRole(StaticUserRoles.ADMIN))
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 403,
                    Message = "You are not allowed to change roles"
                };
            }

           if (!Enum.IsDefined(typeof(RoleType), updateRoleDto.NewRole)
)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 403,
                    Message = "Invalid role change request"
                };
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);
            await _userManager.AddToRoleAsync(user, updateRoleDto.NewRole.ToString());

            if (updateRoleDto.NewRole == RoleType.WASHER)
            {
                bool hasProfile = await applicationDbContext.Washers.AnyAsync(w => w.UserId == user.Id);
                if (!hasProfile)
                {
                    await applicationDbContext.Washers.AddAsync(new Washer
                    {
                        UserId = user.Id,
                        IsAvailable = true
                    });
                }
            }
            else if (updateRoleDto.NewRole == RoleType.CUSTOMER)
            {
                bool hasProfile = await applicationDbContext.Customers.AnyAsync(c => c.UserId == user.Id);
                if (!hasProfile)
                {
                    await applicationDbContext.Customers.AddAsync(new Customer
                    {
                        UserId = user.Id
                    });
                }
            }

            await applicationDbContext.SaveChangesAsync();
            await _logService.SaveNewLog(user.UserName, "User Roles Updated");

            return GeneralServiceResponseDto.CreateResponse(true, 200, "Role updated successfully");
        }
        #endregion

        #region MeAsync
        public async Task<LoginServiceResponseDto?> MeAsync(MeDto meDto)
        {
            ClaimsPrincipal handler = new JwtSecurityTokenHandler().ValidateToken(meDto.Token, new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["JWT:ValidIssuer"],
                ValidAudience = _configuration["JWT:ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]))
            }, out SecurityToken securityToken);

            string? decodedUserName = handler.Claims.FirstOrDefault(q => q.Type == ClaimTypes.Name)?.Value;
            if (decodedUserName is null)
            {
                return null;
            }

            var user = await _userManager.FindByNameAsync(decodedUserName);
            if (user is null)
            {
                return null;
            }

            var newToken = await GenerateJWTTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var userInfo = GenerateUserInfoObject(user, roles);
            await _logService.SaveNewLog(user.UserName, "New Token Generated");

            return new LoginServiceResponseDto()
            {
                NewToken = newToken,
                UserInfo = userInfo
            };
        }
        #endregion

        #region GetUsersListAsync
        public async Task<IEnumerable<UserInfoResult>> GetUsersListAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            List<UserInfoResult> userInfoResults = new List<UserInfoResult>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userInfoResults.Add(GenerateUserInfoObject(user, roles));
            }

            return userInfoResults;
        }
        #endregion

        #region GetUserDetailsByUserNameAsync
        public async Task<UserInfoResult?> GetUserDetailsByUserNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return GenerateUserInfoObject(user, roles);
        }
        #endregion

        #region GetUsernamesListAsync
        public async Task<IEnumerable<string>> GetUsernamesListAsync()
        {
            return await _userManager.Users
                .Select(q => q.UserName)
                .ToListAsync();
        }
        #endregion

        #region GenerateJWTTokenAsync
        private async Task<string> GenerateJWTTokenAsync(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim("FirstName", user.FirstName ?? ""),
                new Claim("LastName", user.LastName ?? ""),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var signingCredentials = new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256);

            var tokenObject = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(3),
                claims: authClaims,
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenObject);
        }
        #endregion

        #region GenerateUserInfoObject
        private UserInfoResult GenerateUserInfoObject(ApplicationUser user, IEnumerable<string> Roles)
        {
            return new UserInfoResult()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Roles = Roles
            };
        }
        #endregion
    }
}