namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using BirdWatching.Shared.Model;

[ApiController]
[Route("api/[controller]")]
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
    public IActionResult CreateUser([FromBody] UserDto userDto)
    {
        if (userDto == null)
            return BadRequest("User data must be provided.");

        try
        {
            User user = userDto.ToEntity();
            _userRepo.Add(user);
            return Ok(user.ToFullDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user.");
            return Problem("An error occurred while creating the user.");
        }
    }

    /// <summary>
    /// Update self, or if admin any user.
    /// </summary>
    [HttpPost("Update/{token}")]
    public IActionResult UpdateUser(string token, [FromBody] UserDto userDto)
    {
        if (string.IsNullOrWhiteSpace(token) || userDto == null)
            return BadRequest("Invalid token or user data.");

        var authResult = AuthUserByToken(token, userDto.Id);
        if (!IsAuthorized(authResult))
        {
            var adminAuth = AuthAdminByToken(token);
            if (!IsAuthorized(adminAuth))
                return Unauthorized("Access denied.");
        }

        User? user = _userRepo.GetById(userDto.Id);
        if (user == null)
            return NotFound("User not found.");

        // Only update fields that are allowed to be updated
        user.UserName = userDto.UserName ?? user.UserName;
        user.PasswordHash = userDto.PasswordHash ?? user.PasswordHash;

        try
        {
            _userRepo.Update(user);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user with ID {UserId}.", userDto.Id);
            return Problem("Failed to update user.");
        }
    }

    /// <summary>
    /// Delete self or if admin anyone.
    /// </summary>
    [HttpDelete("Delete/{token}")]
    public IActionResult DeleteUser(string token, [FromQuery] int userId)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Token must be provided.");

        var authResult = AuthUserByToken(token, userId);
        if (!IsAuthorized(authResult))
        {
            var adminAuth = AuthAdminByToken(token);
            if (!IsAuthorized(adminAuth))
                return Unauthorized("Access denied.");
        }

        try
        {
            bool deleted = _userRepo.Delete(userId);
            if (!deleted)
                return NotFound("User not found.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user with ID {UserId}.", userId);
            return Problem("Failed to delete user.");
        }
    }

    /// <summary>
    /// Get current user info.
    /// </summary>
    [HttpGet("Get/{token}")]
    public IActionResult GetUser(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Token must be provided.");

        var authResult = AuthUserByToken(token);
        if (!IsAuthorized(authResult.Result))
            return Unauthorized("Access denied.");

        User? user = authResult.User;
        if (user == null)
            return NotFound("User not found.");

        var userDto = user.ToFullDto();
        userDto.AuthTokens = null;
        return Ok(userDto);
    }

    /// <summary>
    /// Get info about user by ID, either self or admin.
    /// </summary>
    [HttpGet("Get/{token}/{userId}")]
    public IActionResult GetUser(string token, int userId)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Token must be provided.");

        var authResult = AuthUserByToken(token, userId);
        if (!IsAuthorized(authResult))
        {
            var adminAuth = AuthAdminByToken(token);
            if (!IsAuthorized(adminAuth))
                return Unauthorized("Access denied.");
        }

        User? user = _userRepo.GetById(userId);
        if (user == null)
            return NotFound("User not found.");

        var userDto = user.ToFullDto();
        userDto.AuthTokens = null;
        return Ok(userDto);
    }

    /// <summary>
    /// Get all users (admin only).
    /// </summary>
    [HttpGet("GetAll/{token}")]
    public IActionResult GetAllUsers(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Token must be provided.");

        var authResult = AuthAdminByToken(token);
        if (!IsAuthorized(authResult))
            return Unauthorized("Admin privileges required.");

        var users = _userRepo.GetAll();
        var userDtos = users.Select(u => {
            var dto = u.ToFullDto();
            dto.AuthTokens = null;
            return dto;
        }).ToArray();

        return Ok(userDtos);
    }

    /// <summary>
    /// Add curated watcher to current user.
    /// </summary>
    [HttpPost("AddCuratedWatcher/{token}/{watcherPublicId}")]
    public IActionResult AddCuratedWatcher(string token, string watcherPublicId)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(watcherPublicId))
            return BadRequest("Token and watcherPublicId must be provided.");

        var authResult = AuthUserByToken(token);
        if (!IsAuthorized(authResult.Result))
            return Unauthorized("Access denied.");

        User? user = authResult.User;
        if (user == null)
            return NotFound("User not found.");

        Watcher? watcher = _watcherRepo.GetByPublicId(watcherPublicId);
        if (watcher == null)
            return NotFound("Watcher not found.");

        try
        {
            if (!watcher.Curators.Contains(user))
            {
                watcher.Curators.Add(user);
                _watcherRepo.Update(watcher);
            }
            else
            {
                return Conflict("User is already a curator of this watcher.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add curator {UserId} to watcher {WatcherId}.", user.Id, watcherPublicId);
            return Problem("Failed to add curator.");
        }

        return Ok();
    }
}
