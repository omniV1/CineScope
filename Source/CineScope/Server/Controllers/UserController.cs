using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Server.Services;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace CineScope.Server.Controllers
{
    /// <summary>
    /// API controller for user-related operations.
    /// Provides endpoints for managing user profiles.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// Reference to the user service for business logic.
        /// </summary>
        private readonly UserService _userService;

        /// <summary>
        /// Initializes a new instance of the UserController.
        /// </summary>
        /// <param name="userService">Injected user service</param>
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// GET: api/User/profile
        /// Retrieves the authenticated user's profile.
        /// </summary>
        /// <returns>The user profile information</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile()
        {
            try
            {
                // Get the user ID from the authenticated user claims
                // In a real application, you'd get this from the JWT token claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "User not authenticated properly" });
                }

                // Get the user profile
                var userProfile = await _userService.GetUserProfileAsync(userId);

                if (userProfile == null)
                {
                    return NotFound(new { Message = "User profile not found" });
                }

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                return StatusCode(500, new { Message = "An error occurred while retrieving the user profile", Error = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/User/profile
        /// Updates the authenticated user's profile.
        /// </summary>
        /// <param name="updateProfileRequest">The profile update information</param>
        /// <returns>Success or error result</returns>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileRequest updateProfileRequest)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get the user ID from the authenticated user claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "User not authenticated properly" });
                }

                // Update the profile
                var result = await _userService.UpdateUserProfileAsync(userId, updateProfileRequest);

                if (!result.Success)
                {
                    return BadRequest(new { Message = result.Message });
                }

                return Ok(new { Message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                return StatusCode(500, new { Message = "An error occurred while updating the user profile", Error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/User/{userId}
        /// Retrieves basic public information about a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve</param>
        /// <returns>The public user information</returns>
        [HttpGet("{userId}")]
        public async Task<ActionResult<PublicUserDto>> GetPublicUserInfo(string userId)
        {
            try
            {
                var publicUserInfo = await _userService.GetPublicUserInfoAsync(userId);

                if (publicUserInfo == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                return Ok(publicUserInfo);
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                return StatusCode(500, new { Message = "An error occurred while retrieving user information", Error = ex.Message });
            }
        }
    }
}