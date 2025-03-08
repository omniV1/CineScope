using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Server.Interfaces;
using CineScope.Server.Services;
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
            try
            {
                // Get all movies from the service
                var movies = await _movieService.GetAllMoviesAsync();

                // Check if movies were found
                if (movies == null || movies.Count == 0)
                {
                    // Return 204 No Content if no movies found
                    return NoContent();
                }

                // Return the movies with 200 OK status
                return Ok(movies);
            }
            catch (System.Exception ex)
            {
                // Log the exception in a real application
                return StatusCode(500, new { Message = $"Error retrieving movies: {ex.Message}" });
            }
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
            try
            {
                // Get the movie with the specified ID from the service
                var movie = await _movieService.GetMovieByIdAsync(id);

                // If the movie doesn't exist, return 404 Not Found
                if (movie == null)
                    return NotFound(new { Message = $"Movie with ID {id} not found" });

                // Return the movie with the 200 OK status
                return Ok(movie);
            }
            catch (System.Exception ex)
            {
                // Log the exception in a real application
                return StatusCode(500, new { Message = $"Error retrieving movie: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: api/Movie/genre/{genre}
        /// Retrieves all movies of a specific genre.
        /// </summary>
        /// <param name="genre">The genre to filter by</param>
        /// <returns>A list of movies matching the specified genre</returns>
        [HttpGet("genre/{genre}")]
        public async Task<ActionResult<List<MovieDto>>> GetMoviesByGenre(string genre)
        {
            try
            {
                // Get movies by genre from the service
                var movies = await _movieService.GetMoviesByGenreAsync(genre);

                // Check if movies were found
                if (movies == null || movies.Count == 0)
                {
                    // Return 204 No Content if no movies found
                    return NoContent();
                }

                // Return the movies with 200 OK status
                return Ok(movies);
            }
            catch (System.Exception ex)
            {
                // Log the exception in a real application
                return StatusCode(500, new { Message = $"Error retrieving movies by genre: {ex.Message}" });
            }
        }
    }
}