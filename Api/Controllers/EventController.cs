namespace BirdWatching.Api.Controllers
{
    using BirdWatching.Shared.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
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
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventDto eventDto)
        {
            if (eventDto == null)
                return BadRequest("Event data must be provided.");

            // ID aktuálního uživatele z claimu 'sub'
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            try
            {
                var e = eventDto.ToEntity();
                e.MainAdminId = userId;

                // unikátní public identifier
                var existing = await _eventRepo.GetAllPublicIdentifiersAsync();
                string pubId;
                do
                {
                    pubId = GenerateUrlSafeString(5);
                } while (existing.ContainsKey(pubId));
                e.PublicIdentifier = pubId;

                await _eventRepo.AddAsync(e);
                return Ok(e.ToFullDto());
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
        public async Task<IActionResult> GetAll()
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
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid event ID.");
            var e = await _eventRepo.GetByIdAsync(id);
            if (e == null) return NotFound("Event not found.");
            return Ok(e.ToFullDto());
        }

        /// <summary>
        /// Get event by public identifier.
        /// </summary>
        [HttpGet("public/{publicId}")]
        public async Task<IActionResult> GetByPublicId(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return BadRequest("Public identifier must be provided.");

            var e = await _eventRepo.GetByPublicIdAsync(publicId);
            if (e == null) return NotFound("Event not found.");
            return Ok(e.ToFullDto());
        }

        /// <summary>
        /// Get events administered by a given user.
        /// </summary>
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            if (userId <= 0) return BadRequest("Invalid user ID.");
            var list = await _eventRepo.GetByUserIdAsync(userId);
            var dtos = list.Select(ev => {
                var dto = ev.ToFullDto();
                return dto ?? new EventDto { Id = ev.Id, Name = "(deleted)" };
            }).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Get events a watcher is participating in.
        /// </summary>
        [HttpGet("watcher/{watcherId:int}")]
        public async Task<IActionResult> GetByWatcherId(int watcherId)
        {
            if (watcherId <= 0) return BadRequest("Invalid watcher ID.");
            var list = await _eventRepo.GetByWatcherIdAsync(watcherId);
            var dtos = list.Select(ev => {
                var dto = ev.ToFullDto();
                return dto ?? new EventDto { Id = ev.Id, Name = "(deleted)" };
            }).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Update an existing event (only its MainAdmin or Admin role).
        /// </summary>
        [HttpPatch("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EventDto dto)
        {
            if (dto == null) return BadRequest("Event data must be provided.");
            if (id <= 0) return BadRequest("Invalid event ID.");

            var e = await _eventRepo.GetByIdAsync(id);
            if (e == null) return NotFound("Event not found.");

            // ID uživatele z claimu 'sub'
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // kontrola vlastníka nebo role Admin
            var isAdmin = User.IsInRole("Admin");
            if (e.MainAdminId != userId && !isAdmin)
                return Forbid();

            // aktualizace polí
            e.Name = dto.Name ?? e.Name;
            e.Start = dto.Start;
            e.End = dto.End;
            e.AllowDuplicates = dto.AllowDuplicates;
            e.GenusRegex = dto.GenusRegex;
            e.SpeciesRegex = dto.SpeciesRegex;

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
        [HttpGet("participants/{eventId:int}")]
        public async Task<IActionResult> GetWatchers(int eventId)
        {
            if (eventId <= 0) return BadRequest("Invalid event ID.");
            var list = await _eventRepo.GetParticipantsAsync(eventId);
            var dtos = list?.Select(w => w.ToFullDto()).ToList();
            return Ok(dtos);
        }
    }
}
