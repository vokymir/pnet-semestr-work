namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using BirdWatching.Shared.Model;

[ApiController]
public class WatcherController : BaseApiController
{
    private readonly ILogger<WatcherController> _logger;

    public WatcherController(AppDbContext context, ILogger<WatcherController> logger)
    {
        _context = context;
        _logger = logger;
        Init();
    }

    /// <summary>
    /// Create a new watcher and assing it to current user.
    /// </summary>
    [HttpPost("Create/{token}")]
    public IResult CreateWatcher(string token, WatcherDto watcherDto)
    {
        var gettingUser = AuthUserByToken(token);
        if (!gettingUser.Result.Equals(Results.Ok())) return gettingUser.Result;
        User user = gettingUser.User!;

        try
        {
            var watcher = watcherDto.ToEntity();
            watcher.MainCuratorId = user.Id;
            watcher.Curators.Add(user);

            _watcherRepo.Add(watcher);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }

        return Results.Ok();
    }

    /// <summary>
    /// Get absolutely all watchers, if you are admin.
    /// </summary>
    [HttpGet("GetAll/{token}")]
    public IResult GetAllIfAdmin(string token)
    {
        var result = AuthAdminByToken(token);
        if (!result.Equals(Results.Ok())) return result;

        var watchers = _watcherRepo.GetAll();
        var watcherDtos = new List<WatcherDto>();
        foreach (var w in watchers)
            watcherDtos.Add(w.ToFullDto());

        return Results.Ok(watcherDtos);
    }

    /// <summary>
    /// Get all watchers a user can edit.
    /// </summary>
    [HttpGet("AllUserHave/{token}")]
    public IResult GetUserWatchers(string token)
    {
        var gettingUser = AuthUserByToken(token);
        if (!gettingUser.Result.Equals(Results.Ok())) return gettingUser.Result;
        User user = gettingUser.User!;

        Watcher[] watchers = _watcherRepo.GetByUser(user);
        var watcherDtos = new List<WatcherDto>();
        foreach (var w in watchers)
            watcherDtos.Add(w.ToFullDto());

        return Results.Ok(watcherDtos);
    }
}
