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
    public IActionResult Create(string token, EventDto eventDto)
    {
        var response = AuthUserByToken(token);
        if (!response.Result.Equals(Ok()))
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
            return Problem(ex.Message);
        }
        return Ok();
    }

    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
        var es = _eventRepo.GetAll();
        if (es is null)
            return NotFound();

        List<EventDto> eds = new();
        foreach (var e in es)
            eds.Add(e.ToFullDto());

        return Ok(eds);
    }

    [HttpGet("Get/{id}")]
    public IActionResult Get(int id)
    {
        Event? e = _eventRepo.GetById(id);
        if (e is null)
            return NotFound();

        EventDto eDto = e.ToFullDto();
        return Ok(eDto);
    }

    [HttpGet("GetByPublicId/{id}")]
    public IActionResult GetByPublicId(string id)
    {
        Event? e = _eventRepo.GetByPublicId(id);
        if (e is null)
            return NotFound();

        EventDto eDto = e.ToFullDto();
        return Ok(eDto);
    }

    [HttpGet("GetByUserId/{userId}")]
    public IActionResult GetByUserId(int userId)
    {
        var usr = _userRepo.GetById(userId);
        if (usr is null) return NotFound("User not found...");

        var evs = usr.Events;
        if (evs is null) return NotFound("User have no events...");

        List<EventDto> evds = new();
        foreach (var e in evs)
            evds.Add(_eventRepo.GetById(e.Id)?.ToFullDto() ?? new EventDto() { Name = "CHYBA" });
        // can be optimized in making just one SQL call, in eventRepo... the same with following method

        return Ok(evds);
    }

    [HttpGet("GetByWatcherId/{watcherId}")]
    public IActionResult GetByWatcherId(int watcherId)
    {
        var w = _watcherRepo.GetById(watcherId);
        if (w is null) return NotFound("Watcher not found");

        var evs = w.Participating;

        List<EventDto> evds = new();
        foreach (var e in evs)
            evds.Add(_eventRepo.GetById(e.Id)?.ToFullDto() ?? new EventDto() { Name = "CHYBA" });

        return Ok(evds);
    }

    [HttpPatch("Update/{token}/{id}")]
    public IActionResult Update(string token, int id, EventDto eDto)
    {
        var response = AuthUserByToken(token);
        if (!response.Result.Equals(Ok()))
            return response.Result;

        Event? e = _eventRepo.GetById(id);
        if (e is null || response.User is null) return NotFound();
        if (e.MainAdminId != response.User.Id) return Problem("You do not own this event and therefore cannot make changes.");

        Event updatedEvent = eDto.ToEntity();
        e = updatedEvent;
        try
        {
            _eventRepo.Update(e);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }

        return Ok();
    }

    [HttpGet("Participants/{eventId}")]
    public IActionResult GetWatchers(int eventId)
    {
        var ws = _eventRepo.GetParticipants(eventId);
        if (ws is null) return NotFound();

        List<WatcherDto> wsd = new();
        foreach (var w in ws) wsd.Add(w.ToFullDto());

        return Ok(wsd);
    }
}
