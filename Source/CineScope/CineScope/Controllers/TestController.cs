using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Controllers
{
    // Explicitly set the route to lowercase "test" to ensure consistency
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IUserService _userService;
        private readonly IReviewService _reviewService;
        private readonly ILogger<TestController> _logger;

        public TestController(
            IMovieService movieService,
            IUserService userService,
            IReviewService reviewService,
            ILogger<TestController> logger)
        {
            _movieService = movieService;
            _userService = userService;
            _reviewService = reviewService;
            _logger = logger;
        }

        [HttpGet("movies")]
        public async Task<ActionResult<List<MovieModel>>> GetMovies()
        {
            try
            {
                _logger.LogInformation("Getting all movies");
                var movies = await _movieService.GetAllMoviesAsync();
                return Ok(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movies");
                return StatusCode(500, new { message = $"Error retrieving movies: {ex.Message}" });
            }
        }

        [HttpGet("movies/top-rated")]
        public async Task<ActionResult<List<MovieModel>>> GetTopRatedMovies([FromQuery] int limit = 5)
        {
            try
            {
                var movies = await _movieService.GetTopRatedMoviesAsync(limit);
                return Ok(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top-rated movies");
                return StatusCode(500, new { message = $"Error retrieving top-rated movies: {ex.Message}" });
            }
        }

        [HttpGet("movies/{id}")]
        public async Task<ActionResult<MovieModel>> GetMovie(string id)
        {
            try
            {
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                    return NotFound(new { message = $"Movie with ID {id} not found" });

                return Ok(movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movie with ID {MovieId}", id);
                return StatusCode(500, new { message = $"Error retrieving movie: {ex.Message}" });
            }
        }

        [HttpGet("movies/{movieId}/reviews")]
        public async Task<ActionResult<List<ReviewModel>>> GetMovieReviews(string movieId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByMovieIdAsync(movieId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for movie ID {MovieId}", movieId);
                return StatusCode(500, new { message = $"Error retrieving reviews: {ex.Message}" });
            }
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<UserModel>>> GetUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();

                // Don't return password hashes
                foreach (var user in users)
                {
                    user.PasswordHash = null;
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { message = $"Error retrieving users: {ex.Message}" });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Username and password are required" });
                }

                var isAuthenticated = await _userService.AuthenticateUserAsync(request.Username, request.Password);

                if (!isAuthenticated)
                    return Unauthorized(new { message = "Invalid username or password" });

                return Ok(new { message = "Login successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return StatusCode(500, new { message = $"Login error: {ex.Message}" });
            }
        }
     
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}