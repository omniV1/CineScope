using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Server.Interfaces;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CineScope.Server.Controllers
{
    /// <summary>
    /// API controller for movie-related operations.
    /// Provides endpoints for retrieving movie information.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController : ControllerBase
    {
        /// <summary>
        /// Reference to the movie service for business logic operations.
        /// </summary>
        private readonly IMovieService _movieService;

        /// <summary>
        /// Initializes a new instance of the MovieController.
        /// </summary>
        /// <param name="movieService">Injected movie service</param>
        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        /// <summary>
        /// GET: api/Movie
        /// Retrieves all movies from the database.
        /// </summary>
        /// <returns>A list of all movies</returns>
        [HttpGet]
        public async Task<ActionResult<List<MovieDto>>> GetAllMovies()
        {
            // Get all movies from the service
            var movies = await _movieService.GetAllMoviesAsync();

            // Return the movies with 200 OK status
            return Ok(movies);
        }
        /// <summary>
        /// GET: api/Movie/{id}
        /// Retrieves a specific movie by ID.
        /// </summary>
        /// <param name="id">The ID of the movie to retrieve</param>
        /// <returns>The movie if found, 404 Not Found otherwise</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDto>> GetMovieById(string id)
        {
            // Get the movie with the specified ID from the service
            var movie = await _movieService.GetMovieByIdAsync(id);

            // If the movie doesn't exist, return 404 Not Found
            if (movie == null)
                return NotFound();

            // Return the movie with 200 OK status
            return Ok(movie);
        }
    }
}