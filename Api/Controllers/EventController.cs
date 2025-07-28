namespace BirdWatching.Api.Controllers
{
    using BirdWatching.Shared.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using NSwag.Annotations;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/event")]
    public class EventController : BaseApiController
    {
        private readonly ILogger<EventController> _logger;

        public EventController(AppDbContext context, ILogger<EventController> logger)
        {
            _context = context;
            _logger = logger;
            InitRepos__ContextMustNotBeNull();
        }

        /// <summary>
        /// Create a new event. Current user becomes MainAdmin.
        /// </summary>
        [Authorize]
        [OpenApiOperation("Event_Create")]
        [ProducesResponseType(typeof(EventDto), 201)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] EventDto eventDto)
        {
            if (eventDto == null || string.IsNullOrWhiteSpace(eventDto.Name))
                return BadRequest(new ProblemDetails { Title = "Invalid Input", Detail = "Event name is required." });

            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "JWT token is missing or invalid." });

            try
            {
                var e = eventDto.ToEntity();
                e.MainAdminId = userId.Value;
                e.MainAdmin = (await _userRepo.GetByIdAsync(userId.Value))!;

                string pubId;
                do
                {
                    pubId = GenerateUrlSafeString(5);
                } while (await _eventRepo.GetByPublicIdAsync(pubId) != null);
                e.PublicIdentifier = pubId;

                await _eventRepo.AddAsync(e);
                return CreatedAtAction(nameof(GetEventById), new { id = e.Id }, e.ToFullDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event for user {UserId}.", userId);
                return Problem("An error occurred while creating the event.");
            }
        }

        /// <summary>
        /// Get all public events.
        /// </summary>
        [HttpGet("all")]
        [AllowAnonymous]
        [OpenApiOperation("Event_GetAll")]
        [ProducesResponseType(typeof(List<EventDto>), 200)]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = (await _eventRepo.GetAllAsync()).ToList();
            var dtos = events.Select(e => e.ToFullDto()).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Get event by internal ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [OpenApiOperation("Event_GetById")]
        [ProducesResponseType(typeof(EventDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetEventById(int id)
        {
            if (id <= 0)
                return BadRequest(new ProblemDetails { Title = "Invalid ID", Detail = "Event ID must be a positive integer." });

            var e = await _eventRepo.GetByIdAsync(id);
            if (e == null)
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Event with ID {id} does not exist." });

            return Ok(e.ToFullDto());
        }

        /// <summary>
        /// Get event by public identifier.
        /// </summary>
        [HttpGet("public/{publicId}")]
        [AllowAnonymous]
        [OpenApiOperation("Event_GetByPublicId")]
        [ProducesResponseType(typeof(EventDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetEventByPublicId(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return BadRequest(new ProblemDetails { Title = "Invalid ID", Detail = "Public identifier must be provided." });

            var e = await _eventRepo.GetByPublicIdAsync(publicId);
            if (e == null)
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"No event found with public ID '{publicId}'." });

            return Ok(e.ToFullDto());
        }

        /// <summary>
        /// Get events administered by a given user.
        /// </summary>
        [Authorize]
        [HttpGet("user/{userId:int}")]
        [OpenApiOperation("Event_GetByUserId")]
        [ProducesResponseType(typeof(List<EventDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetEventsByUserId(int userId)
        {
            if (userId <= 0 && !(DEBUG && userId == -1))
                return BadRequest(new ProblemDetails { Title = "Invalid ID", Detail = "User ID must be a positive integer." });

            var list = await _eventRepo.GetByUserIdAsync(userId);
            var dtos = list.Select(ev => ev.ToFullDto() ?? new EventDto { Id = ev.Id, Name = "(deleted)" }).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Get events a watcher is participating in.
        /// </summary>
        [Authorize]
        [HttpGet("watcher/{watcherId:int}")]
        [OpenApiOperation("Event_GetByWatcherId")]
        [ProducesResponseType(typeof(List<EventDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetEventsByWatcherId(int watcherId)
        {
            if (watcherId <= 0)
                return BadRequest(new ProblemDetails { Title = "Invalid ID", Detail = "Watcher ID must be a positive integer." });

            var list = await _eventRepo.GetByWatcherIdAsync(watcherId);
            var dtos = list.Select(ev => ev.ToFullDto() ?? new EventDto { Id = ev.Id, Name = "(deleted)" }).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Update an existing event (only its MainAdmin or Admin role).
        /// </summary>
        [Authorize]
        [HttpPatch("update/{id:int}")]
        [OpenApiOperation("Event_Update")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventDto dto)
        {
            if (dto == null) return BadRequest("Event data must be provided.");
            if (id <= 0) return BadRequest("Invalid event ID.");

            var e = await _eventRepo.GetByIdAsync(id);
            if (e == null) return NotFound("Event not found.");

            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            if (e.MainAdminId != userId && !isAdmin)
                return Forbid();

            e.Name = dto.Name ?? e.Name;
            e.Start = dto.Start;
            e.End = dto.End;
            e.AllowDuplicates = dto.AllowDuplicates;
            e.GenusRegex = dto.GenusRegex;
            e.SpeciesRegex = dto.SpeciesRegex;
            e.IsPublic = dto.IsPublic;

            try
            {
                await _eventRepo.UpdateAsync(e);
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
        [Authorize]
        [HttpGet("participants/{eventId:int}")]
        [OpenApiOperation("Event_GetWatchers")]
        [ProducesResponseType(typeof(List<WatcherDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetWatchersForEvent(int eventId)
        {
            if (eventId <= 0)
                return BadRequest(new ProblemDetails { Title = "Invalid ID", Detail = "Event ID must be a positive integer." });

            var list = await _eventRepo.GetParticipantsAsync(eventId);
            var dtos = list?.Select(w => w.ToFullDto()).ToList() ?? new List<WatcherDto>();
            return Ok(dtos);
        }

        [HttpPatch("toggle-valid")]
        [OpenApiOperation("Event_ToggleRecordValidity")]
        public async Task<IActionResult> ToggleRecordValidity(int eventId, int recordId)
        {
            try
            {
                Event e = (await _eventRepo.GetByIdAsync(eventId))!;
                Record r = (await _recordRepo.GetByIdAsync(recordId))!;
                if (!e.NotValidRecords.Remove(r))
                {
                    e.NotValidRecords.Add(r);
                }
                await _eventRepo.UpdateAsync(e);
                return Ok();
            }
            catch
            {
                return Problem("Failed to find event or record.");
            }
        }
    }
}
