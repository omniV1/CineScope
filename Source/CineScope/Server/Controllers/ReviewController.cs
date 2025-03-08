using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Server.Models;
using CineScope.Server.Services;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        /// Initializes a new instance of the ReviewController.
        /// </summary>
        /// <param name="reviewService">Injected review service</param>
        /// <param name="contentFilterService">Injected content filter service</param>
        public ReviewController(ReviewService reviewService, ContentFilterService contentFilterService)
        {
            _reviewService = reviewService;
            _contentFilterService = contentFilterService;
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
            // Get reviews from service
            var reviews = await _reviewService.GetReviewsByMovieIdAsync(movieId);

            // Map to DTOs
            var reviewDtos = new List<ReviewDto>();
            foreach (var review in reviews)
            {
                reviewDtos.Add(MapToDto(review));
            }

            return Ok(reviewDtos);
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

            // Map DTO to model
            var review = new Review
            {
                MovieId = reviewDto.MovieId,
                UserId = reviewDto.UserId, // In a real implementation, get from authenticated user
                Rating = reviewDto.Rating,
                Text = reviewDto.Text,
                CreatedAt = System.DateTime.UtcNow
            };

            // Create the review in the database
            var createdReview = await _reviewService.CreateReviewAsync(review);

            // Map back to DTO and return
            return CreatedAtAction(nameof(GetReviewById), new { id = createdReview.Id }, MapToDto(createdReview));
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

            // Perform the update
            var success = await _reviewService.UpdateReviewAsync(id, existingReview);

            if (success)
                return NoContent();
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
                return NoContent();
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