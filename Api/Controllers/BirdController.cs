namespace BirdWatching.Api.Controllers
{
    using BirdWatching.Shared.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Linq;
    using System.Threading.Tasks;
    using Swashbuckle.AspNetCore.Annotations;

    [ApiController]
    [Route("api/bird")]
    public class BirdController : BaseApiController
    {
        private readonly ILogger<BirdController> _logger;

        public BirdController(AppDbContext context, ILogger<BirdController> logger)
        {
            _context = context;
            _logger = logger;
            InitRepos__ContextMustNotBeNull();
        }

        /// <summary>
        /// Create a new bird.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BirdDto dto)
        {
            if (dto == null)
                return BadRequest("Bird data must be provided.");

            try
            {
                var bird = dto.ToEntity();
                await _birdRepo.AddAsync(bird);
                return Ok(bird.ToFullDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bird.");
                return Problem("An error occurred while creating the bird.");
            }
        }

        /// <summary>
        /// Get all birds.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(OperationId = "AllBirdsAsync")]
        [ProducesResponseType(typeof(BirdDto[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var birds = await _birdRepo.GetAllAsync() ?? Enumerable.Empty<Bird>();
            return Ok(birds.Select(b => b.ToFullDto()));
        }

        /// <summary>
        /// Search birds by prefix.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> GetByPrefix([FromQuery] string prefix)
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
        /// Get a bird by ID.
        /// </summary>
        [HttpGet("{id:int}")]
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
        /// Append text to a bird's comment.
        /// </summary>
        [HttpPatch("comment/append/{birdId:int}")]
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
        [HttpPatch("comment/replace/{birdId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditComment(int birdId, [FromBody] string newComment)
        {
            if (birdId <= 0 || newComment == null)
                return BadRequest("Bird ID and new comment must be provided.");

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
                _logger.LogError(ex, "Error replacing comment for bird {BirdId}.", birdId);
                return Problem("Failed to replace comment.");
            }
        }
    }
}
