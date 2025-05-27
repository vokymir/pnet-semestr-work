using Microsoft.AspNetCore.Mvc;

namespace BirdWatching.Api.Controllers;
using System.Security.Cryptography;

using BirdWatching.Shared.Model;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected AppDbContext _context = null!;
    protected IAuthTokenRepository _authRepo = null!;
    protected IBirdRepository _birdRepo = null!;
    protected IEventRepository _eventRepo = null!;
    protected IRecordRepository _recordRepo = null!;
    protected IUserRepository _userRepo = null!;
    protected IWatcherRepository _watcherRepo = null!;

    protected void Init()
    {
        _authRepo = new EFAuthTokenRepository(_context);
        _birdRepo = new EFBirdRepository(_context);
        _eventRepo = new EFEventRepository(_context);
        _recordRepo = new EFRecordRepository(_context);
        _userRepo = new EFUserRepository(_context);
        _watcherRepo = new EFWatcherRepository(_context);
    }

    protected IActionResult AuthUserByToken(string token, int userId)
    {
        AuthToken? auth = _authRepo.GetByString(token);
        if (auth is null)
            return BadRequest(new { error = "Cannot find user by token." });

        if (auth.User is null)
            return StatusCode(500, new { error = "User not set as reference." });

        if (auth.User.Id != userId)
            return Unauthorized(new { error = "Don't have permission to do this." });

        return Ok();
    }

    protected (IActionResult Result, User? User) AuthUserByToken(string token)
    {
        AuthToken? auth = _authRepo.GetByString(token);
        if (auth is null)
            return (BadRequest(new { error = "Cannot find user by token." }), null);

        if (auth.User is null)
            return (StatusCode(500, new { error = "User not set as reference." }), null);

        var u = _userRepo.GetById(auth.User.Id);
        return (Ok(), u);
    }

    protected IActionResult AuthAdminByToken(string token)
    {
        AuthToken? auth = _authRepo.GetByString(token);
        if (auth is null)
            return BadRequest(new { error = "Cannot find user by token." });

        if (auth.User is null)
            return StatusCode(500, new { error = "User not set as reference." });

        if (!auth.User.IsAdmin)
            return Unauthorized(new { error = "Don't have permission to do this." });

        return Ok();
    }

    protected string GenerateUrlSafeString(int length)
    {
        byte[] data = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(data);
        }

        return Convert.ToBase64String(data).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
