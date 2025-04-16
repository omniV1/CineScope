using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Server.Models;
using CineScope.Server.Services;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Linq;
using System.Security.Claims;

namespace CineScope.Server.Controllers
{
    /// <summary>
    /// API controller for review-related operations.
    /// Provides endpoints for managing movie reviews.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        /// <summary>
        /// Reference to the review service for business logic.
        /// </summary>
        private readonly ReviewService _reviewService;

        /// <summary>
        /// Reference to the content filter service for validation.
        /// </summary>
        private readonly ContentFilterService _contentFilterService;

        /// <summary>
        /// Reference to the user service for retrieving user information.
        /// </summary>
        private readonly UserService _userService;

        /// <summary>
        /// Initializes a new instance of the ReviewController.
        /// </summary>
        /// <param name="reviewService">Injected review service</param>
        /// <param name="contentFilterService">Injected content filter service</param>
        /// <param name="userService">Injected user service</param>
        public ReviewController(ReviewService reviewService, ContentFilterService contentFilterService, UserService userService)
        {
            _reviewService = reviewService;
            _contentFilterService = contentFilterService;
            _userService = userService;
        }

        /// <summary>
        /// GET: api/Review/movie/{movieId}
        /// Retrieves all reviews for a specific movie.
        /// </summary>
        /// <param name="movieId">The ID of the movie</param>
        /// <returns>List of reviews for the specified movie</returns>
        [HttpGet("movie/{movieId}")]
        public async Task<ActionResult<List<ReviewDto>>> GetReviewsByMovieId(string movieId)
        {
            // Log the requested movie ID for debugging
            Console.WriteLine($"Retrieving reviews for movie ID: {movieId}");

            // Ensure movieId is in a valid format for MongoDB
            if (!ObjectId.TryParse(movieId, out _))
            {
                Console.WriteLine($"Invalid movie ID format: {movieId}");
                return BadRequest("Invalid movie ID format");
            }

            try
            {
                // Get reviews from service
                var reviews = await _reviewService.GetReviewsByMovieIdAsync(movieId);

                // Log the count for debugging
                Console.WriteLine($"Found {reviews.Count} reviews for movie {movieId}");

                // Map to DTOs and populate usernames
                var reviewDtos = new List<ReviewDto>();
                foreach (var review in reviews)
                {
                    var dto = MapToDto(review);

                    // Try to populate username if not already set
                    if (string.IsNullOrEmpty(dto.Username))
                    {
                        try
                        {
                            var userInfo = await _userService.GetPublicUserInfoAsync(review.UserId);
                            if (userInfo != null)
                            {
                                dto.Username = userInfo.Username;
                            }
                            else
                            {
                                dto.Username = "Unknown User";
                            }
                        }
                        catch
                        {
                            dto.Username = "Unknown User";
                        }
                    }

                    reviewDtos.Add(dto);
                }

                return Ok(reviewDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving reviews: {ex.Message}");
                return StatusCode(500, new { Error = $"Failed to retrieve reviews: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: api/Review/user/{userId}
        /// Retrieves all reviews created by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>List of reviews by the specified user</returns>
        [HttpGet("user/{userId}")]
        [Authorize] // Require authentication
        public async Task<ActionResult<List<ReviewDto>>> GetReviewsByUserId(string userId)
        {
            try
            {
                // Get reviews from service
                var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);

                // Map to DTOs
                var reviewDtos = new List<ReviewDto>();
                foreach (var review in reviews)
                {
                    reviewDtos.Add(MapToDto(review));
                }

                return Ok(reviewDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to retrieve user reviews: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: api/Review/{id}
        /// Retrieves a specific review by its ID.
        /// </summary>
        /// <param name="id">The ID of the review to retrieve</param>
        /// <returns>The review if found, 404 Not Found otherwise</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetReviewById(string id)
        {
            // Get the review from service
            var review = await _reviewService.GetReviewByIdAsync(id);

            // If review doesn't exist, return 404
            if (review == null)
                return NotFound();

            // Map to DTO and return
            return Ok(MapToDto(review));
        }

        /// <summary>
        /// POST: api/Review
        /// Creates a new review after content validation.
        /// </summary>
        /// <param name="reviewDto">The review data to create</param>
        /// <returns>The created review with assigned ID</returns>
        [HttpPost]
        [Authorize] // Require authentication
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] ReviewDto reviewDto)
        {
            try
            {
                // Add debugging to see user claims
                Console.WriteLine("User claims in CreateReview:");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }

                // User identification verification 
                // Make sure the review is associated with the authenticated user
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                                 User.FindFirst("sub");

                if (userIdClaim == null)
                {
                    Console.WriteLine("User ID claim not found in token");
                    return Unauthorized(new { Message = "User identity could not be determined" });
                }

                // Force the userId to match the authenticated user
                reviewDto.UserId = userIdClaim.Value;
                Console.WriteLine($"Setting review UserId to authenticated user: {reviewDto.UserId}");

                // Validate content against banned words
                var contentValidation = await _contentFilterService.ValidateContentAsync(reviewDto.Text);

                // If content is not approved, return bad request
                if (!contentValidation.IsApproved)
                {
                    return BadRequest(new
                    {
                        Message = "Review contains inappropriate content",
                        ViolationWords = contentValidation.ViolationWords
                    });
                }

                // Ensure movie ID is valid
                if (!ObjectId.TryParse(reviewDto.MovieId, out _))
                {
                    return BadRequest("Invalid movie ID format");
                }

                // Ensure user ID is valid
                if (!ObjectId.TryParse(reviewDto.UserId, out _))
                {
                    return BadRequest("Invalid user ID format");
                }

                // Map DTO to model
                var review = new Review
                {
                    MovieId = reviewDto.MovieId,
                    UserId = userIdClaim.Value, // Use the authenticated user's ID
                    Rating = reviewDto.Rating,
                    Text = reviewDto.Text,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsApproved = true,
                    FlaggedWords = Array.Empty<string>()
                };

                // Create the review in the database
                var createdReview = await _reviewService.CreateReviewAsync(review);

                // Update the movie's average rating
                await _reviewService.UpdateMovieAverageRatingAsync(review.MovieId);

                // Map back to DTO and return
                var createdDto = MapToDto(createdReview);
                createdDto.Username = reviewDto.Username; // Preserve username from request

                return CreatedAtAction(nameof(GetReviewById), new { id = createdReview.Id }, createdDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateReview: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { Error = $"Failed to create review: {ex.Message}" });
            }
        }

        /// <summary>
        /// PUT: api/Review/{id}
        /// Updates an existing review after content validation.
        /// </summary>
        /// <param name="id">The ID of the review to update</param>
        /// <param name="reviewDto">The updated review data</param>
        /// <returns>No content if successful, appropriate error otherwise</returns>
        [HttpPut("{id}")]
        [Authorize] // Require authentication
        public async Task<IActionResult> UpdateReview(string id, [FromBody] ReviewDto reviewDto)
        {
            // Validate content against banned words
            var contentValidation = await _contentFilterService.ValidateContentAsync(reviewDto.Text);

            // If content is not approved, return bad request
            if (!contentValidation.IsApproved)
            {
                return BadRequest(new
                {
                    Message = "Review contains inappropriate content",
                    ViolationWords = contentValidation.ViolationWords
                });
            }

            // Get the existing review
            var existingReview = await _reviewService.GetReviewByIdAsync(id);

            // If review doesn't exist, return 404
            if (existingReview == null)
                return NotFound();

            // In a real implementation, verify the user is authorized to update this review
            // For example: if (existingReview.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            //    return Forbid();

            // Update properties
            existingReview.Rating = reviewDto.Rating;
            existingReview.Text = reviewDto.Text;
            existingReview.UpdatedAt = DateTime.UtcNow;

            // Perform the update
            var success = await _reviewService.UpdateReviewAsync(id, existingReview);

            if (success)
            {
                // Update the movie's average rating
                await _reviewService.UpdateMovieAverageRatingAsync(existingReview.MovieId);
                return NoContent();
            }
            else
                return BadRequest("Failed to update review");
        }

        /// <summary>
        /// DELETE: api/Review/{id}
        /// Deletes a specific review.
        /// </summary>
        /// <param name="id">The ID of the review to delete</param>
        /// <returns>No content if successful, appropriate error otherwise</returns>
        [HttpDelete("{id}")]
        [Authorize] // Require authentication
        public async Task<IActionResult> DeleteReview(string id)
        {
            // Get the existing review
            var existingReview = await _reviewService.GetReviewByIdAsync(id);

            // If review doesn't exist, return 404
            if (existingReview == null)
                return NotFound();

            // In a real implementation, verify the user is authorized to delete this review
            // For example: if (existingReview.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            //    return Forbid();

            // Perform the deletion
            var success = await _reviewService.DeleteReviewAsync(id);

            if (success)
            {
                // Update the movie's average rating
                await _reviewService.UpdateMovieAverageRatingAsync(existingReview.MovieId);
                return NoContent();
            }
            else
                return BadRequest("Failed to delete review");
        }

        /// <summary>
        /// Maps a Review model to a ReviewDto for client consumption.
        /// </summary>
        /// <param name="review">The Review model to map</param>
        /// <returns>A ReviewDto representation of the Review</returns>
        private ReviewDto MapToDto(Review review)
        {
            return new ReviewDto
            {
                Id = review.Id,
                UserId = review.UserId,
                MovieId = review.MovieId,
                Rating = review.Rating,
                Text = review.Text,
                CreatedAt = review.CreatedAt,
                Username = review.Username // This would be populated from a join in a real implementation
            };
        }
    }
}