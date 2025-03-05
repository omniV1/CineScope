using CineScope.Interfaces;
using CineScope.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IUserService _userService;
        private readonly IReviewService _reviewService;

        public TestController(
            IMovieService movieService,
            IUserService userService,
            IReviewService reviewService)
        {
            _movieService = movieService;
            _userService = userService;
            _reviewService = reviewService;
        }

        [HttpGet("movies")]
        public async Task<ActionResult<List<MovieModel>>> GetMovies()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            return Ok(movies);
        }

        [HttpGet("movies/top-rated")]
        public async Task<ActionResult<List<MovieModel>>> GetTopRatedMovies([FromQuery] int limit = 5)
        {
            var movies = await _movieService.GetTopRatedMoviesAsync(limit);
            return Ok(movies);
        }

        [HttpGet("movies/{id}")]
        public async Task<ActionResult<MovieModel>> GetMovie(string id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
                return NotFound();

            return Ok(movie);
        }

        [HttpGet("movies/{movieId}/reviews")]
        public async Task<ActionResult<List<ReviewModel>>> GetMovieReviews(string movieId)
        {
            var reviews = await _reviewService.GetReviewsByMovieIdAsync(movieId);
            return Ok(reviews);
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<UserModel>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();

            // Don't return password hashes
            foreach (var user in users)
            {
                user.PasswordHash = null;
            }

            return Ok(users);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            var isAuthenticated = await _userService.AuthenticateUserAsync(request.Username, request.Password);
            if (!isAuthenticated)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(new { message = "Login successful" });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}