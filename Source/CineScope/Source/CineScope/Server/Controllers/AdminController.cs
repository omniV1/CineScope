using CineScope.Server.Services;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineScope.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            try
            {
                var stats = await _adminService.GetDashboardStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new { Message = "Error retrieving dashboard statistics", Error = ex.Message });
            }
        }

        [HttpGet("collection-stats")]
        public async Task<ActionResult<Dictionary<string, long>>> GetCollectionStats()
        {
            try
            {
                var stats = await _adminService.GetCollectionStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collection stats");
                return StatusCode(500, new { Message = "Error retrieving collection statistics", Error = ex.Message });
            }
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<UserAdminDto>>> GetUsers([FromQuery] string? search = null, [FromQuery] string? role = null, [FromQuery] string? status = null)
        {
            try
            {
                var users = await _adminService.GetAllUsersAsync(search, role, status);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for admin");
                return StatusCode(500, new { Message = "Error retrieving user data", Error = ex.Message });
            }
        }

        [HttpPut("users/{userId}/status")]
        public async Task<IActionResult> UpdateUserStatus(string userId, [FromBody] string status)
        {
            try
            {
                await _adminService.UpdateUserStatusAsync(userId, status);
                return Ok(new { Message = $"User status updated to {status}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for user {userId}");
                return StatusCode(500, new { Message = "Error updating user status", Error = ex.Message });
            }
        }

        [HttpPost("users/{userId}/toggle-admin")]
        public async Task<IActionResult> ToggleUserAdminPrivileges(string userId)
        {
            try
            {
                await _adminService.ToggleUserAdminPrivilegesAsync(userId);
                return Ok(new { Message = "ToggleUserAdminPrivilegesAsync privileges toggled" });
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for user {userId}");
                return StatusCode(500, new { Message = "Error updating user status", Error = ex.Message });
            }
        }

        [HttpGet("banned-words")]
        public async Task<ActionResult<List<BannedWordDto>>> GetBannedWords([FromQuery] string? category = null, [FromQuery] int? severity = null)
        {
            try
            {
                var bannedWords = await _adminService.GetAllBannedWordsAsync(category, severity);
                return Ok(bannedWords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting banned words");
                return StatusCode(500, new { Message = "Error retrieving banned words", Error = ex.Message });
            }
        }

        [HttpPost("banned-words")]
        public async Task<ActionResult<BannedWordDto>> AddBannedWord([FromBody] BannedWordDto bannedWordDto)
        {
            try
            {
                var result = await _adminService.AddBannedWordAsync(bannedWordDto);
                return CreatedAtAction(nameof(GetBannedWords), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding banned word");
                return StatusCode(500, new { Message = "Error adding banned word", Error = ex.Message });
            }
        }

        [HttpPost("moderate/{reviewId}")]
        public async Task<IActionResult> ModerateContent(string reviewId, [FromBody] ModerationActionDto action)
        {
            try
            {
                await _adminService.ModerateContentAsync(reviewId, action);
                return Ok(new { Message = $"Review {reviewId} moderated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error moderating content for review {reviewId}");
                return StatusCode(500, new { Message = "Error moderating content", Error = ex.Message });
            }
        }

        [HttpGet("flagged-reviews")]
        public async Task<ActionResult<List<ReviewModerationDto>>> GetFlaggedReviews()
        {
            try
            {
                var flaggedReviews = await _adminService.GetFlaggedReviewsAsync();
                return Ok(flaggedReviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flagged reviews");
                return StatusCode(500, new { Message = "Error retrieving flagged reviews", Error = ex.Message });
            }
        }
    }
}