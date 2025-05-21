namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

using BirdWatching.Shared.Model;

/// <summary>
/// Can do:
/// -
/// Should do:
/// - create/update/get event
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
}
