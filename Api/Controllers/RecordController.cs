namespace BirdWatching.Api.Controllers
{
    using BirdWatching.Shared.Model;
    // using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/record")]
    public class RecordController : BaseApiController
    {
        private readonly ILogger<RecordController> _logger;

        public RecordController(AppDbContext context, ILogger<RecordController> logger)
        {
            _context = context;
            _logger = logger;
            InitRepos__ContextMustNotBeNull();
        }

        /// <summary>
        /// Create a new record under a watcher.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RecordDto recordDto)
        {
            if (recordDto == null)
                return BadRequest("Record data must be provided.");

            // získáme přihlášeného uživatele z claimu NameIdentifier
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // validujeme existence entit
            var bird = await _birdRepo.GetByIdAsync(recordDto.BirdId);
            if (bird == null)
                return NotFound("Bird not found.");

            var watcher = await _watcherRepo.GetByIdAsync(recordDto.WatcherId);
            if (watcher == null)
                return NotFound("Watcher not found.");

            // kontrola, že přihlášený uživatel je jedním z kurátorů
            if (!watcher.Curators.Any(u => u.Id == userId) && !User.IsInRole("Admin"))
                return Forbid("You do not have permission to add records to this watcher.");

            var record = recordDto.ToEntity();
            record.Bird = bird;
            record.Watcher = watcher;

            try
            {
                await _recordRepo.AddAsync(record);
                return Ok(record.ToFullDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating record for watcher {WatcherId}.", watcher.Id);
                return Problem("An error occurred while creating the record.");
            }
        }

        /// <summary>
        /// Retrieve all records.
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var records = await _recordRepo.GetAllAsync() ?? Enumerable.Empty<Record>();
            return Ok(records.Select(r => r.ToFullDto()));
        }

        /// <summary>
        /// Retrieve a record by its ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid record ID.");

            var record = await _recordRepo.GetByIdAsync(id);
            if (record == null) return NotFound("Record not found.");

            return Ok(record.ToFullDto());
        }

        /// <summary>
        /// Retrieve all records for a specific watcher.
        /// </summary>
        [HttpGet("watcher/{watcherId:int}")]
        public async Task<IActionResult> GetByWatcher(int watcherId)
        {
            if (watcherId <= 0) return BadRequest("Invalid watcher ID.");

            var watcher = await _watcherRepo.GetByIdAsync(watcherId);
            if (watcher == null) return NotFound("Watcher not found.");

            var records = await _recordRepo.GetWatcherRecordsAsync(watcherId)
                              ?? Enumerable.Empty<Record>();
            return Ok(records.Select(r => r.ToFullDto()));
        }

        /// <summary>
        /// Append text to an existing record's comment (any authenticated user).
        /// </summary>
        [HttpPatch("append/{recordId:int}")]
        public async Task<IActionResult> AppendComment(int recordId, [FromBody] string additionalText)
        {
            if (recordId <= 0 || string.IsNullOrEmpty(additionalText))
                return BadRequest("Record ID and text must be provided.");

            var record = await _recordRepo.GetByIdAsync(recordId);
            if (record == null) return NotFound("Record not found.");

            record.Comment += additionalText;
            try
            {
                await _recordRepo.UpdateAsync(record);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error appending comment for record {RecordId}.", recordId);
                return Problem("Failed to append comment.");
            }
        }

        /// <summary>
        /// Edit a record's comment (owner or admin).
        /// </summary>
        [HttpPatch("edit/{recordId:int}")]
        public async Task<IActionResult> EditComment(int recordId, [FromBody] string newComment)
        {
            if (recordId <= 0 || newComment == null)
                return BadRequest("Record ID and new comment must be provided.");

            var record = await _recordRepo.GetByIdAsync(recordId);
            if (record == null) return NotFound("Record not found.");

            // získáme aktuálního uživatele
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // majitel záznamu = watcher.MainCurator? Nebo watcher.Curators
            var isOwner = record.Watcher.Curators.Any(u => u.Id == userId);
            if (!isOwner && !User.IsInRole("Admin"))
                return Forbid("You do not have permission to edit this record.");

            record.Comment = newComment;
            try
            {
                await _recordRepo.UpdateAsync(record);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing comment for record {RecordId}.", recordId);
                return Problem("Failed to edit comment.");
            }
        }
    }
}
