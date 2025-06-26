using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

        var user = await _userRepo.GetByUsernameAsync(login.username);
        if (user == null || user.PasswordHash != login.passwordhash)
        {
            return Unauthorized(new ProblemDetails {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Neplatné jméno nebo heslo."
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
        userDto.PasswordHash = "Haha :>)";
        return Ok(new TokenResponseDto(tokenString, expires, userDto));
    }
}
