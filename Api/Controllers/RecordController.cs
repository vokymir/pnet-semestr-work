namespace BirdWatching.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using BirdWatching.Shared.Model;

    [ApiController]
    [Route("api/[controller]")]
    public class RecordController : BaseApiController
    {
        private readonly ILogger<RecordController> _logger;

        public RecordController(AppDbContext context, ILogger<RecordController> logger)
        {
            _context = context;
            _logger = logger;
            Init();
        }

        /// <summary>
        /// Create a new record under a watcher.
        /// </summary>
        [HttpPost("Create/{token}")]
        public IActionResult Create(string token, [FromBody] RecordDto recordDto)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token is required.");
            if (recordDto == null)
                return BadRequest("Record data must be provided.");

            var auth = AuthUserByToken(token);
            if (!IsAuthorized(auth.Result))
                return Unauthorized("Invalid or expired token.");

            var bird = _birdRepo.GetById(recordDto.BirdId);
            if (bird == null)
                return NotFound("Bird not found.");

            var watcher = _watcherRepo.GetById(recordDto.WatcherId);
            if (watcher == null)
                return NotFound("Watcher not found.");
            if (!watcher.Curators.Contains(auth.User!))
                return Forbid("You do not have permission to add records to this watcher.");

            var record = recordDto.ToEntity();
            record.Bird = bird;
            record.Watcher = watcher;

            try
            {
                _recordRepo.Add(record);
                var createdDto = record.ToFullDto();
                return Ok(createdDto);
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
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var records = _recordRepo.GetAll() ?? Enumerable.Empty<Record>();
            var dtos = records.Select(r => r.ToFullDto()).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Retrieve a record by its ID.
        /// </summary>
        [HttpGet("GetById/{recordId}")]
        public IActionResult GetById(int recordId)
        {
            if (recordId <= 0)
                return BadRequest("Invalid record ID.");

            var record = _recordRepo.GetById(recordId);
            if (record == null)
                return NotFound("Record not found.");

            return Ok(record.ToFullDto());
        }

        /// <summary>
        /// Retrieve all records for a specific watcher.
        /// </summary>
        [HttpGet("GetByWatcher/{watcherId}")]
        public IActionResult GetByWatcher(int watcherId)
        {
            if (watcherId <= 0)
                return BadRequest("Invalid watcher ID.");

            var watcher = _watcherRepo.GetById(watcherId);
            if (watcher == null)
                return NotFound("Watcher not found.");

            var records = _recordRepo.GetWatcherRecords(watcherId) ?? Enumerable.Empty<Record>();
            var dtos = records.Select(r => r.ToFullDto()).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Append text to an existing record's comment.
        /// </summary>
        [HttpPatch("AppendComment/{recordId}")]
        public IActionResult AppendComment(int recordId, [FromBody] string additionalText)
        {
            if (recordId <= 0 || string.IsNullOrEmpty(additionalText))
                return BadRequest("Record ID and text must be provided.");

            var record = _recordRepo.GetById(recordId);
            if (record == null)
                return NotFound("Record not found.");

            record.Comment += additionalText;
            try
            {
                _recordRepo.Update(record);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error appending comment for record {RecordId}.", recordId);
                return Problem("Failed to append comment.");
            }
        }

        /// <summary>
        /// Edit a record's comment (owner or admin only).
        /// </summary>
        [HttpPatch("EditComment/{token}/{recordId}")]
        public IActionResult EditComment(string token, int recordId, [FromBody] string newComment)
        {
            if (string.IsNullOrWhiteSpace(token) || recordId <= 0 || newComment == null)
                return BadRequest("Token, record ID, and new comment must be provided.");

            var record = _recordRepo.GetById(recordId);
            if (record == null)
                return NotFound("Record not found.");

            var auth = AuthUserByToken(token);
            if (!IsAuthorized(auth.Result))
            {
                var adminAuth = AuthAdminByToken(token);
                if (!IsAuthorized(adminAuth))
                    return Unauthorized("Access denied.");
            }
            else if (!auth.User!.Watchers.Contains(record.Watcher))
            {
                return Forbid("You do not have permission to edit this record.");
            }

            record.Comment = newComment;
            try
            {
                _recordRepo.Update(record);
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
