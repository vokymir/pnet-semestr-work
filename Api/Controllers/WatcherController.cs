namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using BirdWatching.Shared.Model;

/// <summary>
/// Can do the following:
/// - Create watcher and bind it to current user.
/// - Get all watchers belonging to user (or all if admin).
/// - join event
/// - Add/remove curators??? = done in users
/// Should do:
/// - Update watcher.
/// - Change mainUser.
/// Also:
/// - participate in event + leave
/// </summary>
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

            var wStrings = _watcherRepo.GetAllPublicIdentifiers();
            string identificator;
            do
                identificator = GenerateUrlSafeString(8);
            while (wStrings.ContainsKey(identificator));
            watcher.PublicIdentifier = identificator;

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

    [HttpGet("Get")]
    public IResult GetById(int id)
    {
        var w = _watcherRepo.GetById(id);
        if (w is null) return Results.NotFound();

        WatcherDto wDto = w.ToFullDto();

        return Results.Ok(wDto);
    }

    [HttpPost("JoinEvent/{token}")]
    public IResult JoinEvent(string token, int watcherId, string eventPublicId)
    {
        var gettingUser = AuthUserByToken(token);
        if (!gettingUser.Result.Equals(Results.Ok())) return gettingUser.Result;
        User user = gettingUser.User!;

        Watcher? watcher = _watcherRepo.GetById(watcherId);
        if (watcher is null) return Results.NotFound("Watcher not found.");
        if (!watcher.Curators.Contains(user)) return Results.Problem("Don't have permission to edit watcher.");

        Event? e = _eventRepo.GetByPublicId(eventPublicId);
        if (e is null) return Results.NotFound("Event not found.");

        try
        {
            watcher.Participating.Add(e);
            _watcherRepo.Update(watcher);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }

        return Results.Ok();
    }
}
