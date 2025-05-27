namespace BirdWatching.Api.Controllers
{
    using BirdWatching.Shared.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/user")]
    public class UserController : BaseApiController
    {
        private readonly ILogger<UserController> _logger;

        public UserController(AppDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
            InitRepos__ContextMustNotBeNull();
        }

        /// <summary>
        /// Create new user (registrovat se může kdokoliv).
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        {
            if (userDto == null)
                return BadRequest("User data must be provided.");

            try
            {
                var user = userDto.ToEntity();
                await _userRepo.AddAsync(user);
                return Ok(user.ToFullDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                return Problem("An error occurred while creating the user.");
            }
        }

        /// <summary>
        /// Update own profile (nebo Admin může update komukoli).
        /// </summary>
        [HttpPut("{userId:int}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserDto userDto)
        {
            if (userDto == null)
                return BadRequest("User data must be provided.");

            // ID přihlášeného z claimu
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(sub, out var currentUserId))
                return Unauthorized();

            // jestli je Admin nebo aktualizuje sám sebe
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && currentUserId != userId)
                return Forbid();

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.UserName = userDto.UserName ?? user.UserName;
            user.PasswordHash = userDto.PasswordHash ?? user.PasswordHash;

            try
            {
                await _userRepo.UpdateAsync(user);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}.", userId);
                return Problem("Failed to update user.");
            }
        }

        /// <summary>
        /// Delete own account (Admin může delete kohokoliv).
        /// </summary>
        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(sub, out var currentUserId))
                return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && currentUserId != userId)
                return Forbid();

            bool deleted;
            try
            {
                deleted = await _userRepo.DeleteAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}.", userId);
                return Problem("Failed to delete user.");
            }

            return deleted ? NoContent() : NotFound("User not found.");
        }

        /// <summary>
        /// Get own profile (nebo Admin může get kohokoliv).
        /// </summary>
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(sub, out var currentUserId))
                return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && currentUserId != userId)
                return Forbid();

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var dto = user.ToFullDto();
            return Ok(dto);
        }

        /// <summary>
        /// List all users (Admin only).
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepo.GetAllAsync();
            var dtos = users
                .Select(u => {
                    var d = u.ToFullDto();
                    return d;
                })
                .ToArray();
            return Ok(dtos);
        }

        /// <summary>
        /// Add curated watcher to current user.
        /// </summary>
        [HttpPost("curate/{watcherPublicId}")]
        public async Task<IActionResult> AddCuratedWatcher(string watcherPublicId)
        {
            if (string.IsNullOrWhiteSpace(watcherPublicId))
                return BadRequest("WatcherPublicId must be provided.");

            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(sub, out var userId))
                return Unauthorized();

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var watcher = await _watcherRepo.GetByPublicIdAsync(watcherPublicId);
            if (watcher == null)
                return NotFound("Watcher not found.");

            if (watcher.Curators.Any(u => u.Id == userId))
                return Conflict("Already a curator.");

            watcher.Curators.Add(user);
            try
            {
                await _watcherRepo.UpdateAsync(watcher);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add curated watcher for user {UserId}.", userId);
                return Problem("Failed to add curated watcher.");
            }
        }
    }
}
