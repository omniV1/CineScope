using System.Text.Json;
using Microsoft.JSInterop;
using CineScope.Shared.DTOs;

namespace CineScope.Client.Services
{
    public class MovieCacheService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MovieCacheService> _logger;
        private readonly Dictionary<string, string> _posterCache = new();
        private readonly Dictionary<string, CacheEntry<MovieDto>> _movieCache = new();
        private readonly Dictionary<string, CacheEntry<List<MovieDto>>> _moviesListCache = new();

        // Cache keys
        private const string ALL_MOVIES_CACHE_KEY = "all_movies";
        private const string MOVIE_BY_ID_CACHE_KEY_PREFIX = "movie_";
        private const string MOVIES_BY_GENRE_CACHE_KEY_PREFIX = "genre_";
        private const string POSTER_CACHE_KEY_PREFIX = "poster_";

        // Cache durations
        private const int MOVIE_CACHE_DURATION_MINUTES = 30;
        private const int POSTER_CACHE_DURATION_MINUTES = 60;

        // Batch size for poster loading
        private const int POSTER_BATCH_SIZE = 5;

        public MovieCacheService(IJSRuntime jsRuntime, HttpClient httpClient, ILogger<MovieCacheService> logger)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
            _logger = logger;
        }

        // Movie Data Caching Methods
        public async Task<List<MovieDto>?> GetCachedMoviesAsync()
        {
            if (_moviesListCache.TryGetValue(ALL_MOVIES_CACHE_KEY, out var cacheEntry))
            {
                if (!IsCacheExpired(cacheEntry.Timestamp, MOVIE_CACHE_DURATION_MINUTES))
                {
                    return cacheEntry.Value;
                }
                _moviesListCache.Remove(ALL_MOVIES_CACHE_KEY);
            }
            return null;
        }

        public async Task CacheMoviesAsync(List<MovieDto> movies)
        {
            _moviesListCache[ALL_MOVIES_CACHE_KEY] = new CacheEntry<List<MovieDto>>
            {
                Value = movies,
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<MovieDto?> GetCachedMovieByIdAsync(string id)
        {
            if (_movieCache.TryGetValue($"{MOVIE_BY_ID_CACHE_KEY_PREFIX}{id}", out var cacheEntry))
            {
                if (!IsCacheExpired(cacheEntry.Timestamp, MOVIE_CACHE_DURATION_MINUTES))
                {
                    return cacheEntry.Value;
                }
                _movieCache.Remove($"{MOVIE_BY_ID_CACHE_KEY_PREFIX}{id}");
            }
            return null;
        }

        public async Task CacheMovieByIdAsync(string id, MovieDto movie)
        {
            _movieCache[$"{MOVIE_BY_ID_CACHE_KEY_PREFIX}{id}"] = new CacheEntry<MovieDto>
            {
                Value = movie,
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<List<MovieDto>?> GetCachedMoviesByGenreAsync(string genre)
        {
            if (_moviesListCache.TryGetValue($"{MOVIES_BY_GENRE_CACHE_KEY_PREFIX}{genre}", out var cacheEntry))
            {
                if (!IsCacheExpired(cacheEntry.Timestamp, MOVIE_CACHE_DURATION_MINUTES))
                {
                    return cacheEntry.Value;
                }
                _moviesListCache.Remove($"{MOVIES_BY_GENRE_CACHE_KEY_PREFIX}{genre}");
            }
            return null;
        }

        public async Task CacheMoviesByGenreAsync(string genre, List<MovieDto> movies)
        {
            _moviesListCache[$"{MOVIES_BY_GENRE_CACHE_KEY_PREFIX}{genre}"] = new CacheEntry<List<MovieDto>>
            {
                Value = movies,
                Timestamp = DateTime.UtcNow
            };
        }

        // Poster Caching Methods
        public async Task<string> GetPosterWithCachingAsync(string movieId, string posterUrl)
        {
            // Check memory cache first
            if (_posterCache.TryGetValue(movieId, out var cachedUrl))
            {
                return cachedUrl;
            }

            // If the URL is from media-amazon.com or GitHub, use it directly
            if (posterUrl.Contains("m.media-amazon.com") || 
                posterUrl.Contains("githubusercontent.com"))
            {
                _posterCache[movieId] = posterUrl;
                return posterUrl;
            }

            // For GitHub URLs, convert to raw URL
            if (posterUrl.Contains("github.com"))
            {
                var rawUrl = posterUrl.Replace("github.com", "raw.githubusercontent.com")
                                    .Replace("/blob/", "/");
                _posterCache[movieId] = rawUrl;
                return rawUrl;
            }

            // For other URLs or if URL is empty, use a placeholder
            var fallbackUrl = $"https://placehold.co/300x450/1a1a1a/white?text={Uri.EscapeDataString(movieId)}";
            _posterCache[movieId] = fallbackUrl;
            return fallbackUrl;
        }

        public async Task ClearCacheAsync()
        {
            _posterCache.Clear();
            _movieCache.Clear();
            _moviesListCache.Clear();
        }

        private bool IsCacheExpired(DateTime timestamp, int expirationMinutes)
        {
            return DateTime.UtcNow.Subtract(timestamp).TotalMinutes > expirationMinutes;
        }

        private class CacheEntry<T>
        {
            public T Value { get; set; } = default!;
            public DateTime Timestamp { get; set; }
        }

        public async ValueTask DisposeAsync()
        {
            await ClearCacheAsync();
        }
    }
} 