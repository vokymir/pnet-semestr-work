namespace BirdWatching.Api.Controllers
{
    using BirdWatching.Shared.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using NSwag.Annotations;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/bird")]
    public class BirdController : BaseApiController
    {
        private readonly ILogger<BirdController> _logger;

        public BirdController(AppDbContext context, ILogger<BirdController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
            InitRepos__ContextMustNotBeNull();
        }

        /// <summary>Create a new bird.</summary>
        [HttpPost]
        [OpenApiOperation("Bird_Create")]
        [ProducesResponseType(typeof(BirdDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BirdDto dto)
        {
            if (dto == null)
                return BadRequest(new ProblemDetails { Title = "Invalid data", Detail = "Bird data must be provided." });

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

        /// <summary>Get all birds.</summary>
        [HttpGet]
        [OpenApiOperation("Bird_GetAll")]
        [ProducesResponseType(typeof(BirdDto[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var birds = await _birdRepo.GetAllAsync() ?? Enumerable.Empty<Bird>();
            return Ok(birds.Select(b => b.ToFullDto()));
        }

        /// <summary>Search birds by name prefix.</summary>
        [HttpGet("search")]
        [OpenApiOperation("Bird_GetByPrefix")]
        [ProducesResponseType(typeof(BirdDto[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByPrefix([FromQuery] string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Prefix must be provided." });

            var birds = await _birdRepo.GetByPrefixAsync(prefix) ?? Enumerable.Empty<Bird>();
            var dtos = birds.Select(b => b.ToFullDto()).ToList();

            return dtos is not null
                ? Ok(dtos)
                : NotFound(new ProblemDetails { Title = "Not Found", Detail = $"No birds found with prefix '{prefix}'." });
        }

        /// <summary>Search birds by name prefix.</summary>
        [HttpGet("search/contains")]
        [OpenApiOperation("Bird_GetByContains")]
        [ProducesResponseType(typeof(BirdDto[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByContains([FromQuery] string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Prefix must be provided." });

            var birds = await _birdRepo.GetByContainsAsync(str) ?? Enumerable.Empty<Bird>();
            var dtos = birds.Select(b => b.ToFullDto()).ToList();

            return dtos is not null
                ? Ok(dtos)
                : NotFound(new ProblemDetails { Title = "Not Found", Detail = $"No birds found with prefix '{str}'." });
        }

        /// <summary>Get a bird by ID.</summary>
        [HttpGet("{id:int}")]
        [OpenApiOperation("Bird_GetById")]
        [ProducesResponseType(typeof(BirdDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Invalid bird ID." });

            var bird = await _birdRepo.GetByIdAsync(id);
            return bird == null
                ? NotFound(new ProblemDetails { Title = "Not Found", Detail = "Bird not found." })
                : Ok(bird.ToFullDto());
        }

        /// <summary>Append text to a bird's comment.</summary>
        [Authorize]
        [HttpPatch("comment/append/{birdId:int}")]
        [OpenApiOperation("Bird_AppendComment")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AppendComment(int birdId, [FromBody] string additional)
        {
            if (birdId <= 0 || string.IsNullOrWhiteSpace(additional))
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Bird ID and comment text must be provided." });

            var bird = await _birdRepo.GetByIdAsync(birdId);
            if (bird == null)
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Bird not found." });

            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized(new ProblemDetails { Title = "Unauthorized" });
            var user = await _userRepo.GetByIdAsync((int) userId);

            bird.Comment += $"{user?.DisplayName ?? "Anonym"}: {additional}";
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

        /// <summary>Replace a bird's comment (admin only).</summary>
        [HttpPatch("comment/replace/{birdId:int}")]
        [Authorize(Roles = "Admin")]
        [OpenApiOperation("Bird_EditComment")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> EditComment(int birdId, [FromBody] string newComment)
        {
            if (birdId <= 0 || newComment == null)
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Bird ID and new comment must be provided." });

            var bird = await _birdRepo.GetByIdAsync(birdId);
            if (bird == null)
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Bird not found." });

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

        /// <summary>Replace a bird's comment (admin only).</summary>
        [HttpPatch("update/{birdId:int}")]
        [Authorize(Roles = "Admin")]
        [OpenApiOperation("Bird_Update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int birdId, [FromBody] BirdDto newBird)
        {
            if (birdId <= 0 || newBird == null)
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Bird ID and new bird info must be provided." });

            var bird = await _birdRepo.GetByIdAsync(birdId);
            if (bird == null)
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Bird not found." });

            bird.Species = newBird.Species;
            bird.Genus = newBird.Genus;
            bird.Familia = newBird.Familia;
            bird.Ordo = newBird.Ordo;
            bird.Comment = newBird.Comment;
            try
            {
                await _birdRepo.UpdateAsync(bird);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing info for bird {BirdId}.", birdId);
                return Problem("Failed to replace info.");
            }
        }
    }
}
