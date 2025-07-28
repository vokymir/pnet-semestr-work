namespace BirdWatching.Api.Controllers
{
    using BirdWatching.Shared.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using NSwag.Annotations;

    [ApiController]
    [Route("api/user")]
    public class UserController : BaseApiController
    {
        private readonly ILogger<UserController> _logger;

        public UserController(AppDbContext context, ILogger<UserController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            InitRepos__ContextMustNotBeNull();
        }

        /// <summary>Create new user (registration is public).</summary>
        [HttpPost]
        [AllowAnonymous]
        [OpenApiOperation("User_Create", "Creates a new user account.")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        {
            if (userDto == null)
                return BadRequest(new ProblemDetails { Title = "Invalid Input", Detail = "User data must be provided." });

            try
            {
                var user = userDto.ToEntity();
                await _userRepo.AddAsync(user);
                return Ok(user.ToFullDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                return Problem(title: "Server Error", detail: "An error occurred while creating the user.");
            }
        }

        /// <summary>Update own profile (or admin can update anyone).</summary>
        [HttpPut("{userId:int}")]
        [Authorize]
        [OpenApiOperation("User_Update", "Updates an existing user.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserDto userDto)
        {
            if (userDto == null)
                return BadRequest(new ProblemDetails { Title = "Invalid Input", Detail = "User data must be provided." });

            var currentUserId = GetCurrentUserId();
            if (currentUserId is null)
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "JWT token is missing or invalid." });

            if (!User.IsInRole("Admin") && currentUserId != userId)
                return Forbid();

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new ProblemDetails { Title = "User Not Found", Detail = $"User with ID {userId} does not exist." });

            user.PasswordHash = userDto.PasswordHash ?? user.PasswordHash;
            user.DisplayName = userDto.DisplayName ?? user.DisplayName;
            user.Email = userDto.Email ?? user.Email;
            user.PreferenceLoginMinutes = userDto.PreferenceLoginMinutes;

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

        /// <summary>Delete own account (or admin can delete anyone).</summary>
        [HttpDelete("{userId:int}")]
        [Authorize]
        [OpenApiOperation("User_Delete", "Deletes a user account.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId is null)
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "JWT token is missing or invalid." });

            if (!User.IsInRole("Admin") && currentUserId != userId)
                return Forbid();

            try
            {
                bool deleted = await _userRepo.DeleteAsync(userId);
                return deleted
                    ? NoContent()
                    : NotFound(new ProblemDetails { Title = "User Not Found", Detail = $"User with ID {userId} does not exist." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}.", userId);
                return Problem("Failed to delete user.");
            }
        }

        /// <summary>Get own profile (or admin can access anyone).</summary>
        [HttpGet("{userId:int}")]
        [Authorize]
        [OpenApiOperation("User_Get", "Gets a user profile.")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId is null)
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "JWT token is missing or invalid." });

            if (!User.IsInRole("Admin") && currentUserId != userId)
                return Forbid();

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new ProblemDetails { Title = "User Not Found", Detail = $"User with ID {userId} does not exist." });

            return Ok(user.ToFullDto());
        }

        /// <summary>List all users (admin only).</summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        [OpenApiOperation("User_GetAll", "Lists all registered users.")]
        [ProducesResponseType(typeof(UserDto[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepo.GetAllAsync();
            var dtos = users.Select(u => u.ToFullDto()).ToArray();
            return Ok(dtos);
        }

        /// <summary>Add curated watcher to current user.</summary>
        [HttpPost("curate/{watcherPublicId}")]
        [Authorize]
        [OpenApiOperation("User_AddCuratedWatcher", "Adds a watcher to current user's curated list.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCuratedWatcher(string watcherPublicId)
        {
            if (string.IsNullOrWhiteSpace(watcherPublicId))
                return BadRequest(new ProblemDetails { Title = "Invalid Input", Detail = "WatcherPublicId must be provided." });

            var uId = GetCurrentUserId();
            if (uId is null)
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "JWT token is missing or invalid." });

            var user = await _userRepo.GetByIdAsync((int) uId);
            if (user == null)
                return NotFound(new ProblemDetails { Title = "User Not Found", Detail = "User not found." });

            var watcher = await _watcherRepo.GetByPublicIdAsync(watcherPublicId);
            if (watcher == null)
                return NotFound(new ProblemDetails { Title = "Watcher Not Found", Detail = "Watcher not found." });

            if (watcher.Curators.Any(u => u.Id == user.Id))
                return Conflict(new ProblemDetails { Title = "Already a Curator", Detail = "User is already curating this watcher." });

            watcher.Curators.Add(user);
            try
            {
                await _watcherRepo.UpdateAsync(watcher);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add curated watcher for user {UserId}.", user.Id);
                return Problem("Failed to add curated watcher.");
            }
        }

        /// <summary>Promotes a user to admin (admin only).</summary>
        [HttpPost("{userId:int}/promote")]
        [Authorize(Roles = "Admin")]
        [OpenApiOperation("User_Promote", "Promotes a user to admin.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PromoteUser(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new ProblemDetails { Title = "User Not Found", Detail = $"User with ID {userId} not found." });

            if (user.IsAdmin)
                return NoContent(); // Already admin

            user.IsAdmin = true;
            try
            {
                await _userRepo.UpdateAsync(user);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to promote user {UserId} to admin.", userId);
                return Problem("Failed to promote user.");
            }
        }

        /// <summary>Demotes an admin user to regular user (admin only).</summary>
        [HttpPost("{userId:int}/demote")]
        [Authorize(Roles = "Admin")]
        [OpenApiOperation("User_Demote", "Demotes an admin user to regular user.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DemoteUser(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new ProblemDetails { Title = "User Not Found", Detail = $"User with ID {userId} not found." });

            if (!user.IsAdmin)
                return NoContent(); // Already regular

            user.IsAdmin = false;
            try
            {
                await _userRepo.UpdateAsync(user);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to demote user {UserId} from admin.", userId);
                return Problem("Failed to demote user.");
            }
        }
    }
}
