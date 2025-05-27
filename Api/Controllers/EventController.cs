namespace BirdWatching.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using BirdWatching.Shared.Model;

    [ApiController]
    [Route("api/[controller]")]
    public class EventController : BaseApiController
    {
        private readonly ILogger<EventController> _logger;

        public EventController(AppDbContext context, ILogger<EventController> logger)
        {
            _context = context;
            _logger = logger;
            Init();
        }

        /// <summary>
        /// Create a new event. Current user becomes MainAdmin.
        /// </summary>
        [HttpPost("Create/{token}")]
        public IActionResult Create(string token, [FromBody] EventDto eventDto)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token is required.");
            if (eventDto == null)
                return BadRequest("Event data must be provided.");

            var auth = AuthUserByToken(token);
            if (!IsAuthorized(auth.Result))
                return Unauthorized("Invalid or expired token.");

            var user = auth.User!;

            try
            {
                var e = eventDto.ToEntity();
                e.MainAdminId = user.Id;

                // Generate unique public identifier
                var existing = _eventRepo.GetAllPublicIdentifiers();
                string pubId;
                do
                {
                    pubId = GenerateUrlSafeString(5);
                } while (existing.ContainsKey(pubId));
                e.PublicIdentifier = pubId;

                _eventRepo.Add(e);

                var createdDto = e.ToFullDto();
                return Ok(createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event for user {UserId}.", user.Id);
                return Problem("An error occurred while creating the event.");
            }
        }

        /// <summary>
        /// Get all public events.
        /// </summary>
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var events = _eventRepo.GetAll()?.ToList();
            if (events == null || !events.Any())
                return Ok(new EventDto[0]);

            var dtos = events.Select(e => e.ToFullDto()).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Get event by internal ID.
        /// </summary>
        [HttpGet("Get/{id}")]
        public IActionResult GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid event ID.");

            var e = _eventRepo.GetById(id);
            if (e == null)
                return NotFound("Event not found.");

            return Ok(e.ToFullDto());
        }

        /// <summary>
        /// Get event by public identifier.
        /// </summary>
        [HttpGet("GetByPublicId/{publicId}")]
        public IActionResult GetByPublicId(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return BadRequest("Public identifier must be provided.");

            var e = _eventRepo.GetByPublicId(publicId);
            if (e == null)
                return NotFound("Event not found.");

            return Ok(e.ToFullDto());
        }

        /// <summary>
        /// Get events administered by a given user.
        /// </summary>
        [HttpGet("GetByUserId/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid user ID.");

            var user = _userRepo.GetById(userId);
            if (user == null)
                return NotFound("User not found.");

            // THIS SHOULD BE UPDATED AND AVAILABLE FROM EVENTREPO
            var events = user.Events ?? Enumerable.Empty<Event>();
            var dtos = events.Select(ev => {
                var full = _eventRepo.GetById(ev.Id)?.ToFullDto();
                return full ?? new EventDto { Id = ev.Id, Name = "(deleted)" };
            })
                .ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Get events a watcher is participating in.
        /// </summary>
        [HttpGet("GetByWatcherId/{watcherId}")]
        public IActionResult GetByWatcherId(int watcherId)
        {
            if (watcherId <= 0)
                return BadRequest("Invalid watcher ID.");

            var w = _watcherRepo.GetById(watcherId);
            if (w == null)
                return NotFound("Watcher not found.");

            // THIS SHOULD BE UPDATED AND AVAILABLE FROM EVENTREPO
            var events = w.Participating ?? Enumerable.Empty<Event>();
            var dtos = events.Select(ev => {
                var full = _eventRepo.GetById(ev.Id)?.ToFullDto();
                return full ?? new EventDto { Id = ev.Id, Name = "(deleted)" };
            })
                .ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Update an existing event (only its MainAdmin).
        /// </summary>
        [HttpPatch("Update/{token}/{id}")]
        public IActionResult Update(string token, int id, [FromBody] EventDto dto)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token required.");
            if (dto == null)
                return BadRequest("Event data must be provided.");
            if (id <= 0)
                return BadRequest("Invalid event ID.");

            var auth = AuthUserByToken(token);
            if (!IsAuthorized(auth.Result))
                return Unauthorized("Invalid or expired token.");

            var user = auth.User!;
            var existing = _eventRepo.GetById(id);
            if (existing == null)
                return NotFound("Event not found.");
            if (existing.MainAdminId != user.Id)
                return Forbid("You are not the administrator of this event.");

            try
            {
                // Map only updatable fields
                existing.Name = dto.Name ?? existing.Name;
                existing.Start = dto.Start;
                existing.End = dto.End;
                existing.AllowDuplicates = dto.AllowDuplicates;
                existing.GenusRegex = dto.GenusRegex;
                existing.SpeciesRegex = dto.SpeciesRegex;

                _eventRepo.Update(existing);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event {EventId}.", id);
                return Problem("Failed to update event.");
            }
        }

        /// <summary>
        /// Get participants (watchers) of an event.
        /// </summary>
        [HttpGet("Participants/{eventId}")]
        public IActionResult GetWatchers(int eventId)
        {
            if (eventId <= 0)
                return BadRequest("Invalid event ID.");

            var watchers = _eventRepo.GetParticipants(eventId);
            if (watchers == null)
                return NotFound("Event not found or has no participants.");

            var dtos = watchers.Select(w => w.ToFullDto()).ToList();
            return Ok(dtos);
        }

        // Helper to interpret Auth*ByToken
        private static bool IsAuthorized(IActionResult result) =>
            result is OkResult or OkObjectResult;
    }
}
