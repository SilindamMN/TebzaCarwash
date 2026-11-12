namespace Application.Services
{
  using Application.Interfaces;
  using Domain;
  using Domain.Dtos.Auth;
  using Domain.Dtos.General;
  using Domain.Entities;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.IdentityModel.Tokens;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IdentityModel.Tokens.Jwt;
  using System.Security.Claims;
  using System.Text;
  using System.Threading.Tasks;

  public class AuthService : IAuthService
  {
    private readonly ApplicationDbContext applicationDbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly ILogService _logService;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager, ILogService logService, IConfiguration configuration)
    {
      this.applicationDbContext = applicationDbContext;
      this._userManager = userManager;
      _roleManager = roleManager;
      _logService = logService;
      _configuration = configuration;
    }

    #region SeedRolesAsync
    public async Task<GeneralServiceResponseDto> SeedRolesAsync()
    {
      bool isRunnerRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.RUNNER);
      bool isAdminRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
      bool isCustomerRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.CUSTOMER);
      bool isUserRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);

      if (isAdminRoleExists && isRunnerRoleExists && isCustomerRoleExists && isUserRoleExists)
      {
        return new GeneralServiceResponseDto()
        {
          IsSucceed = true,
          StatusCode = 200,
          Message = "Roles Seeding is Already Done"
        };
      }

      await _roleManager.CreateAsync(new IdentityRole<int>(StaticUserRoles.RUNNER));
      await _roleManager.CreateAsync(new IdentityRole<int>(StaticUserRoles.ADMIN));
      await _roleManager.CreateAsync(new IdentityRole<int>(StaticUserRoles.CUSTOMER));
      await _roleManager.CreateAsync(new IdentityRole<int>(StaticUserRoles.USER));

      return new GeneralServiceResponseDto()
      {
        IsSucceed = true,
        StatusCode = 201,
        Message = "Roles Seeding Done Successfully"
      };
    }
    #endregion

    #region RegisterAsync
    public async Task<GeneralServiceResponseDto> RegisterAsync(RegisterDto registerDto)
    {
      var isExistsUser = await _userManager.FindByNameAsync(registerDto.UserName);
      if (isExistsUser is not null)
      {
        return new GeneralServiceResponseDto()
        {
          IsSucceed = false,
          StatusCode = 409,
          Message = "UserName Already Exists"
        };
      }

      ApplicationUser newUser = new ApplicationUser()
      {
        FirstName = registerDto.FirstName,
        LastName = registerDto.LastName,
        Email = registerDto.Email,
        UserName = registerDto.UserName,
        Address = registerDto.Address,
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

      // Add a Default USER Role to all users
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
      // Find user with username
      var user = await _userManager.FindByNameAsync(loginDto.UserName);
      if (user is null)
      {
        return null;
      }

      // check password of user
      var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);
      if (!isPasswordCorrect)
      {
        return null;
      }

      // Return Token and userInfo to front-end
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

      var userRoles = await _userManager.GetRolesAsync(user);

      // Only ADMIN can update roles
      if (User.IsInRole(StaticUserRoles.ADMIN))
      {
        // Only change role to USER, RUNNER, or CUSTOMER
        if (updateRoleDto.NewRole == RoleType.USER || updateRoleDto.NewRole == RoleType.RUNNER || updateRoleDto.NewRole == RoleType.CUSTOMER)
        {
          // Remove the old roles and assign the new one
          await _userManager.RemoveFromRolesAsync(user, userRoles);
          await _userManager.AddToRoleAsync(user, updateRoleDto.NewRole.ToString());

          // Insert into the respective table (Runner or Customer) based on the new role
          if (updateRoleDto.NewRole == RoleType.RUNNER)
          {
            // Add the user as a Runner
            var runner = new Runner
            {
              UserId = user.Id,
              IsAvailable = true, // Set as needed
              CurrentLatitude = 0.0, // Set as needed
              CurrentLongitude = 0.0, // Set as needed
              LastSeen = DateTime.Now
            };
          //  await applicationDbContext.Runners.AddAsync(runner);
          }
          else if (updateRoleDto.NewRole == RoleType.CUSTOMER)
          {
            // Add the user as a Customer
            var customer = new Customer
            {
              UserId = user.Id,
              Address = user.Address,
              PhoneNumber = user.PhoneNumber,
            };
            await applicationDbContext.Customers.AddAsync(customer);
          }

          await _logService.SaveNewLog(user.UserName, "User Roles Updated");

          return GeneralServiceResponseDto.CreateResponse(true, 200, "Role updated successfully");
        }
        else
        {
          return new GeneralServiceResponseDto()
          {
            IsSucceed = false,
            StatusCode = 403,
            Message = "Invalid role change request"
          };
        }
      }
      else
      {
        return new GeneralServiceResponseDto()
        {
          IsSucceed = false,
          StatusCode = 403,
          Message = "You are not allowed to change roles"
        };
      }
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

      string decodedUserName = handler.Claims.First(q => q.Type == ClaimTypes.Name).Value;
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
        var userInfo = GenerateUserInfoObject(user, roles);
        userInfoResults.Add(userInfo);
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

      var roles = await this._userManager.GetRolesAsync(user);
      var userInfo = this.GenerateUserInfoObject(user, roles);
      return userInfo;
    }
    #endregion

    #region GetUsernamesListAsync
    public async Task<IEnumerable<string>> GetUsernamesListAsync()
    {
      var userNames = await this._userManager.Users
          .Select(q => q.UserName)
          .ToListAsync();

      return userNames;
    }
    #endregion

    #region GenerateJWTTokenAsync
    private async Task<string> GenerateJWTTokenAsync(ApplicationUser user)
    {
      var userRoles = await this._userManager.GetRolesAsync(user);

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

      var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration["JWT:Secret"]));
      var signingCredentials = new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256);

      var tokenObject = new JwtSecurityToken(
          issuer: this._configuration["JWT:ValidIssuer"],
          audience: this._configuration["JWT:ValidAudience"],
          notBefore: DateTime.Now,
          expires: DateTime.Now.AddHours(3),
          claims: authClaims,
          signingCredentials: signingCredentials
      );

      string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);
      return token;
    }

    #endregion

    #region GenerateUserInfoObject
    private UserInfoResult GenerateUserInfoObject(ApplicationUser user, IEnumerable<string> Roles)
    {
      // Instead of this, You can use Automapper packages. But i don't want it in this project
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

    internal class Runner
    {
        public int UserId { get; set; }
        public bool IsAvailable { get; set; }
        public double CurrentLatitude { get; set; }
        public double CurrentLongitude { get; set; }
        public DateTime LastSeen { get; set; }
    }
}