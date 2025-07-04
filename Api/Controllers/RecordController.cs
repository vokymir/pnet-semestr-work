using BirdWatching.Shared.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BirdWatching.Api.Controllers
{
    [ApiController]
    [Route("api/record")]
    [Authorize]
    public class RecordController : BaseApiController
    {
        private readonly ILogger<RecordController> _logger;

        public RecordController(AppDbContext context, ILogger<RecordController> logger)
        {
            _context = context;
            _logger = logger;
            InitRepos__ContextMustNotBeNull();
        }

        /// <summary>Create a new record for a watcher</summary>
        [HttpPost]
        [ProducesResponseType(typeof(RecordDto), 201)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<IActionResult> Create([FromBody] RecordDto recordDto)
        {
            if (recordDto == null || !ModelState.IsValid)
                return BadRequest(new ProblemDetails { Title = "Invalid input", Detail = "Record data is required." });

            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "User not authenticated." });

            var bird = await _birdRepo.GetByIdAsync(recordDto.BirdId);
            if (bird == null)
                return NotFound(new ProblemDetails { Title = "Bird not found" });

            var watcher = await _watcherRepo.GetByIdAsync(recordDto.WatcherId);
            if (watcher == null)
                return NotFound(new ProblemDetails { Title = "Watcher not found" });

            var isCurator = watcher.Curators.Any(u => u.Id == userId);
            if (!isCurator && !User.IsInRole("Admin"))
                return Forbid();

            var record = recordDto.ToEntity();
            record.Bird = bird;
            record.Watcher = watcher;

            try
            {
                await _recordRepo.AddAsync(record);
                return CreatedAtAction(nameof(GetById), new { id = record.Id }, record.ToFullDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating record");
                return Problem("An error occurred while creating the record.");
            }
        }

        /// <summary>Get all records</summary>
        [HttpGet("all")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<RecordDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var records = await _recordRepo.GetAllAsync();
            return Ok(records.Select(r => r.ToFullDto()));
        }

        /// <summary>Get a record by ID</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RecordDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new ProblemDetails { Title = "Invalid ID" });

            var record = await _recordRepo.GetByIdAsync(id);
            if (record == null)
                return NotFound(new ProblemDetails { Title = "Record not found" });

            return Ok(record.ToFullDto());
        }

        /// <summary>Get records for a specific watcher</summary>
        [HttpGet("watcher/{watcherId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<RecordDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetByWatcher(int watcherId)
        {
            if (watcherId <= 0)
                return BadRequest(new ProblemDetails { Title = "Invalid Watcher ID" });

            var watcher = await _watcherRepo.GetByIdAsync(watcherId);
            if (watcher == null)
                return NotFound(new ProblemDetails { Title = "Watcher not found" });

            var records = await _recordRepo.GetWatcherRecordsAsync(watcherId);
            return Ok(records.Select(r => r.ToFullDto()));
        }

        /// <summary>Append text to a record's comment</summary>
        [HttpPatch("append/{recordId:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<IActionResult> AppendComment(int recordId, [FromBody] CommentUpdateDto dto)
        {
            if (recordId <= 0 || string.IsNullOrWhiteSpace(dto?.Text))
                return BadRequest(new ProblemDetails { Title = "Invalid input" });

            var record = await _recordRepo.GetByIdAsync(recordId);
            if (record == null)
                return NotFound(new ProblemDetails { Title = "Record not found" });

            record.Comment += dto.Text;

            try
            {
                await _recordRepo.UpdateAsync(record);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to append comment to record {RecordId}", recordId);
                return Problem("Error updating the record.");
            }
        }

        /// <summary>Edit comment of a record</summary>
        [HttpPatch("edit/{recordId:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<IActionResult> EditComment(int recordId, [FromBody] CommentUpdateDto dto)
        {
            if (recordId <= 0 || dto == null)
                return BadRequest(new ProblemDetails { Title = "Invalid input" });

            var record = await _recordRepo.GetByIdAsync(recordId);
            if (record == null)
                return NotFound(new ProblemDetails { Title = "Record not found" });

            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized(new ProblemDetails { Title = "Unauthorized" });

            var isCurator = record.Watcher.Curators.Any(u => u.Id == userId);
            if (!isCurator && !User.IsInRole("Admin"))
                return Forbid();

            record.Comment = dto.Text;

            try
            {
                await _recordRepo.UpdateAsync(record);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit comment for record {RecordId}", recordId);
                return Problem("Failed to update comment.");
            }
        }

        /// <summary>Update a full record</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<IActionResult> Update(int id, [FromBody] RecordDto dto)
        {
            if (id <= 0 || dto == null || id != dto.Id)
                return BadRequest(new ProblemDetails { Title = "Invalid input" });

            var record = await _recordRepo.GetByIdAsync(id);
            if (record == null)
                return NotFound(new ProblemDetails { Title = "Record not found" });

            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized(new ProblemDetails { Title = "Unauthorized" });

            var isCurator = record.Watcher.Curators.Any(u => u.Id == userId);
            if (!isCurator && !User.IsInRole("Admin"))
                return Forbid();

            // Update fields
            record.DateSeen = dto.DateSeen;
            record.Comment = dto.Comment;
            record.Latitude = dto.Latitude;
            record.Longitude = dto.Longitude;
            record.Accuracy = dto.Accuracy;
            record.LocationDescribed = dto.LocationDescribed;
            record.Count = dto.Count;

            if (dto.BirdId != record.Bird.Id)
            {
                var newBird = await _birdRepo.GetByIdAsync(dto.BirdId);
                if (newBird == null)
                    return NotFound(new ProblemDetails { Title = "Bird not found" });

                record.Bird = newBird;
            }

            try
            {
                await _recordRepo.UpdateAsync(record);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update record {RecordId}", id);
                return Problem("Failed to update the record.");
            }
        }

        /// <summary>Delete a record</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new ProblemDetails { Title = "Invalid ID" });

            var record = await _recordRepo.GetByIdAsync(id);
            if (record == null)
                return NotFound(new ProblemDetails { Title = "Record not found" });

            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized(new ProblemDetails { Title = "Unauthorized" });

            var isCurator = record.Watcher.Curators.Any(u => u.Id == userId);
            if (!isCurator && !User.IsInRole("Admin"))
                return Forbid();

            try
            {
                await _recordRepo.DeleteAsync(record.Id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete record {RecordId}", id);
                return Problem("Failed to delete the record.");
            }
        }
    }
}
