using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BirdWatching.Shared.Model;
using NSwag.Annotations;

namespace BirdWatching.Api.Controllers;

[ApiController]
[Route("api/watcher")]
public class WatcherController : BaseApiController
{
    private readonly ILogger<WatcherController> _logger;

    public WatcherController(AppDbContext context, ILogger<WatcherController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitRepos__ContextMustNotBeNull();
    }

    private bool IsAdmin => User.IsInRole("Admin");

    /// <summary>Creates a new watcher entity (user must be logged in).</summary>
    [HttpPost]
    [Authorize]
    [OpenApiOperation("Watcher_Create", "Creates a new watcher and assigns current user as main curator.")]
    [ProducesResponseType(typeof(WatcherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateWatcher([FromBody] WatcherDto watcherDto)
    {
        if (watcherDto == null)
            return BadRequest(new ProblemDetails { Title = "Invalid Input", Detail = "Watcher data must be provided." });

        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "JWT token is missing or invalid." });

        var user = await _userRepo.GetByIdAsync(userId.Value);
        if (user == null)
            return Problem("Cannot find user.");

        try
        {
            var watcher = watcherDto.ToEntity();
            watcher.MainCuratorId = user.Id;
            watcher.Curators.Add(user);

            string id;
            do
            {
                id = GenerateUrlSafeString(5);
            } while (await _watcherRepo.GetByPublicIdAsync(id) != null);

            watcher.PublicIdentifier = id;

            await _watcherRepo.AddAsync(watcher);
            return Ok(watcher.ToFullDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating watcher for user {UserId}.", user.Id);
            return Problem("An error occurred while creating the watcher.");
        }
    }

    /// <summary>Returns all watchers (admin only).</summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [OpenApiOperation("Watcher_GetAll", "Returns all watchers (admin only).")]
    [ProducesResponseType(typeof(List<WatcherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllIfAdmin()
    {
        var watchers = (await _watcherRepo.GetAllAsync())
            .Select(w => w.ToFullDto())
            .ToList();

        return Ok(watchers);
    }

    /// <summary>Returns all watchers where current user is a curator.</summary>
    [HttpGet]
    [Authorize]
    [OpenApiOperation("Watcher_GetByUserCurrent", "Returns all watchers associated with current user.")]
    [ProducesResponseType(typeof(List<WatcherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserWatchers()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "JWT token is missing or invalid." });

        var user = await _userRepo.GetByIdAsync(userId.Value);
        if (user == null)
            return Problem("Cannot find user.");

        var watchers = (await _watcherRepo.GetByUserAsync(user))
            .Select(w => w.ToFullDto())
            .ToList();

        return Ok(watchers);
    }

    /// <summary>Gets a single watcher by its ID. This endpoint is public.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [OpenApiOperation("Watcher_Get", "Returns a watcher by ID.")]
    [ProducesResponseType(typeof(WatcherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest(new ProblemDetails { Title = "Invalid ID", Detail = "Watcher ID must be greater than zero." });

        var watcher = await _watcherRepo.GetByIdAsync(id);
        if (watcher == null)
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Watcher with ID {id} does not exist." });

        return Ok(watcher.ToFullDto());
    }

    [HttpGet("leaderboard/{ePubId}/{wId:int}")]
    [AllowAnonymous]
    [OpenApiOperation("Watcher_GetForLeaderboard", "Returns a special watcherDTO by ID.")]
    [ProducesResponseType(typeof(WatcherOnLeaderboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAndEvent(int wId, string ePubId)
    {
        if (wId <= 0)
            return BadRequest(new ProblemDetails { Title = "Invalid watcher ID", Detail = "Watcher ID must be greater than zero." });
        if (string.IsNullOrWhiteSpace(ePubId))
            return BadRequest(new ProblemDetails { Title = "Invalid event public ID", Detail = "Event public ID mustn't be empty string." });

        WatcherOnLeaderboardDto w = new() {
            Id = wId,
            Name = "Nezjisteno",
            ValidRecords = new()
        };

        try
        {
            var x = await _recordRepo.GetValidEventsWatcherRecordsAsync(wId, ePubId);

            if (x.Count() > 0)
                foreach (var r in x)
                    w.ValidRecords.Add(r.ToFullDto());
        }
        catch (Exception ex)
        {
            return NotFound(new ProblemDetails { Title = "Something wrong", Detail = $"SQL call failed, details: {ex.Message}" });
        }

        return Ok(w);
    }

    /// <summary>Adds the specified watcher to the event's participants. Only curators can perform this.</summary>
    [HttpPost("join/{eventPublicId}")]
    [Authorize]
    [OpenApiOperation("Watcher_JoinEvent", "Adds the watcher to a public event (curators only).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> JoinEvent([FromQuery] int watcherId, string eventPublicId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "JWT token is missing or invalid." });

        var user = await _userRepo.GetByIdAsync(userId.Value);
        if (user == null)
            return Problem("Cannot find user.");

        var watcher = await _watcherRepo.GetByIdAsync(watcherId);
        if (watcher == null)
            return NotFound(new ProblemDetails { Title = "Watcher Not Found", Detail = $"Watcher with ID {watcherId} does not exist." });

        if (!watcher.Curators.Any(c => c.Id == user.Id))
            return StatusCode(403, new ProblemDetails { Title = "Forbidden", Detail = "You are not a curator of this watcher." });

        var ev = await _eventRepo.GetByPublicIdAsync(eventPublicId);
        if (ev == null)
            return NotFound(new ProblemDetails { Title = "Event Not Found", Detail = $"Event with public ID '{eventPublicId}' does not exist." });

        if (watcher.Participating.Any(e => e.Id == ev.Id))
            return Conflict(new ProblemDetails { Title = "Already Participating", Detail = "This watcher is already participating in the event." });

        try
        {
            watcher.Participating.Add(ev);
            await _watcherRepo.UpdateAsync(watcher);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding watcher {WatcherId} to event {EventId}.", watcherId, ev.Id);
            return Problem("Failed to join event.");
        }
    }
}
