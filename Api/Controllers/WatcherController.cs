namespace BirdWatching.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using BirdWatching.Shared.Model;
    using System.Threading.Tasks;
    using System.Linq;

    [ApiController]
    [Route("api/[controller]")]
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
        /// Create a new watcher and assign it to current user.
        /// </summary>
        [HttpPost("Create/{token}")]
        public async Task<IActionResult> CreateWatcher(string token, [FromBody] WatcherDto watcherDto)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token must be provided.");
            if (watcherDto == null)
                return BadRequest("Watcher data must be provided.");

            var auth = await AuthUserByTokenAsync(token);
            if (!IsAuthorized(auth.Result))
                return Unauthorized("Invalid or expired token.");

            var user = auth.User!;
            try
            {
                var watcher = watcherDto.ToEntity();
                watcher.MainCuratorId = user.Id;
                watcher.Curators.Add(user);

                // Generate unique public identifier
                var existing = await _watcherRepo.GetAllPublicIdentifiersAsync();
                string id;
                do
                {
                    id = GenerateUrlSafeString(5);
                } while (existing.ContainsKey(id));
                watcher.PublicIdentifier = id;

                await _watcherRepo.AddAsync(watcher);

                var resultDto = watcher.ToFullDto();
                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating watcher for user {UserId}.", user.Id);
                return Problem("An error occurred while creating the watcher.");
            }
        }

        /// <summary>
        /// Get absolutely all watchers, if admin.
        /// </summary>
        [HttpGet("GetAll/{token}")]
        public async Task<IActionResult> GetAllIfAdmin(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token must be provided.");

            var auth = await AuthAdminByTokenAsync(token);
            if (!IsAuthorized(auth))
                return Unauthorized("Admin privileges required.");

            var watchers = (await _watcherRepo.GetAllAsync())
                .Select(w => w.ToFullDto())
                .ToList();

            return Ok(watchers);
        }

        /// <summary>
        /// Get watchers that current user can edit.
        /// </summary>
        [HttpGet("AllUserHave/{token}")]
        public async Task<IActionResult> GetUserWatchers(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token must be provided.");

            var auth = await AuthUserByTokenAsync(token);
            if (!IsAuthorized(auth.Result))
                return Unauthorized("Invalid or expired token.");

            var user = auth.User!;
            var watchers = (await _watcherRepo.GetByUserAsync(user))
                .Select(w => w.ToFullDto())
                .ToList();

            return Ok(watchers);
        }

        /// <summary>
        /// Get watcher by ID.
        /// </summary>
        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid watcher ID.");

            var watcher = await _watcherRepo.GetByIdAsync(id);
            if (watcher == null)
                return NotFound("Watcher not found.");

            return Ok(watcher.ToFullDto());
        }

        /// <summary>
        /// Join a watcher to an event.
        /// </summary>
        [HttpPost("JoinEvent/{token}/{watcherId}/{eventPublicId}")]
        public async Task<IActionResult> JoinEvent(string token, int watcherId, string eventPublicId)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token must be provided.");

            var auth = await AuthUserByTokenAsync(token);
            if (!IsAuthorized(auth.Result))
                return Unauthorized("Invalid or expired token.");

            var user = auth.User!;
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
}
