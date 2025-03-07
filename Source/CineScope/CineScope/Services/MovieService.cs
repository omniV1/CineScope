using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Services
{
    /// <summary>
    /// Service responsible for movie-related business logic
    /// Implements IMovieService to provide movie operations for the application
    /// Acts as an intermediary between controllers and the movie repository
    /// </summary>
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;

        /// <summary>
        /// Constructor for MovieService
        /// </summary>
        /// <param name="movieRepository">Repository for accessing movie data</param>
        public MovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        /// <summary>
        /// Retrieves all movies from the database
        /// </summary>
        /// <returns>A list of all movies</returns>
        public async Task<List<MovieModel>> GetAllMoviesAsync()
        {
            return await _movieRepository.GetAllAsync();
        }

        /// <summary>
        /// Finds a movie by its string ID
        /// </summary>
        /// <param name="id">String representation of the movie's ObjectId</param>
        /// <returns>The movie if found, null otherwise</returns>
        public async Task<MovieModel> GetMovieByIdAsync(string id)
        {
            // Convert string ID to MongoDB ObjectId
            return await _movieRepository.GetByIdAsync(new ObjectId(id));
        }

        /// <summary>
        /// Retrieves all movies that belong to a specific genre
        /// </summary>
        /// <param name="genre">The genre to filter by</param>
        /// <returns>A list of movies in the specified genre</returns>
        public async Task<List<MovieModel>> GetMoviesByGenreAsync(string genre)
        {
            return await _movieRepository.GetByGenreAsync(genre);
        }

        /// <summary>
        /// Gets the highest rated movies in descending order
        /// </summary>
        /// <param name="limit">Maximum number of movies to return (default: 10)</param>
        /// <returns>A list of top rated movies</returns>
        public async Task<List<MovieModel>> GetTopRatedMoviesAsync(int limit = 10)
        {
            return await _movieRepository.GetTopRatedAsync(limit);
        }

        /// <summary>
        /// Gets the most recently released movies
        /// </summary>
        /// <param name="limit">Maximum number of movies to return (default: 10)</param>
        /// <returns>A list of recent movies</returns>
        public async Task<List<MovieModel>> GetRecentMoviesAsync(int limit = 10)
        {
            return await _movieRepository.GetRecentAsync(limit);
        }

        /// <summary>
        /// Creates a new movie in the database
        /// </summary>
        /// <param name="movie">The movie to create</param>
        /// <returns>The created movie with its generated ID</returns>
        public async Task<MovieModel> CreateMovieAsync(MovieModel movie)
        {
            // Generate a new ID if one doesn't exist
            if (movie.Id == ObjectId.Empty)
            {
                movie.Id = ObjectId.GenerateNewId();
            }

            // Set default release date if not provided
            if (movie.ReleaseDate == DateTime.MinValue)
            {
                movie.ReleaseDate = DateTime.UtcNow;
            }

            // Initialize empty collections if null
            if (movie.Genres == null)
            {
                movie.Genres = new List<string>();
            }

            if (movie.Actors == null)
            {
                movie.Actors = new List<string>();
            }

            // Save the movie to the database
            return await _movieRepository.CreateAsync(movie);
        }

        /// <summary>
        /// Updates an existing movie in the database
        /// </summary>
        /// <param name="id">String representation of the movie's ObjectId</param>
        /// <param name="movie">The updated movie data</param>
        public async Task UpdateMovieAsync(string id, MovieModel movie)
        {
            // Convert string ID to MongoDB ObjectId and update the movie
            await _movieRepository.UpdateAsync(new ObjectId(id), movie);
        }

        /// <summary>
        /// Deletes a movie from the database
        /// </summary>
        /// <param name="id">String representation of the movie's ObjectId</param>
        public async Task DeleteMovieAsync(string id)
        {
            // Convert string ID to MongoDB ObjectId and delete the movie
            await _movieRepository.DeleteAsync(new ObjectId(id));
        }
    }
}


