using Microsoft.AspNetCore.Mvc;
using CineScope.Shared.Models;
using CineScope.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Controllers
{
    /// <summary>
    /// Controller responsible for handling review-related HTTP requests
    /// Provides endpoints for retrieving, creating, and managing movie reviews
    /// </summary>
    [ApiController]
    [Route("api/reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        /// <summary>
        /// Constructor for ReviewController
        /// </summary>
        /// <param name="reviewService">Service for review-related operations</param>
        /// <param name="logger">Logger for recording application events</param>
        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all reviews for a specific movie
        /// </summary>
        /// <param name="movieId">The ID of the movie to get reviews for</param>
        /// <returns>A collection of reviews for the specified movie</returns>
        // GET: api/reviews/movie/{movieId}
        [HttpGet("movie/{movieId}")]
        public async Task<ActionResult<IEnumerable<ReviewModel>>> GetReviewsByMovie(string movieId)
        {
            try
            {
                // Log the request to retrieve reviews for a movie
                _logger.LogInformation($"Getting reviews for movie: {movieId}");

                // Call the service to retrieve the reviews
                var reviews = await _reviewService.GetReviewsByMovieIdAsync(movieId);

                // Return success response with the reviews
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                // Log the error and return a generic error message to the client
                _logger.LogError(ex, $"Error retrieving reviews for movie {movieId}");
                return StatusCode(500, new { message = "An error occurred while retrieving reviews" });
            }
        }

        /// <summary>
        /// Retrieves all reviews created by a specific user
        /// </summary>
        /// <param name="userId">The ID of the user whose reviews to retrieve</param>
        /// <returns>A collection of reviews created by the specified user</returns>
        // GET: api/reviews/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ReviewModel>>> GetReviewsByUser(string userId)
        {
            try
            {
                // Log the request to retrieve reviews by a user
                _logger.LogInformation($"Getting reviews by user: {userId}");

                // Call the service to retrieve the user's reviews
                var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);

                // Return success response with the reviews
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                // Log the error and return a generic error message to the client
                _logger.LogError(ex, $"Error retrieving reviews for user {userId}");
                return StatusCode(500, new { message = "An error occurred while retrieving user reviews" });
            }
        }

        /// <summary>
        /// Creates a new review in the system
        /// </summary>
        /// <param name="review">The review data to be created</param>
        /// <returns>The newly created review with its assigned ID</returns>
        // POST: api/reviews
        [HttpPost]
        public async Task<ActionResult<ReviewModel>> CreateReview(ReviewModel review)
        {
            try
            {
                // Log the creation of a new review
                _logger.LogInformation($"Creating review for movie: {review.MovieId}");

                // Call the service to create the review
                var createdReview = await _reviewService.CreateReviewAsync(review);

                // Return a 201 Created response with the location header pointing to where the review can be retrieved
                return CreatedAtAction(nameof(GetReviewsByMovie), new { movieId = review.MovieId }, createdReview);
            }
            catch (Exception ex)
            {
                // Log the error and return a generic error message to the client
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, new { message = "An error occurred while creating review" });
            }
        }
    }
}
