using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BirdWatching.Shared.Model;

namespace BirdWatching.Api.Controllers;

[ApiController]
[Route("api/watcher")]
public class WatcherController : BaseApiController
{
    private readonly ILogger<WatcherController> _logger;

    public WatcherController(AppDbContext context, ILogger<WatcherController> logger)
    {
        _context = context;
        _logger = logger;
        InitRepos__ContextMustNotBeNull();
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId != null ? await _userRepo.GetByIdAsync(int.Parse(userId)) : null;
    }

    private bool IsAdmin => User.IsInRole("admin");

    [HttpPost]
    public async Task<IActionResult> CreateWatcher([FromBody] WatcherDto watcherDto)
    {
        if (watcherDto == null)
            return BadRequest("Watcher data must be provided.");

        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized("User not found.");

        try
        {
            var watcher = watcherDto.ToEntity();
            watcher.MainCuratorId = user.Id;
            watcher.Curators.Add(user);

            // Generate unique public identifier
            string id;
            do // while id already exists, regenerate
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

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllIfAdmin()
    {
        var watchers = (await _watcherRepo.GetAllAsync())
            .Select(w => w.ToFullDto())
            .ToList();

        return Ok(watchers);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserWatchers()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized("User not found.");

        var watchers = (await _watcherRepo.GetByUserAsync(user))
            .Select(w => w.ToFullDto())
            .ToList();

        return Ok(watchers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid watcher ID.");

        var watcher = await _watcherRepo.GetByIdAsync(id);
        if (watcher == null)
            return NotFound("Watcher not found.");

        return Ok(watcher.ToFullDto());
    }

    [HttpPost("join/{eventPublicId}")]
    public async Task<IActionResult> JoinEvent(int watcherId, string eventPublicId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized("User not found.");

        var watcher = await _watcherRepo.GetByIdAsync(watcherId);
        if (watcher == null)
            return NotFound("Watcher not found.");
        if (!watcher.Curators.Contains(user))
            return Forbid("You do not have permission to modify this watcher.");

        var e = await _eventRepo.GetByPublicIdAsync(eventPublicId);
        if (e == null)
            return NotFound("Event not found.");

        try
        {
            if (watcher.Participating.Any(ev => ev.Id == e.Id))
                return Conflict("Watcher is already participating in this event.");

            watcher.Participating.Add(e);
            await _watcherRepo.UpdateAsync(watcher);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding watcher {WatcherId} to event {EventId}.", watcherId, e.Id);
            return Problem("Failed to join event.");
        }
    }
}
