using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BirdWatching.Shared.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BirdWatching.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _config = null!;

    public static int ValidHours = 1;

    public AuthController(AppDbContext context, ILogger<AuthController> logger, IConfiguration config)
    {
        _context = context;
        _logger = logger;
        _config = config;
        InitRepos__ContextMustNotBeNull();
    }

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        // 1) Over zadané přihlašovací údaje v DB
        var user = await _userRepo.GetByUsernameAsync(login.username);
        if (user == null || user.PasswordHash != login.passwordhash)
            return Unauthorized(new ProblemDetails {
                Status = 401,
                Title = "Unauthorized",
                Detail = "Neplatné jméno nebo heslo."
            });

        // 2) Vytvoříme claimy
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,           user.UserName),
            new Claim(ClaimTypes.Role,           user.IsAdmin ? "Admin" : "User")
        };

        // 3) Načteme klíč, issuer a audience z User Secrets
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var issuer = _config["JWT:Issuer"];
        var audience = _config["JWT:Audience"];

        // 4) Složení tokenu
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(ValidHours),
            signingCredentials: creds
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // 5) Vrátíme token klientovi
        return Ok(tokenString);
    }
}
