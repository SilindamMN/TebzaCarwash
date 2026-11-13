namespace API.Controllers
{
    using Application.Interfaces;
    using Domain.Dtos.Auth;
    using Domain.Entities.Supporting;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase
  {
        /// <summary>
        /// Provides access to authentication operations.
        /// </summary>
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
      _authService = authService;
    }

  
        /// <summary>
        /// Registers a user account.
        /// </summary>
        /// <param name="registerDto">The registration details.</param>
        /// <returns>An HTTP response containing the registration result's status code and message.</returns>
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
      var registerResult = await _authService.RegisterAsync(registerDto);
      return StatusCode(registerResult.StatusCode, registerResult.Message);
    }
        /// <summary>
        /// Authenticates the supplied credentials.
        /// </summary>
        /// <param name="loginDto">The login credentials.</param>
        /// <returns>An <see cref="UnauthorizedResult"/> if the credentials are invalid; otherwise, an <see
        /// cref="OkObjectResult"/> containing the login response.</returns>
    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<LoginServiceResponseDto>> Login([FromBody] LoginDto loginDto)
    {
      var loginResult = await _authService.LoginAsync(loginDto);

      if (loginResult is null)
      {
        return Unauthorized("Your credentials are invalid. Please contact to an Admin");
      }

      return Ok(loginResult);
    }

        /// <summary>
        /// Updates a user's role.
        /// </summary>
        /// <param name="updateRoleDto">The data identifying the user and the role to assign.</param>
        /// <returns>An action result indicating success or containing the status code and message returned by the role update
        /// operation.</returns>
    [HttpPost]
    [Route("update-role")]
    [Authorize(Roles = StaticUserRoles.ADMIN)]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDto updateRoleDto)
    {
      var updateRoleResult = await _authService.UpdateRoleAsync(User, updateRoleDto);

      if (updateRoleResult.IsSucceed)
      {
        return Ok(updateRoleResult.Message);
      }
      else
      {
        return StatusCode(updateRoleResult.StatusCode, updateRoleResult.Message);
      }
    }
        /// <summary>
        /// Authenticates the supplied token and returns the current user's details.
        /// </summary>
        /// <param name="token">The token used to retrieve the current user's details.</param>
        /// <returns>The user's details if the token is valid; otherwise, an unauthorized response.</returns>
    [HttpPost]
    [Route("me")]
    public async Task<ActionResult<LoginServiceResponseDto>> Me([FromBody] MeDto token)
    {
      try
      {
        var me = await _authService.MeAsync(token);
        if (me is not null)
        {
          return Ok(me);
        }
        else
        {
          return Unauthorized("Invalid Token");
        }
      }
      catch (Exception)
      {
        return Unauthorized("Invalid Token");
      }
    }
        /// <summary>
        /// Gets the list of users.
        /// </summary>
        /// <returns>An action result containing the list of users.</returns>
    [HttpGet]
    [Route("users")]
    public async Task<ActionResult<IEnumerable<UserInfoResult>>> GetUsersList()
    {
      var usersList = await _authService.GetUsersListAsync();

      return Ok(usersList);
    }
        /// <summary>
        /// Retrieves user details by user name.
        /// </summary>
        /// <param name="userName">The user name to look up.</param>
        /// <returns>An action result containing the user details if found, or a 404 Not Found response if no matching user
        /// exists.</returns>
    [HttpGet]
    [Route("users/{userName}")]
    public async Task<ActionResult<UserInfoResult>> GetUserDetailsByUserName([FromRoute] string userName)
    {
      var user = await _authService.GetUserDetailsByUserNameAsync(userName);
      if (user is not null)
      {
        return Ok(user);
      }
      else
      {
        return NotFound("UserName not found");
      }
    }
        /// <summary>
        /// Retrieves the list of user names.
        /// </summary>
        /// <returns>An action result containing the user names.</returns>
    [HttpGet]
    [Route("usernames")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserNamesList()
    {
      var usernames = await _authService.GetUsernamesListAsync();

      return Ok(usernames);
    }
  }
}