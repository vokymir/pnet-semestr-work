namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

using BirdWatching.Shared.Model;

[ApiController]
public class AuthController : BaseApiController
{
    private readonly ILogger<AuthController> _logger;
    private readonly AppDbContext _context;
    private IUserRepository _repo;

    public AuthController(AppDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
        _repo = new EFUserRepository(_context);
    }

    public IResult Login(LoginDto login)
    {
        User? user = _repo.GetByUsername(login.username);
        if (user is null)
            return Results.NotFound();
        else if (user.PasswordHash != login.passwordhash)
            return Results.NotFound();
        else
            return Results.Ok("??? HERE TO GIVE TOKEN ???");
    }
}
