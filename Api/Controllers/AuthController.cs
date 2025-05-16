namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

using BirdWatching.Shared.Model;

[ApiController]
public class AuthController : BaseApiController
{
    private readonly ILogger<AuthController> _logger;
    private IUserRepository _userRepo;

    // valid for 1 hour
    public TimeSpan TokenTimeSpan = new TimeSpan(0, 1, 0, 0);

    public AuthController(AppDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
        _userRepo = new EFUserRepository(_context);
        _authRepo = new EFAuthTokenRepository(_context);
    }

    [HttpPost("Login")]
    public IResult Login(LoginDto login)
    {
        User? user = _userRepo.GetByUsername(login.username);
        if (user is null)
            return Results.NotFound();
        else if (user.PasswordHash != login.passwordhash)
            return Results.NotFound();

        string token = GenerateAuthToken();
        AddAuthToken(token, user);

        return Results.Ok(token);
    }

    [HttpPost("Logout/{token}")]
    public IResult Logout(string token)
    {
        try
        {
            _authRepo.Delete(token);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }

        return Results.Ok();
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

    private string GenerateAuthToken()
    {
        int size = 64;

        byte[] data = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(data);
        }

        return Convert.ToBase64String(data).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    // zde jde optimalizovat - předělat do SQL v Modelu, a bylo by to rychlejší
    private void DeleteOldTokens()
    {
        foreach (var token in _authRepo.GetAll())
            if (token.Created + TokenTimeSpan < DateTime.Now)
                _authRepo.Delete(token.Token);
    }
}
