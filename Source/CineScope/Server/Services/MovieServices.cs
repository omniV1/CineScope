using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Shared.DTOs;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq;

namespace CineScope.Server.Services
{
    /// <summary>
    /// Service responsible for managing movie-related operations.
    /// Implements IMovieService interface for dependency injection.
    /// Handles data access and business logic for movie entities.
    /// </summary>
    public class MovieService : IMovieService
    {
        /// <summary>
        /// Reference to the MongoDB service for database operations.
        /// </summary>
        private readonly IMongoDbService _mongoDbService;

        /// <summary>
        /// MongoDB settings from configuration.
        /// </summary>
        private readonly MongoDbSettings _settings;

        /// <summary>
        /// Initializes a new instance of the MovieService.
        /// </summary>
        /// <param name="mongoDbService">Injected MongoDB service</param>
        /// <param name="settings">Injected MongoDB settings</param>
        public MovieService(IMongoDbService mongoDbService, IOptions<MongoDbSettings> settings)
        {
            _mongoDbService = mongoDbService;
            _settings = settings.Value;
        }

        /// <summary>
        /// Retrieves all movies from the database.
        /// </summary>
        /// <returns>A list of all movies converted to DTOs</returns>
        public async Task<List<MovieDto>> GetAllMoviesAsync()
        {
            try
            {
                // Get the movies collection
                var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

                Console.WriteLine($"Attempting to fetch movies from collection: {_settings.MoviesCollectionName}");

                // Retrieve all movies from the database (no filter)
                var movies = await collection.Find(_ => true).ToListAsync();

                Console.WriteLine($"Found {movies.Count} movies in the database");

                // Convert each movie model to DTO before returning
                var movieDtos = movies.Select(MapToDto).ToList();

                Console.WriteLine($"Returning {movieDtos.Count} movie DTOs");

                return movieDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllMoviesAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific movie by its ID.
        /// </summary>
        /// <param name="id">The ID of the movie to retrieve</param>
        /// <returns>The movie as a DTO, or null if not found</returns>
        public async Task<MovieDto?> GetMovieByIdAsync(string id)
        {
            // Get the movies collection
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

            // Find a movie with the specified ID
            var movie = await collection.Find(m => m.Id == id).FirstOrDefaultAsync();

            // Convert to DTO if found, otherwise return null
            return movie != null ? MapToDto(movie) : null;
        }

        /// <summary>
        /// Retrieves all movies belonging to a specific genre.
        /// </summary>
        /// <param name="genre">The genre to filter by</param>
        /// <returns>A list of matching movies converted to DTOs</returns>
        public async Task<List<MovieDto>> GetMoviesByGenreAsync(string genre)
        {
            // Get the movies collection
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

            // Find all movies that contain the specified genre in their Genres list
            var movies = await collection.Find(m => m.Genres.Contains(genre)).ToListAsync();

            // Convert each movie model to DTO before returning
            return movies.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Creates a new movie in the database.
        /// </summary>
        /// <param name="movieDto">The movie data to create</param>
        /// <returns>The created movie as a DTO with assigned ID</returns>
        public async Task<MovieDto> CreateMovieAsync(MovieDto movieDto)
        {
            // Convert the DTO to a Movie model
            var movie = MapToModel(movieDto);

            // Get the movies collection
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

            // Insert the new movie into the database
            await collection.InsertOneAsync(movie);

            // Return the created movie as a DTO (now with an ID)
            return MapToDto(movie);
        }

        /// <summary>
        /// Updates an existing movie in the database.
        /// </summary>
        /// <param name="id">The ID of the movie to update</param>
        /// <param name="movieDto">The updated movie data</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public async Task<bool> UpdateMovieAsync(string id, MovieDto movieDto)
        {
            // Convert the DTO to a Movie model
            var movie = MapToModel(movieDto);

            // Ensure the ID is set correctly
            movie.Id = id;

            // Get the movies collection
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

            // Replace the existing movie document with the updated one
            var result = await collection.ReplaceOneAsync(m => m.Id == id, movie);

            // Return true if at least one document was modified
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Deletes a movie from the database.
        /// </summary>
        /// <param name="id">The ID of the movie to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public async Task<bool> DeleteMovieAsync(string id)
        {
            // Get the movies collection
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

            // Delete the movie with the specified ID
            var result = await collection.DeleteOneAsync(m => m.Id == id);

            // Return true if at least one document was deleted
            return result.DeletedCount > 0;
        }

        /// <summary>
        /// Maps a Movie model to a MovieDto for client-side consumption.
        /// </summary>
        /// <param name="movie">The Movie model to map</param>
        /// <returns>A MovieDto representation of the Movie</returns>
        private MovieDto MapToDto(Movie movie)
        {
            return new MovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                ReleaseDate = movie.ReleaseDate,
                Genres = movie.Genres,
                Director = movie.Director,
                Actors = movie.Actors,
                PosterUrl = movie.PosterUrl,
                AverageRating = movie.AverageRating,
                ReviewCount = movie.ReviewCount
            };
        }

        /// <summary>
        /// Maps a MovieDto to a Movie model for database operations.
        /// </summary>
        /// <param name="dto">The MovieDto to map</param>
        /// <returns>A Movie model representation of the DTO</returns>
        private Movie MapToModel(MovieDto dto)
        {
            return new Movie
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                ReleaseDate = dto.ReleaseDate,
                Genres = dto.Genres,
                Director = dto.Director,
                Actors = dto.Actors,
                PosterUrl = dto.PosterUrl,
                AverageRating = dto.AverageRating,
                ReviewCount = dto.ReviewCount
            };
        }
    }
}