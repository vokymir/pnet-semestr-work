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

    protected IResult AuthUserByToken(string token, int userId)
    {
        AuthToken? auth = _authRepo.GetByString(token);
        if (auth is null) return Results.BadRequest("Cannot find user by token.");
        if (auth.User is null) return Results.Problem("User not set as reference.");
        if (auth.User.Id != userId) return Results.BadRequest("Don't have permission to do this.");
        return Results.Ok();
    }

    protected (IResult Result, User? User) AuthUserByToken(string token)
    {
        AuthToken? auth = _authRepo.GetByString(token);
        if (auth is null) return (Results.BadRequest("Cannot find user by token."), null);
        if (auth.User is null) return (Results.Problem("User not set as reference."), null);
        var u = _userRepo.GetById(auth.User.Id);
        return (Results.Ok(), u);
    }

    protected IResult AuthAdminByToken(string token)
    {
        AuthToken? auth = _authRepo.GetByString(token);
        if (auth is null) return Results.BadRequest("Cannot find user by token.");
        if (auth.User is null) return Results.Problem("User not set as reference.");
        if (!auth.User.IsAdmin) return Results.BadRequest("Don't have permission to do this.");
        return Results.Ok();
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
