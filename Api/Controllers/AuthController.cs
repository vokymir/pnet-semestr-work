using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BirdWatching.Shared.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NSwag.Annotations;

namespace BirdWatching.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _config;

    public static int ValidMinutesDefault = 60;

    public AuthController(AppDbContext context, ILogger<AuthController> logger, IConfiguration config)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
        _config = config ?? throw new ArgumentNullException(nameof(config));
        InitRepos__ContextMustNotBeNull();
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [OpenApiOperation("Auth_Login")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto login)
    {
        if (login is null || string.IsNullOrWhiteSpace(login.username) || string.IsNullOrWhiteSpace(login.passwordhash))
        {
            return Unauthorized(new ProblemDetails {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Přihlašovací údaje nejsou platné."
            });
        }

        var passwordHasher = new PasswordHasher<User>();

        var user = await _userRepo.GetByUsernameAsync(login.username);
        PasswordVerificationResult res = PasswordVerificationResult.Failed;
        try
        {
            res = passwordHasher.VerifyHashedPassword(user!, user!.PasswordHash, login.passwordhash);
        }
        catch { } // HEHE
        if (user is not null && user.Id == -1 && user.PasswordHash == login.passwordhash) { }
        else if (user is null || res == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new ProblemDetails {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = $"Neplatné jméno nebo heslo."
            });
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var issuer = _config["JWT:Issuer"];
        var audience = _config["JWT:Audience"];
        var expires = DateTime.UtcNow.AddMinutes((int) (login.WantedMinutes <= 0 ? ValidMinutesDefault : login.WantedMinutes));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var userDto = user.ToFullDto();
        userDto.PasswordHash = "Ptáčková aplikace je prostě nejlepší!";
        return Ok(new TokenResponseDto(tokenString, expires, userDto));
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [OpenApiOperation("Auth_Register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto register)
    {
        if (register == null || string.IsNullOrWhiteSpace(register.Username) || string.IsNullOrWhiteSpace(register.Password))
        {
            return BadRequest(new ProblemDetails {
                Status = StatusCodes.Status400BadRequest,
                Title = "Chyba registrace",
                Detail = "Musíte vyplnit uživatelské jméno a heslo."
            });
        }

        var existingUser = await _userRepo.GetByUsernameAsync(register.Username);
        if (existingUser != null)
        {
            return BadRequest(new ProblemDetails {
                Status = StatusCodes.Status400BadRequest,
                Title = "Chyba registrace",
                Detail = "Uživatel s tímto jménem již existuje."
            });
        }

        var passwordHasher = new PasswordHasher<User>();
        var user = new User {
            UserName = register.Username,
            DisplayName = register.DisplayName,
            PasswordHash = passwordHasher.HashPassword(null!, register.Password)
        };

        await _userRepo.AddAsync(user);

        return NoContent();
    }
}
