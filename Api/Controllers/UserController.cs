namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

using BirdWatching.Shared.Model;

[ApiController]
public class UserController : BaseApiController
{
    private readonly ILogger<UserController> _logger;
    private IUserRepository _userRepo;

    public UserController(AppDbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
        _userRepo = new EFUserRepository(_context);
        _authRepo = new EFAuthTokenRepository(_context);
    }

    [HttpPost("Create")]
    public IResult CreateUser(UserDto userDto)
    {
        User user = new User() { UserName = userDto.username, PasswordHash = userDto.passwordhash };

        _userRepo.Add(user);

        return Results.Ok();
    }

    [HttpPost("Update/{token}")]
    public IResult UpdateUser(string token, UserDto userDto)
    {
        IResult response = AuthUserByToken(token, userDto.id);
        if (!response.Equals(Results.Ok())) return response;

        User? user = _userRepo.GetById(userDto.id);
        if (user is null) return Results.NotFound();

        user.UserName = userDto.username;
        user.PasswordHash = userDto.passwordhash;

        try
        {
            _userRepo.Update(user);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }

        return Results.Ok();
    }

    [HttpDelete("Delete/{token}")]
    public IResult DeleteUser(string token, int userId)
    {
        IResult response = AuthUserByToken(token, userId);
        if (!response.Equals(Results.Ok())) return response;

        try
        {
            _userRepo.Delete(userId);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }

        return Results.Ok();
    }

    [HttpGet("Get/{token}")]
    public IResult GetUser(string token, int userId)
    {
        IResult response = AuthUserByToken(token, userId);
        if (!response.Equals(Results.Ok())) return response;

        User? user = _userRepo.GetById(userId);
        if (user is null)
            return Results.NotFound();

        UserDto userDto = new(user.Id, user.UserName, user.PasswordHash);
        return Results.Ok(userDto);
    }

    [HttpGet("GetAll/{token}")]
    public IResult GetAllUsers(string token)
    {
        IResult response = AuthAdminByToken(token);
        if (!response.Equals(Results.Ok())) return response;

        User[] users = _userRepo.GetAll().ToArray();
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
