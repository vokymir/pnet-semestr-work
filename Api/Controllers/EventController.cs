namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

using BirdWatching.Shared.Model;

/// <summary>
/// Can do:
/// - create/get event (public)
/// - update (admin)
/// - get all watchers and their bird-count of valid birds
/// -
/// Should do:
/// </summary>
[ApiController]
public class EventController : BaseApiController
{
    private readonly ILogger<EventController> _logger;

    public EventController(AppDbContext context, ILogger<EventController> logger)
    {
        _context = context;
        _logger = logger;
        Init();
    }

    [HttpPost("Create/{token}")]
    public IResult Create(string token, EventDto eventDto)
    {
        var response = AuthUserByToken(token);
        if (!response.Result.Equals(Results.Ok()))
            return response.Result;

        Event e = eventDto.ToEntity();

        if (response.User is not null)
            e.MainAdminId = response.User.Id;

        // generate identifier
        var eStrings = _eventRepo.GetAllPublicIdentifiers();
        string identificator;
        do
            identificator = GenerateUrlSafeString(16);
        while (eStrings.ContainsKey(identificator));
        e.PublicIdentifier = identificator;

        try
        {
            _eventRepo.Add(e);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
        return Results.Ok();
    }

    [HttpGet("GetAll")]
    public IResult GetAll()
    {
        var es = _eventRepo.GetAll();
        if (es is null)
            return Results.NotFound();

        List<EventDto> eds = new();
        foreach (var e in es)
            eds.Add(e.ToFullDto());

        return Results.Ok(eds);
    }

    [HttpGet("Get/{id}")]
    public IResult Get(int id)
    {
        Event? e = _eventRepo.GetById(id);
        if (e is null)
            return Results.NotFound();

        EventDto eDto = e.ToFullDto();
        return Results.Ok(eDto);
    }

    [HttpGet("GetByUserId/{userId}")]
    public IResult GetByUserId(int userId)
    {
        var usr = _userRepo.GetById(userId);
        if (usr is null) return Results.NotFound("User not found...");

        var evs = usr.Events;
        if (evs is null) return Results.NotFound("User have no events...");

        List<EventDto> evds = new();
        foreach (var e in evs)
            evds.Add(_eventRepo.GetById(e.Id)?.ToFullDto() ?? new EventDto() { Name = "CHYBA" });
        // can be optimized in making just one SQL call, in eventRepo... the same with following method

        return Results.Ok(evds);
    }

    [HttpGet("GetByWatcherId/{watcherId}")]
    public IResult GetByWatcherId(int watcherId)
    {
        var w = _watcherRepo.GetById(watcherId);
        if (w is null) return Results.NotFound("Watcher not found");

        var evs = w.Participating;

        List<EventDto> evds = new();
        foreach (var e in evs)
            evds.Add(_eventRepo.GetById(e.Id)?.ToFullDto() ?? new EventDto() { Name = "CHYBA" });

        return Results.Ok(evds);
    }

    [HttpPatch("Update/{token}/{id}")]
    public IResult Update(string token, int id, EventDto eDto)
    {
        var response = AuthUserByToken(token);
        if (!response.Result.Equals(Results.Ok()))
            return response.Result;

        Event? e = _eventRepo.GetById(id);
        if (e is null || response.User is null) return Results.NotFound();
        if (e.MainAdminId != response.User.Id) return Results.Problem("You do not own this event and therefore cannot make changes.");

        Event updatedEvent = eDto.ToEntity();
        e = updatedEvent;
        try
        {
            _eventRepo.Update(e);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }

        return Results.Ok();
    }

    [HttpGet("Participants/{eventId}")]
    public IResult GetWatchers(int eventId)
    {
        var ws = _eventRepo.GetParticipants(eventId);
        if (ws is null) return Results.NotFound();

        List<WatcherDto> wsd = new();
        foreach (var w in ws) wsd.Add(w.ToFullDto());

        return Results.Ok(wsd);
    }
}
