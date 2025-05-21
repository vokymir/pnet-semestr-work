namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

using BirdWatching.Shared.Model;

/// <summary>
/// Can do:
/// - create/get event (public)
/// - update (admin)
/// -
/// Should do:
/// - get all watchers and their bird-count of valid birds
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

    [HttpGet("Get")]
    public IResult Get(int id)
    {
        Event? e = _eventRepo.GetById(id);
        if (e is null)
            return Results.NotFound();

        EventDto eDto = e.ToFullDto();
        return Results.Ok(eDto);
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
}
