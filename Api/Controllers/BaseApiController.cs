using Microsoft.AspNetCore.Mvc;

namespace BirdWatching.Api.Controllers;

using BirdWatching.Shared.Model;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected IAuthTokenRepository _authRepo = null!;
    protected AppDbContext _context = null!;

    protected IResult AuthUserByToken(string token, int userId)
    {
        AuthToken? auth = _authRepo.GetByString(token);
        if (auth is null) return Results.BadRequest("Cannot find user by token.");
        if (auth.User.Id != userId) return Results.BadRequest("Don't have permission to do this.");
        return Results.Ok();
    }

    protected IResult AuthAdminByToken(string token)
    {
        AuthToken? auth = _authRepo.GetByString(token);
        if (auth is null) return Results.BadRequest("Cannot find user by token.");
        if (auth.User is null) return Results.Problem("User not set as reference.");
        if (!auth.User.IsAdmin) return Results.BadRequest("Don't have permission to do this.");
        return Results.Ok();
    }
}
