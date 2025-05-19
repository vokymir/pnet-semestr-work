namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

using BirdWatching.Shared.Model;

[ApiController]
public class UserController : BaseApiController
{
    private readonly ILogger<UserController> _logger;

    public UserController(AppDbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
        Init();
    }

    /// <summary>
    /// Create new user.
    /// </summary>
    [HttpPost("Create")]
    public IResult CreateUser(UserDto userDto)
    {
        User user = userDto.ToEntity();

        _userRepo.Add(user);

        return Results.Ok();
    }

    /// <summary>
    /// Update self, or if admin any user.
    /// </summary>
    [HttpPost("Update/{token}")]
    public IResult UpdateUser(string token, UserDto userDto)
    {
        IResult response = AuthUserByToken(token, userDto.Id);
        if (!response.Equals(Results.Ok()))
        {
            var adminResponse = AuthAdminByToken(token);
            if (!adminResponse.Equals(Results.Ok()))
                return response;
        }

        User? user = _userRepo.GetById(userDto.Id);
        if (user is null) return Results.NotFound();

        user.UserName = userDto.UserName;
        user.PasswordHash = userDto.PasswordHash;

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

    /// <summary>
    /// Delete self or if admin anyone.
    /// </summary>
    [HttpDelete("Delete/{token}")]
    public IResult DeleteUser(string token, int userId)
    {
        IResult response = AuthUserByToken(token, userId);
        if (!response.Equals(Results.Ok()))
        {
            var adminResponse = AuthAdminByToken(token);
            if (!adminResponse.Equals(Results.Ok()))
                return response;
        }

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

    /// <summary>
    /// Get all info about user, if you.
    /// </summary>
    [HttpGet("Get/{token}")]
    public IResult GetUser(string token)
    {
        var response = AuthUserByToken(token);
        if (!response.Result.Equals(Results.Ok())) return response.Result;

        User? user = response.User;
        if (user is null)
            return Results.NotFound();

        UserDto userDto = user.ToFullDto();
        userDto.AuthTokens = null;

        return Results.Ok(userDto);
    }

    /// <summary>
    /// Get info about user, either self or must be admin.
    /// </summary>
    [HttpGet("Get/{token}/{userId}")]
    public IResult GetUser(string token, int userId)
    {
        IResult response = AuthUserByToken(token, userId);
        if (!response.Equals(Results.Ok()))
        {
            var adminResponse = AuthAdminByToken(token);
            if (!adminResponse.Equals(Results.Ok()))
                return response;
        }

        User? user = _userRepo.GetById(userId);
        if (user is null)
            return Results.NotFound();

        UserDto userDto = user.ToFullDto();
        userDto.AuthTokens = null;

        return Results.Ok(userDto);
    }

    /// <summary>
    /// Get all users if you are admin.
    /// </summary>
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
            UserDto userDto = user.ToFullDto();
            userDto.AuthTokens = null;
            userDtos[i] = userDto;
        }

        return Results.Ok(userDtos);
    }
}
