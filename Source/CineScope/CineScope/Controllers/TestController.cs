using Microsoft.AspNetCore.Mvc;
using CineScope.Shared.Models;
using CineScope.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Controllers
{
    [ApiController]
    [Route("api/test")]
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
                _logger.LogInformation($"Found {movies?.Count ?? 0} movies");
                return Ok(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movies");
                return StatusCode(500, new { message = $"Error retrieving movies: {ex.Message}", stackTrace = ex.StackTrace });
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
    }

    [ApiController]
    [Route("api/ping")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "API is working",
                time = DateTime.UtcNow,
                version = "1.0"
            });
        }
    }

    [ApiController]
    [Route("api/diagnostic")]
    public class DiagnosticController : ControllerBase
    {
        private readonly MongoDBSettings _settings;
        private readonly ILogger<DiagnosticController> _logger;

        public DiagnosticController(MongoDBSettings settings, ILogger<DiagnosticController> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        [HttpGet("mongo")]
        public async Task<IActionResult> TestMongo()
        {
            try
            {
                var client = new MongoClient(_settings.ConnectionString);
                var database = client.GetDatabase(_settings.DatabaseName);

                // Get counts from all collections
                var moviesCount = await database.GetCollection<BsonDocument>(_settings.MoviesCollectionName)
                    .CountDocumentsAsync(new BsonDocument());

                var usersCount = await database.GetCollection<BsonDocument>(_settings.UsersCollectionName)
                    .CountDocumentsAsync(new BsonDocument());

                var reviewsCount = await database.GetCollection<BsonDocument>(_settings.ReviewsCollectionName)
                    .CountDocumentsAsync(new BsonDocument());

                var bannedWordsCount = await database.GetCollection<BsonDocument>(_settings.BannedWordsCollectionName)
                    .CountDocumentsAsync(new BsonDocument());

                return Ok(new
                {
                    status = "Connected",
                    database = _settings.DatabaseName,
                    counts = new
                    {
                        movies = moviesCount,
                        users = usersCount,
                        reviews = reviewsCount,
                        bannedWords = bannedWordsCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing MongoDB connection");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}