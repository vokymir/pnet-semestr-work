namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

using BirdWatching.Shared.Model;

[ApiController]
public class UserController : BaseApiController
{
    private readonly ILogger<UserController> _logger;
    private readonly AppDbContext _context;
    private IUserRepository _repo;

    public UserController(AppDbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
        _repo = new EFUserRepository(_context);
    }

    [HttpPost("Create")]
    public IResult CreateUser(UserDto userDto)
    {
        User user = new User() { UserName = userDto.username, PasswordHash = userDto.passwordhash };

        _repo.Add(user);

        return Results.Ok();
    }

    [HttpPost("Update")]
    public IResult UpdateUser(UserDto userDto)
    {
        User? user = _repo.GetById(userDto.id);
        if (user is null)
            return Results.NotFound();

        user.UserName = userDto.username;
        user.PasswordHash = userDto.passwordhash;

        try
        {
            _repo.Update(user);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }

        return Results.Ok();
    }

    [HttpDelete("Delete")]
    public IResult DeleteUser(int userId)
    {
        try
        {
            _repo.Delete(userId);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }

        return Results.Ok();
    }

    [HttpGet("Get")]
    public IResult GetUser(int userId)
    {
        User? user = _repo.GetById(userId);
        if (user is null)
            return Results.NotFound();

        UserDto userDto = new(user.Id, user.UserName, user.PasswordHash);
        return Results.Ok(userDto);
    }

    [HttpGet("GetAll")]
    public IResult GetAllUsers()
    {
        User[] users = _repo.GetAll().ToArray();
        UserDto[] userDtos = new UserDto[users.Length];

        for (int i = 0; i < users.Length; i++)
        {
            User user = users[i];
            UserDto userDto = new(user.Id, user.UserName, user.PasswordHash);
            userDtos[i] = userDto;
        }

        return Results.Ok(userDtos);
    }
}
