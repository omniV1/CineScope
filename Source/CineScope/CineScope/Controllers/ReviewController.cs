using Microsoft.AspNetCore.Mvc;
using CineScope.Shared.Models;
using CineScope.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        // GET: api/reviews/movie/{movieId}
        [HttpGet("movie/{movieId}")]
        public async Task<ActionResult<IEnumerable<ReviewModel>>> GetReviewsByMovie(string movieId)
        {
            try
            {
                _logger.LogInformation($"Getting reviews for movie: {movieId}");
                var reviews = await _reviewService.GetReviewsByMovieIdAsync(movieId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving reviews for movie {movieId}");
                return StatusCode(500, new { message = "An error occurred while retrieving reviews" });
            }
        }

        // GET: api/reviews/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ReviewModel>>> GetReviewsByUser(string userId)
        {
            try
            {
                _logger.LogInformation($"Getting reviews by user: {userId}");
                var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving reviews for user {userId}");
                return StatusCode(500, new { message = "An error occurred while retrieving user reviews" });
            }
        }

        // POST: api/reviews
        [HttpPost]
        public async Task<ActionResult<ReviewModel>> CreateReview(ReviewModel review)
        {
            try
            {
                _logger.LogInformation($"Creating review for movie: {review.MovieId}");
                var createdReview = await _reviewService.CreateReviewAsync(review);
                return CreatedAtAction(nameof(GetReviewsByMovie), new { movieId = review.MovieId }, createdReview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, new { message = "An error occurred while creating review" });
            }
        }
    }
} 