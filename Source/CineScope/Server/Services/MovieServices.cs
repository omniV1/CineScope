using System;
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
    /// Uses caching to improve performance for frequently accessed data.
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
        /// Reference to the movie cache service.
        /// </summary>
        private readonly MovieCacheService _cacheService;

        /// <summary>
        /// Initializes a new instance of the MovieService.
        /// </summary>
        /// <param name="mongoDbService">Injected MongoDB service</param>
        /// <param name="settings">Injected MongoDB settings</param>
        /// <param name="cacheService">Injected movie cache service</param>
        public MovieService(
            IMongoDbService mongoDbService,
            IOptions<MongoDbSettings> settings,
            MovieCacheService cacheService)
        {
            _mongoDbService = mongoDbService;
            _settings = settings.Value;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Retrieves all movies from cache or database if not in cache.
        /// </summary>
        /// <returns>A list of all movies converted to DTOs</returns>
        public async Task<List<MovieDto>> GetAllMoviesAsync()
        {
            try
            {
                // Try to get from cache first
                var cachedMovies = _cacheService.GetAllMovies();
                if (cachedMovies != null)
                {
                    Console.WriteLine($"Returned {cachedMovies.Count} movies from cache");
                    return cachedMovies;
                }

                // If not in cache, get from database
                Console.WriteLine($"Attempting to fetch movies from collection: {_settings.MoviesCollectionName}");

                // Get the movies collection
                var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

                // Retrieve all movies from the database (no filter)
                var movies = await collection.Find(_ => true).ToListAsync();

                Console.WriteLine($"Found {movies.Count} movies in the database");

                // Convert each movie model to DTO before returning
                var movieDtos = movies.Select(MapToDto).ToList();

                // Cache the result for future requests
                _cacheService.SetAllMovies(movieDtos);

                Console.WriteLine($"Returning {movieDtos.Count} movie DTOs");

                return movieDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllMoviesAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to propagate to controller
            }
        }

        /// <summary>
        /// Retrieves a specific movie by its ID from cache or database.
        /// </summary>
        /// <param name="id">The ID of the movie to retrieve</param>
        /// <returns>The movie as a DTO, or null if not found</returns>
        public async Task<MovieDto?> GetMovieByIdAsync(string id)
        {
            // Try to get from cache first
            var cachedMovie = _cacheService.GetMovieById(id);
            if (cachedMovie != null)
            {
                Console.WriteLine($"Retrieved movie {id} from cache");
                return cachedMovie;
            }

            // If not in cache, get from database
            Console.WriteLine($"Fetching movie {id} from database");

            // Get the movies collection
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

            // Find a movie with the specified ID
            var movie = await collection.Find(m => m.Id == id).FirstOrDefaultAsync();

            // If movie found, convert to DTO and cache it
            if (movie != null)
            {
                var movieDto = MapToDto(movie);
                _cacheService.SetMovieById(movieDto);
                return movieDto;
            }

            // Return null if movie not found
            return null;
        }

        /// <summary>
        /// Retrieves all movies belonging to a specific genre from cache or database.
        /// </summary>
        /// <param name="genre">The genre to filter by</param>
        /// <returns>A list of matching movies converted to DTOs</returns>
        public async Task<List<MovieDto>> GetMoviesByGenreAsync(string genre)
        {
            // Try to get from cache first
            var cachedMovies = _cacheService.GetMoviesByGenre(genre);
            if (cachedMovies != null)
            {
                Console.WriteLine($"Retrieved {cachedMovies.Count} movies for genre {genre} from cache");
                return cachedMovies;
            }

            // If not in cache, get from database
            Console.WriteLine($"Fetching movies for genre {genre} from database");

            // Get the movies collection
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

            // Find all movies that contain the specified genre in their Genres list
            var movies = await collection.Find(m => m.Genres.Contains(genre)).ToListAsync();

            // Convert each movie model to DTO
            var movieDtos = movies.Select(MapToDto).ToList();

            // Cache the result for future requests
            _cacheService.SetMoviesByGenre(genre, movieDtos);

            return movieDtos;
        }

        /// <summary>
        /// Creates a new movie in the database and invalidates relevant caches.
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

            // Invalidate the all movies cache since we added a new movie
            _cacheService.InvalidateAllMovies();

            // Invalidate genre caches that this movie belongs to
            if (movie.Genres != null)
            {
                foreach (var genre in movie.Genres)
                {
                    _cacheService.InvalidateGenre(genre);
                }
            }

            // Return the created movie as a DTO (now with an ID)
            return MapToDto(movie);
        }

        /// <summary>
        /// Updates an existing movie in the database and invalidates relevant caches.
        /// </summary>
        /// <param name="id">The ID of the movie to update</param>
        /// <param name="movieDto">The updated movie data</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public async Task<bool> UpdateMovieAsync(string id, MovieDto movieDto)
        {
            // First get the existing movie to know which genres to invalidate
            var existingMovie = await GetMovieByIdAsync(id);

            // Convert the DTO to a Movie model
            var movie = MapToModel(movieDto);

            // Ensure the ID is set correctly
            movie.Id = id;

            // Get the movies collection
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

            // Replace the existing movie document with the updated one
            var result = await collection.ReplaceOneAsync(m => m.Id == id, movie);

            // Invalidate the specific movie cache
            _cacheService.InvalidateMovie(id);

            // Invalidate the all movies cache
            _cacheService.InvalidateAllMovies();

            // Invalidate genre caches that this movie belongs to or belonged to
            var allGenres = new HashSet<string>(movie.Genres ?? new List<string>());
            if (existingMovie?.Genres != null)
            {
                foreach (var genre in existingMovie.Genres)
                {
                    allGenres.Add(genre);
                }
            }

            foreach (var genre in allGenres)
            {
                _cacheService.InvalidateGenre(genre);
            }

            // Return true if at least one document was modified
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Deletes a movie from the database and invalidates relevant caches.
        /// </summary>
        /// <param name="id">The ID of the movie to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public async Task<bool> DeleteMovieAsync(string id)
        {
            // First get the existing movie to know which genres to invalidate
            var existingMovie = await GetMovieByIdAsync(id);

            // Get the movies collection
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

            // Delete the movie with the specified ID
            var result = await collection.DeleteOneAsync(m => m.Id == id);

            // Invalidate the specific movie cache
            _cacheService.InvalidateMovie(id);

            // Invalidate the all movies cache
            _cacheService.InvalidateAllMovies();

            // Invalidate genre caches that this movie belonged to
            if (existingMovie?.Genres != null)
            {
                foreach (var genre in existingMovie.Genres)
                {
                    _cacheService.InvalidateGenre(genre);
                }
            }

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
            try
            {
                return new MovieDto
                {
                    Id = movie.Id,
                    Title = movie.Title ?? string.Empty,
                    Description = movie.Description ?? string.Empty,
                    ReleaseDate = movie.ReleaseDate,
                    Genres = movie.Genres ?? new List<string>(),
                    Director = movie.Director ?? string.Empty,
                    Actors = movie.Actors ?? new List<string>(),
                    PosterUrl = movie.PosterUrl ?? string.Empty,
                    AverageRating = movie.AverageRating,
                    ReviewCount = movie.ReviewCount
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mapping movie {movie.Id}: {ex.Message}");
                throw;
            }
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