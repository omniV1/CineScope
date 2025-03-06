using Microsoft.AspNetCore.Mvc;
using CineScope.Shared.Models;
using CineScope.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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
                _logger.LogError(ex, $"Error retrieving movie with ID {id}");
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
                _logger.LogError(ex, $"Error retrieving movies with genre {genre}");
                return StatusCode(500, new { message = "An error occurred while retrieving movies by genre" });
            }
        }
    }
}