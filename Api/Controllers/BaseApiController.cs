using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using BirdWatching.Shared.Model;

namespace BirdWatching.Api.Controllers;

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

    protected async Task<IActionResult> AuthUserByTokenAsync(string token, int userId)
    {
        var auth = await _authRepo.GetByStringAsync(token);
        if (auth is null)
            return BadRequest(new { error = "Cannot find user by token." });

        if (auth.User is null)
            return StatusCode(500, new { error = "User not set as reference." });

        if (auth.User.Id != userId)
            return Unauthorized(new { error = "Don't have permission to do this." });

        return Ok();
    }

    protected async Task<(IActionResult Result, User? User)> AuthUserByTokenAsync(string token)
    {
        var auth = await _authRepo.GetByStringAsync(token);
        if (auth is null)
            return (BadRequest(new { error = "Cannot find user by token." }), null);

        if (auth.User is null)
            return (StatusCode(500, new { error = "User not set as reference." }), null);

        var user = await _userRepo.GetByIdAsync(auth.User.Id);
        return (Ok(), user);
    }

    protected async Task<IActionResult> AuthAdminByTokenAsync(string token)
    {
        var auth = await _authRepo.GetByStringAsync(token);
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
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        char[] result = new char[length];
        byte[] buffer = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(buffer);

        for (int i = 0; i < length; i++)
        {
            int index = buffer[i] % alphabet.Length;
            result[i] = alphabet[index];
        }

        return new string(result);
    }

    protected static bool IsAuthorized(IActionResult result) =>
        result is OkResult or OkObjectResult;
}
