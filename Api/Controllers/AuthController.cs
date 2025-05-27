using Microsoft.AspNetCore.Mvc;
using BirdWatching.Shared.Model;

namespace BirdWatching.Api.Controllers;

/// <summary>
/// Can do:
/// - LogIN/OUT
/// - give new authTokens
/// - delete old tokens when asked to
/// </summary>
[ApiController]
public class AuthController : BaseApiController
{
    private readonly ILogger<AuthController> _logger;

    // valid for 1 hour
    public TimeSpan TokenTimeSpan = TimeSpan.FromHours(1);

    public AuthController(AppDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
        Init();
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        var user = await _userRepo.GetByUsernameAsync(login.username);
        if (user is null || user.PasswordHash != login.passwordhash)
            return NotFound();

        string token;
        do
        {
            token = GenerateUrlSafeString(64);
        }
        while (!await IsUniqueAsync(token));

        await AddAuthTokenAsync(token, user);
        return Ok(token);
    }

    [HttpPost("Logout/{token}")]
    public async Task<IActionResult> Logout(string token)
    {
        try
        {
            await _authRepo.DeleteAsync(token);
            return Ok();
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
    }

    private async Task AddAuthTokenAsync(string token, User user)
    {
        var authToken = new AuthToken {
            Token = token,
            User = user,
            Created = DateTime.Now
        };

        await _authRepo.AddAsync(authToken);
    }

    private async Task<bool> IsUniqueAsync(string token)
    {
        var existing = await _authRepo.GetByStringAsync(token);
        return existing is null;
    }

    [HttpPost("DeleteOldTokens")]
    public async Task<IActionResult> DeleteOldTokens()
    {
        var tokens = await _authRepo.GetAllAsync();
        foreach (var token in tokens)
        {
            if (token.Created + TokenTimeSpan < DateTime.Now)
            {
                await _authRepo.DeleteAsync(token.Token);
            }
        }

        return Ok();
    }
}
