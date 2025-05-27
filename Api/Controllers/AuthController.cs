namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

using BirdWatching.Shared.Model;

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
    public TimeSpan TokenTimeSpan = new TimeSpan(0, 1, 0, 0);

    public AuthController(AppDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
        Init();
    }

    [HttpPost("Login")]
    public IActionResult Login(LoginDto login)
    {
        User? user = _userRepo.GetByUsername(login.username);
        if (user is null)
            return NotFound();
        else if (user.PasswordHash != login.passwordhash)
            return NotFound();

        string token;
        do
            token = GenerateUrlSafeString(64);
        while (!IsUnique(token));

        AddAuthToken(token, user);

        return Ok(token);
    }

    [HttpPost("Logout/{token}")]
    public IActionResult Logout(string token)
    {
        try
        {
            _authRepo.Delete(token);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }

        return Ok();
    }

    private void AddAuthToken(string token, User user)
    {
        _authRepo.Add(
                new AuthToken() {
                    Token = token,
                    User = user,
                    Created = DateTime.Now
                });
    }

    private bool IsUnique(string token)
    {
        var existing = _authRepo.GetByString(token);
        return existing is null;
    }

    // zde jde optimalizovat - předělat do SQL v Modelu, a bylo by to rychlejší
    [HttpPost("DeleteOldTokens")]
    public void DeleteOldTokens()
    {
        foreach (var token in _authRepo.GetAll())
            if (token.Created + TokenTimeSpan < DateTime.Now)
                _authRepo.Delete(token.Token);
    }
}
