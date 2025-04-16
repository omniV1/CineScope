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
        Task<MovieDto?> GetMovieByIdAsync(string id);

        /// <summary>
        /// Retrieves all movies belonging to a specific genre.
        /// </summary>
        /// <param name="genre">The genre to filter by</param>
        /// <returns>A list of movies in the specified genre</returns>
        Task<List<MovieDto>> GetMoviesByGenreAsync(string genre);
        
        /// <summary>
        /// Creates a new movie in the database.
        /// </summary>
        /// <param name="movieDto">The movie data to create</param>
        /// <returns>The created movie as a DTO with assigned ID</returns>
        Task<MovieDto> CreateMovieAsync(MovieDto movieDto);
        
        /// <summary>
        /// Updates an existing movie in the database.
        /// </summary>
        /// <param name="id">The ID of the movie to update</param>
        /// <param name="movieDto">The updated movie data</param>
        /// <returns>True if update was successful, false otherwise</returns>
        Task<bool> UpdateMovieAsync(string id, MovieDto movieDto);
        
        /// <summary>
        /// Deletes a movie from the database.
        /// </summary>
        /// <param name="id">The ID of the movie to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> DeleteMovieAsync(string id);
    }
}