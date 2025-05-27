namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

using BirdWatching.Shared.Model;

/// <summary>
/// Can do following:
/// - Create new user from dto.
/// - Update and delete self (or any if admin).
/// - List all users (Admin only).
/// - Get info about current user (or any if admin).
/// - Add curated watcher to existing user.
/// Should do:
/// -
/// </summary>
[ApiController]
public class UserController : BaseApiController
{
    private readonly ILogger<UserController> _logger;

    public UserController(AppDbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
        Init();
    }

    /// <summary>
    /// Create new user.
    /// </summary>
    [HttpPost("Create")]
    public IActionResult CreateUser(UserDto userDto)
    {
        User user = userDto.ToEntity();

        _userRepo.Add(user);

        return Ok();
    }

    /// <summary>
    /// Update self, or if admin any user.
    /// </summary>
    [HttpPost("Update/{token}")]
    public IActionResult UpdateUser(string token, UserDto userDto)
    {
        IActionResult response = AuthUserByToken(token, userDto.Id);
        if (!response.Equals(Ok()))
        {
            var adminResponse = AuthAdminByToken(token);
            if (!adminResponse.Equals(Ok()))
                return response;
        }

        User? user = _userRepo.GetById(userDto.Id);
        if (user is null) return NotFound();

        user.UserName = userDto.UserName;
        user.PasswordHash = userDto.PasswordHash;

        try
        {
            _userRepo.Update(user);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }

        return Ok();
    }

    /// <summary>
    /// Delete self or if admin anyone.
    /// </summary>
    [HttpDelete("Delete/{token}")]
    public IActionResult DeleteUser(string token, int userId)
    {
        IActionResult response = AuthUserByToken(token, userId);
        if (!response.Equals(Ok()))
        {
            var adminResponse = AuthAdminByToken(token);
            if (!adminResponse.Equals(Ok()))
                return response;
        }

        try
        {
            _userRepo.Delete(userId);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }

        return Ok();
    }

    /// <summary>
    /// Get all info about user, if you.
    /// </summary>
    [HttpGet("Get/{token}")]
    public IActionResult GetUser(string token)
    {
        var response = AuthUserByToken(token);
        if (!response.Result.Equals(Ok())) return response.Result;

        User? user = response.User;
        if (user is null)
            return NotFound();

        UserDto userDto = user.ToFullDto();
        userDto.AuthTokens = null;

        return Ok(userDto);
    }

    /// <summary>
    /// Get info about user, either self or must be admin.
    /// </summary>
    [HttpGet("Get/{token}/{userId}")]
    public IActionResult GetUser(string token, int userId)
    {
        IActionResult response = AuthUserByToken(token, userId);
        if (!response.Equals(Ok()))
        {
            var adminResponse = AuthAdminByToken(token);
            if (!adminResponse.Equals(Ok()))
                return response;
        }

        User? user = _userRepo.GetById(userId);
        if (user is null)
            return NotFound();

        UserDto userDto = user.ToFullDto();
        userDto.AuthTokens = null;

        return Ok(userDto);
    }

    /// <summary>
    /// Get all users if you are admin.
    /// </summary>
    [HttpGet("GetAll/{token}")]
    public IActionResult GetAllUsers(string token)
    {
        IActionResult response = AuthAdminByToken(token);
        if (!response.Equals(Ok())) return response;

        User[] users = _userRepo.GetAll().ToArray();
        UserDto[] userDtos = new UserDto[users.Length];

        for (int i = 0; i < users.Length; i++)
        {
            User user = users[i];
            UserDto userDto = user.ToFullDto();
            userDto.AuthTokens = null;
            userDtos[i] = userDto;
        }

        return Ok(userDtos);
    }

    [HttpPost("AddCuratedWatcher/{token}/{watcherPublicId}")]
    public IActionResult AddCuratedWatcher(string token, string watcherPublicId)
    {
        var response = AuthUserByToken(token);
        if (!response.Result.Equals(Ok())) return response.Result;

        User? user = response.User;
        if (user is null)
            return NotFound("User not found.");

        Watcher? watcher = _watcherRepo.GetByPublicId(watcherPublicId);
        if (watcher is null) return NotFound("Watcher not found.");

        try
        {
            watcher.Curators.Add(user);
            _watcherRepo.Update(watcher);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }

        return Ok();
    }
}
