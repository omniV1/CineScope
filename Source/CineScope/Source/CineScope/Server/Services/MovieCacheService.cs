using CineScope.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CineScope.Server.Services
{
    /// <summary>
    /// Service responsible for caching movie-related data.
    /// Provides methods to store and retrieve movie data from cache.
    /// </summary>
    public class MovieCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MovieCacheService> _logger;
        private readonly MovieCacheSettings _settings;

        // Cache keys
        private const string ALL_MOVIES_CACHE_KEY = "AllMovies";
        private const string MOVIE_BY_ID_CACHE_KEY_PREFIX = "Movie_";
        private const string MOVIES_BY_GENRE_CACHE_KEY_PREFIX = "MoviesByGenre_";

        // Cache statistics
        private int _cacheHits = 0;
        private int _cacheMisses = 0;

        /// <summary>
        /// Initializes a new instance of the MovieCacheService.
        /// </summary>
        /// <param name="cache">Memory cache instance</param>
        /// <param name="logger">Logger for recording operations</param>
        /// <param name="options">Movie cache settings from configuration</param>
        public MovieCacheService(
            IMemoryCache cache,
            ILogger<MovieCacheService> logger,
            IOptions<MovieCacheSettings> options)
        {
            _cache = cache;
            _logger = logger;
            _settings = options.Value;
        }

        /// <summary>
        /// Gets all movies from cache or returns null if not cached.
        /// </summary>
        /// <returns>List of cached movies or null if not in cache</returns>
        public List<MovieDto> GetAllMovies()
        {
            if (!_settings.Enabled)
                return null;

            if (_cache.TryGetValue(ALL_MOVIES_CACHE_KEY, out List<MovieDto> movies))
            {
                Interlocked.Increment(ref _cacheHits);
                _logger.LogInformation("Cache hit for all movies. Total hits: {CacheHits}", _cacheHits);
                return movies;
            }

            Interlocked.Increment(ref _cacheMisses);
            _logger.LogInformation("Cache miss for all movies. Total misses: {CacheMisses}", _cacheMisses);
            return null;
        }

        /// <summary>
        /// Sets all movies in the cache.
        /// </summary>
        /// <param name="movies">List of movies to cache</param>
        public void SetAllMovies(List<MovieDto> movies)
        {
            if (!_settings.Enabled || movies == null)
                return;

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.AllMoviesExpirationMinutes))
                .SetSlidingExpiration(TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes))
                .SetPriority(CacheItemPriority.High);

            _cache.Set(ALL_MOVIES_CACHE_KEY, movies, cacheOptions);
            _logger.LogInformation("Cached {Count} movies for {Minutes} minutes",
                movies.Count, _settings.AllMoviesExpirationMinutes);
        }

        /// <summary>
        /// Gets a movie by ID from cache or returns null if not cached.
        /// </summary>
        /// <param name="id">The ID of the movie to retrieve</param>
        /// <returns>Cached movie or null if not in cache</returns>
        public MovieDto GetMovieById(string id)
        {
            if (!_settings.Enabled || string.IsNullOrEmpty(id))
                return null;

            string cacheKey = $"{MOVIE_BY_ID_CACHE_KEY_PREFIX}{id}";
            if (_cache.TryGetValue(cacheKey, out MovieDto movie))
            {
                Interlocked.Increment(ref _cacheHits);
                _logger.LogInformation("Cache hit for movie ID {MovieId}. Total hits: {CacheHits}", id, _cacheHits);
                return movie;
            }

            Interlocked.Increment(ref _cacheMisses);
            _logger.LogInformation("Cache miss for movie ID {MovieId}. Total misses: {CacheMisses}", id, _cacheMisses);
            return null;
        }

        /// <summary>
        /// Sets a movie in the cache by ID.
        /// </summary>
        /// <param name="movie">The movie to cache</param>
        public void SetMovieById(MovieDto movie)
        {
            if (!_settings.Enabled || movie == null || string.IsNullOrEmpty(movie.Id))
                return;

            string cacheKey = $"{MOVIE_BY_ID_CACHE_KEY_PREFIX}{movie.Id}";
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.MovieByIdExpirationMinutes))
                .SetSlidingExpiration(TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes))
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(cacheKey, movie, cacheOptions);
            _logger.LogInformation("Cached movie ID {MovieId} for {Minutes} minutes",
                movie.Id, _settings.MovieByIdExpirationMinutes);
        }

        /// <summary>
        /// Gets movies by genre from cache or returns null if not cached.
        /// </summary>
        /// <param name="genre">The genre to filter by</param>
        /// <returns>List of cached movies for the genre or null if not in cache</returns>
        public List<MovieDto> GetMoviesByGenre(string genre)
        {
            if (!_settings.Enabled || string.IsNullOrEmpty(genre))
                return null;

            string cacheKey = $"{MOVIES_BY_GENRE_CACHE_KEY_PREFIX}{genre}";
            if (_cache.TryGetValue(cacheKey, out List<MovieDto> movies))
            {
                Interlocked.Increment(ref _cacheHits);
                _logger.LogInformation("Cache hit for movies by genre {Genre}. Total hits: {CacheHits}", genre, _cacheHits);
                return movies;
            }

            Interlocked.Increment(ref _cacheMisses);
            _logger.LogInformation("Cache miss for movies by genre {Genre}. Total misses: {CacheMisses}", genre, _cacheMisses);
            return null;
        }

        /// <summary>
        /// Sets movies by genre in the cache.
        /// </summary>
        /// <param name="genre">The genre of the movies</param>
        /// <param name="movies">List of movies to cache</param>
        public void SetMoviesByGenre(string genre, List<MovieDto> movies)
        {
            if (!_settings.Enabled || string.IsNullOrEmpty(genre) || movies == null)
                return;

            string cacheKey = $"{MOVIES_BY_GENRE_CACHE_KEY_PREFIX}{genre}";
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.MoviesByGenreExpirationMinutes))
                .SetSlidingExpiration(TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes))
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(cacheKey, movies, cacheOptions);
            _logger.LogInformation("Cached {Count} movies for genre {Genre} for {Minutes} minutes",
                movies.Count, genre, _settings.MoviesByGenreExpirationMinutes);
        }

        /// <summary>
        /// Invalidates a movie in the cache by ID.
        /// Also invalidates the all movies and genre caches.
        /// </summary>
        /// <param name="id">The ID of the movie to invalidate</param>
        public void InvalidateMovie(string id)
        {
            if (!_settings.Enabled || string.IsNullOrEmpty(id))
                return;

            string cacheKey = $"{MOVIE_BY_ID_CACHE_KEY_PREFIX}{id}";

            // Check if the movie is in cache to get genre info before removing
            if (_cache.TryGetValue(cacheKey, out MovieDto movie) && movie?.Genres != null)
            {
                foreach (var genre in movie.Genres)
                {
                    InvalidateGenre(genre);
                }
            }

            // Remove the movie from cache
            _cache.Remove(cacheKey);

            // Also invalidate the all movies cache
            _cache.Remove(ALL_MOVIES_CACHE_KEY);

            _logger.LogInformation("Invalidated cache for movie ID {MovieId}", id);
        }

        /// <summary>
        /// Invalidates the cache for a specific genre.
        /// </summary>
        /// <param name="genre">The genre to invalidate</param>
        public void InvalidateGenre(string genre)
        {
            if (!_settings.Enabled || string.IsNullOrEmpty(genre))
                return;

            string cacheKey = $"{MOVIES_BY_GENRE_CACHE_KEY_PREFIX}{genre}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("Invalidated cache for genre {Genre}", genre);
        }

        /// <summary>
        /// Invalidates all movie caches.
        /// </summary>
        public void InvalidateAllMovies()
        {
            if (!_settings.Enabled)
                return;

            _cache.Remove(ALL_MOVIES_CACHE_KEY);
            _logger.LogInformation("Invalidated cache for all movies");
        }

        /// <summary>
        /// Gets cache statistics.
        /// </summary>
        /// <returns>Dictionary with cache statistics</returns>
        public Dictionary<string, int> GetCacheStatistics()
        {
            return new Dictionary<string, int>
            {
                { "CacheHits", _cacheHits },
                { "CacheMisses", _cacheMisses },
                { "HitRatio", _cacheHits + _cacheMisses > 0
                    ? (int)((_cacheHits / (double)(_cacheHits + _cacheMisses)) * 100)
                    : 0 }
            };
        }
    }

    /// <summary>
    /// Configuration settings for the movie cache.
    /// Values are populated from appsettings.json configuration.
    /// </summary>
    public class MovieCacheSettings
    {
        /// <summary>
        /// Indicates whether movie caching is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Expiration time in minutes for the all movies cache.
        /// </summary>
        public int AllMoviesExpirationMinutes { get; set; } = 15;

        /// <summary>
        /// Expiration time in minutes for individual movie caches.
        /// </summary>
        public int MovieByIdExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Expiration time in minutes for movies by genre caches.
        /// </summary>
        public int MoviesByGenreExpirationMinutes { get; set; } = 20;

        /// <summary>
        /// Sliding expiration time in minutes for all caches.
        /// Extends the cache lifetime on access.
        /// </summary>
        public int SlidingExpirationMinutes { get; set; } = 10;
    }
}