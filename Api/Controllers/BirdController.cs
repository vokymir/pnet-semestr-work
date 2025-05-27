namespace BirdWatching.Api.Controllers
{
    using BirdWatching.Shared.Model;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class BirdController : BaseApiController
    {
        private readonly ILogger<BirdController> _logger;

        public BirdController(AppDbContext context, ILogger<BirdController> logger)
        {
            _context = context;
            _logger = logger;
            Init();
        }

        /// <summary>
        /// Create a new bird (public).
        /// </summary>
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] BirdDto dto)
        {
            if (dto == null)
                return BadRequest("Bird data must be provided.");

            try
            {
                var bird = dto.ToEntity();
                await _birdRepo.AddAsync(bird);
                var createdDto = bird.ToFullDto();
                return Ok(createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bird.");
                return Problem("An error occurred while creating the bird.");
            }
        }

        /// <summary>
        /// Get all birds (public).
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var birds = await _birdRepo.GetAllAsync() ?? Enumerable.Empty<Bird>();
            var dtos = birds.Select(b => b.ToFullDto()).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Get birds whose full name starts with prefix (public).
        /// </summary>
        [HttpGet("GetByPrefix/{prefix}")]
        public async Task<IActionResult> GetByPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return BadRequest("Prefix must be provided.");

            var birds = await _birdRepo.GetByPrefixAsync(prefix) ?? Enumerable.Empty<Bird>();
            var dtos = birds.Select(b => b.ToFullDto()).ToList();
            return dtos.Any()
                ? Ok(dtos)
                : NotFound($"No birds found with prefix '{prefix}'.");
        }

        /// <summary>
        /// Get a bird by its ID (public).
        /// </summary>
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid bird ID.");

            var bird = await _birdRepo.GetByIdAsync(id);
            if (bird == null)
                return NotFound("Bird not found.");

            return Ok(bird.ToFullDto());
        }

        /// <summary>
        /// Append text to a bird's comment (public).
        /// </summary>
        [HttpPatch("AppendComment/{birdId}")]
        public async Task<IActionResult> AppendComment(int birdId, [FromBody] string additional)
        {
            if (birdId <= 0 || string.IsNullOrEmpty(additional))
                return BadRequest("Bird ID and comment text must be provided.");

            var bird = await _birdRepo.GetByIdAsync(birdId);
            if (bird == null)
                return NotFound("Bird not found.");

            bird.Comment += additional;

            try
            {
                await _birdRepo.UpdateAsync(bird);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error appending comment to bird {BirdId}.", birdId);
                return Problem("Failed to append comment.");
            }
        }

        /// <summary>
        /// Replace a bird's comment (admin only).
        /// </summary>
        [HttpPatch("EditComment/{token}/{birdId}")]
        public async Task<IActionResult> EditComment(string token, int birdId, [FromBody] string newComment)
        {
            if (string.IsNullOrWhiteSpace(token) || birdId <= 0 || newComment == null)
                return BadRequest("Token, bird ID, and new comment must be provided.");

            var auth = await AuthAdminByTokenAsync(token);
            if (!IsAuthorized(auth))
                return Unauthorized("Admin privileges required.");

            var bird = await _birdRepo.GetByIdAsync(birdId);
            if (bird == null)
                return NotFound("Bird not found.");

            bird.Comment = newComment;

            try
            {
                await _birdRepo.UpdateAsync(bird);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing comment for bird {BirdId}.", birdId);
                return Problem("Failed to edit comment.");
            }
        }
    }
}
