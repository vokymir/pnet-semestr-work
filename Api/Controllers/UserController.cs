namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

using BirdWatching.Shared.Model;

[ApiController]
public class UserController : BaseApiController
{
    private readonly ILogger<UserController> _logger;
    private readonly AppDbContext _context;

    public UserController(AppDbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("CreateUser")]
    public void CreateUser(UserDto userDto)
    {
        IUserRepository repo = new EFUserRepository(_context);

        User user = new User() { UserName = userDto.username, PasswordHash = userDto.passwordhash };
        repo.Add(user);
    }
}
