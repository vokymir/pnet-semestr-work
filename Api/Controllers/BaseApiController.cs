using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BirdWatching.Shared.Model;

namespace BirdWatching.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected AppDbContext _context = null!;
    protected IBirdRepository _birdRepo = null!;
    protected IEventRepository _eventRepo = null!;
    protected IRecordRepository _recordRepo = null!;
    protected IUserRepository _userRepo = null!;
    protected IWatcherRepository _watcherRepo = null!;

    protected readonly bool DEBUG = true;

    protected void InitRepos__ContextMustNotBeNull()
    {
        _birdRepo = new EFBirdRepository(_context);
        _eventRepo = new EFEventRepository(_context);
        _recordRepo = new EFRecordRepository(_context);
        _userRepo = new EFUserRepository(_context);
        _watcherRepo = new EFWatcherRepository(_context);
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

    protected int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(claim, out var id) ? id : null;
    }

}
