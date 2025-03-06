using Microsoft.AspNetCore.Mvc;
using CineScope.Shared.Models;
using CineScope.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Controllers
{
    [ApiController]
    [Route("api/movies")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly ILogger<MovieController> _logger;

        public MovieController(IMovieService movieService, ILogger<MovieController> logger)
        {
            _movieService = movieService;
            _logger = logger;
        }

        // GET: api/movies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieModel>>> GetAllMovies()
        {
            try
            {
                var movies = await _movieService.GetAllMoviesAsync();
                return Ok(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all movies");
                return StatusCode(500, new { message = "An error occurred while retrieving movies" });
            }
        }

        // GET: api/movies/top-rated?limit=5
        [HttpGet("top-rated")]
        public async Task<ActionResult<IEnumerable<MovieModel>>> GetTopRatedMovies([FromQuery] int limit = 5)
        {
            try
            {
                var movies = await _movieService.GetTopRatedMoviesAsync(limit);
                return Ok(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top rated movies");
                return StatusCode(500, new { message = "An error occurred while retrieving top rated movies" });
            }
        }

        // GET: api/movies/recent?limit=5
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<MovieModel>>> GetRecentMovies([FromQuery] int limit = 5)
        {
            try
            {
                var movies = await _movieService.GetRecentMoviesAsync(limit);
                return Ok(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent movies");
                return StatusCode(500, new { message = "An error occurred while retrieving recent movies" });
            }
        }

        // GET: api/movies/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MovieModel>> GetMovie(string id)
        {
            try
            {
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    return NotFound(new { message = $"Movie with ID {id} not found" });
                }
                return Ok(movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movie with ID {MovieId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the movie" });
            }
        }

        // GET: api/movies/genre/{genre}
        [HttpGet("genre/{genre}")]
        public async Task<ActionResult<IEnumerable<MovieModel>>> GetMoviesByGenre(string genre)
        {
            try
            {
                var movies = await _movieService.GetMoviesByGenreAsync(genre);
                return Ok(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movies by genre {Genre}", genre);
                return StatusCode(500, new { message = "An error occurred while retrieving movies by genre" });
            }
        }
    }

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
                var reviews = await _reviewService.GetReviewsByMovieIdAsync(movieId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for movie {MovieId}", movieId);
                return StatusCode(500, new { message = "An error occurred while retrieving reviews" });
            }
        }

        // GET: api/reviews/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ReviewModel>>> GetReviewsByUser(string userId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving user reviews" });
            }
        }

        // POST: api/reviews
        [HttpPost]
        public async Task<ActionResult<ReviewModel>> CreateReview(ReviewModel review)
        {
            try
            {
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

    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                bool isAuthenticated = await _userService.AuthenticateUserAsync(model.Username, model.Password);

                if (isAuthenticated)
                {
                    var user = await _userService.GetUserByUsernameAsync(model.Username);
                    return Ok(new
                    {
                        success = true,
                        user = new
                        {
                            id = user.Id.ToString(),
                            username = user.username,
                            email = user.Email,
                            roles = user.Roles
                        }
                    });
                }

                // Check if account is locked
                bool isLocked = await _userService.IsAccountLockedAsync(model.Username);
                if (isLocked)
                {
                    return BadRequest(new { success = false, message = "Your account has been locked due to too many failed login attempts" });
                }

                // Record failed attempt
                await _userService.RecordFailedLoginAttemptAsync(model.Username);

                return BadRequest(new { success = false, message = "Invalid username or password" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                // Check if username already exists
                var existingUser = await _userService.GetUserByUsernameAsync(model.Username);
                if (existingUser != null)
                {
                    return BadRequest(new { success = false, message = "Username already exists" });
                }

                // Check if email already exists
                existingUser = await _userService.GetUserByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { success = false, message = "Email already exists" });
                }

                // Create new user
                var newUser = new UserModel
                {
                    username = model.Username,
                    Email = model.Email,
                    PasswordHash = model.Password, // UserService will hash this
                    Roles = new List<string> { "User" },
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = null,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                };

                var createdUser = await _userService.CreateUserAsync(newUser);

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        id = createdUser.Id.ToString(),
                        username = createdUser.username,
                        email = createdUser.Email
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { success = false, message = "An error occurred during registration" });
            }
        }
    }
}