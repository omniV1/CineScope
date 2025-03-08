using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Shared.DTOs;

namespace CineScope.Server.Interfaces
{
    /// <summary>
    /// Interface defining operations for movie data access and management.
    /// Enables dependency injection and unit testing of movie-related functionality.
    /// </summary>
    public interface IMovieService
    {
        /// <summary>
        /// Retrieves all movies from the database.
        /// </summary>
        /// <returns>A list of all movies as DTOs</returns>
        Task<List<MovieDto>> GetAllMoviesAsync();

        /// <summary>
        /// Retrieves a specific movie by its ID.
        /// </summary>
        /// <param name="id">The ID of the movie to retrieve</param>
        /// <returns>The movie as a DTO, or null if not found</returns>
        Task<MovieDto> GetMovieByIdAsync(string id);

        /// <summary>
        /// Retrieves all movies belonging to a specific genre.
        /// </summary>
        /// <param name="genre">The genre to filter by</param>
        /// <returns>A list of movies in the specified genre</returns>
        Task<List<MovieDto>> GetMoviesByGenreAsync(string genre);
    }
}