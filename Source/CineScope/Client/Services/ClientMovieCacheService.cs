using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CineScope.Shared.DTOs;
using Microsoft.JSInterop;
using System.Text.Json;

namespace CineScope.Client.Services
{
    /// <summary>
    /// Client-side caching service for movie data.
    /// Uses browser's localStorage for persistent caching between sessions.
    /// </summary>
    public class ClientMovieCacheService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        // Cache keys
        private const string ALL_MOVIES_CACHE_KEY = "AllMovies";
        private const string MOVIE_BY_ID_CACHE_KEY_PREFIX = "Movie_";
        private const string MOVIES_BY_GENRE_CACHE_KEY_PREFIX = "MoviesByGenre_";

        // Cache expiration in minutes
        private const int ALL_MOVIES_EXPIRATION_MINUTES = 10;
        private const int MOVIE_BY_ID_EXPIRATION_MINUTES = 20;
        private const int MOVIES_BY_GENRE_EXPIRATION_MINUTES = 15;

        /// <summary>
        /// Initializes a new instance of the ClientMovieCacheService.
        /// </summary>
        /// <param name="httpClient">HTTP client for API calls</param>
        /// <param name="jsRuntime">JavaScript runtime for localStorage access</param>
        public ClientMovieCacheService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Gets all movies with caching.
        /// </summary>
        /// <param name="forceRefresh">If true, bypasses cache and forces a fresh fetch</param>
        /// <returns>List of movies</returns>
        public async Task<List<MovieDto>> GetAllMoviesAsync(bool forceRefresh = false)
        {
            // If not forcing refresh, try to get from cache
            if (!forceRefresh)
            {
                var cachedData = await GetFromLocalStorageAsync<CachedData<List<MovieDto>>>(ALL_MOVIES_CACHE_KEY);
                if (cachedData != null && !IsCacheExpired(cachedData.Timestamp, ALL_MOVIES_EXPIRATION_MINUTES))
                {
                    Console.WriteLine("Retrieved all movies from client cache");
                    return cachedData.Data;
                }
            }

            // If force refresh or not in cache or expired, get from API
            Console.WriteLine("Fetching all movies from API");
            var movies = await _httpClient.GetFromJsonAsync<List<MovieDto>>("api/Movie");

            // Cache the results
            if (movies != null)
            {
                await SetInLocalStorageAsync(ALL_MOVIES_CACHE_KEY, new CachedData<List<MovieDto>>
                {
                    Data = movies,
                    Timestamp = DateTime.UtcNow
                });
            }

            return movies ?? new List<MovieDto>();
        }

        /// <summary>
        /// Gets a movie by ID with caching.
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <param name="forceRefresh">If true, bypasses cache and forces a fresh fetch</param>
        /// <returns>Movie details or null if not found</returns>
        public async Task<MovieDto> GetMovieByIdAsync(string id, bool forceRefresh = false)
        {
            string cacheKey = $"{MOVIE_BY_ID_CACHE_KEY_PREFIX}{id}";

            // If not forcing refresh, try to get from cache
            if (!forceRefresh)
            {
                var cachedData = await GetFromLocalStorageAsync<CachedData<MovieDto>>(cacheKey);
                if (cachedData != null && !IsCacheExpired(cachedData.Timestamp, MOVIE_BY_ID_EXPIRATION_MINUTES))
                {
                    Console.WriteLine($"Retrieved movie {id} from client cache");
                    return cachedData.Data;
                }
            }

            // If force refresh or not in cache or expired, get from API
            Console.WriteLine($"Fetching movie {id} from API");

            try
            {
                var movie = await _httpClient.GetFromJsonAsync<MovieDto>($"api/Movie/{id}");

                // Cache the results
                if (movie != null)
                {
                    await SetInLocalStorageAsync(cacheKey, new CachedData<MovieDto>
                    {
                        Data = movie,
                        Timestamp = DateTime.UtcNow
                    });
                }

                return movie;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching movie {id}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets movies by genre with caching.
        /// </summary>
        /// <param name="genre">Genre to filter by</param>
        /// <param name="forceRefresh">If true, bypasses cache and forces a fresh fetch</param>
        /// <returns>List of movies in the specified genre</returns>
        public async Task<List<MovieDto>> GetMoviesByGenreAsync(string genre, bool forceRefresh = false)
        {
            string cacheKey = $"{MOVIES_BY_GENRE_CACHE_KEY_PREFIX}{genre}";

            // If not forcing refresh, try to get from cache
            if (!forceRefresh)
            {
                var cachedData = await GetFromLocalStorageAsync<CachedData<List<MovieDto>>>(cacheKey);
                if (cachedData != null && !IsCacheExpired(cachedData.Timestamp, MOVIES_BY_GENRE_EXPIRATION_MINUTES))
                {
                    Console.WriteLine($"Retrieved movies for genre {genre} from client cache");
                    return cachedData.Data;
                }
            }

            // If force refresh or not in cache or expired, get from API
            Console.WriteLine($"Fetching movies for genre {genre} from API");

            try
            {
                var movies = await _httpClient.GetFromJsonAsync<List<MovieDto>>($"api/Movie/genre/{genre}");

                // Cache the results
                if (movies != null)
                {
                    await SetInLocalStorageAsync(cacheKey, new CachedData<List<MovieDto>>
                    {
                        Data = movies,
                        Timestamp = DateTime.UtcNow
                    });
                }

                return movies ?? new List<MovieDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching movies for genre {genre}: {ex.Message}");
                return new List<MovieDto>();
            }
        }

        /// <summary>
        /// Clears all movie-related cache entries.
        /// </summary>
        public async Task ClearAllMovieCacheAsync()
        {
            try
            {
                // Remove all movies cache
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", new object[] { ALL_MOVIES_CACHE_KEY });

                // We need to get all keys from localStorage to find and remove all movie and genre caches
                var allKeys = await _jsRuntime.InvokeAsync<string[]>("eval", new object[] {
                    @"
                    (function() {
                        var keys = [];
                        for (var i = 0; i < localStorage.length; i++) {
                            keys.push(localStorage.key(i));
                        }
                        return keys;
                    })()
                    "
                });

                // Remove all movie and genre cache entries
                foreach (var key in allKeys)
                {
                    if (key.StartsWith(MOVIE_BY_ID_CACHE_KEY_PREFIX) || key.StartsWith(MOVIES_BY_GENRE_CACHE_KEY_PREFIX))
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", new object[] { key });
                    }
                }

                Console.WriteLine("Cleared all movie cache entries");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a cache entry is expired based on its timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp when the entry was cached</param>
        /// <param name="expirationMinutes">The cache duration in minutes</param>
        /// <returns>True if the cache entry is expired, false otherwise</returns>
        private bool IsCacheExpired(DateTime timestamp, int expirationMinutes)
        {
            return DateTime.UtcNow.Subtract(timestamp).TotalMinutes > expirationMinutes;
        }

        /// <summary>
        /// Gets a value from localStorage.
        /// </summary>
        /// <typeparam name="T">The type of the value to get</typeparam>
        /// <param name="key">The key of the value to get</param>
        /// <returns>The value if found, null otherwise</returns>
        private async Task<T> GetFromLocalStorageAsync<T>(string key)
        {
            try
            {
                // Fixed: Correct parameter passing for JavaScript interop
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", new object[] { key });
                if (string.IsNullOrEmpty(json))
                    return default;

                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving from localStorage: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Sets a value in localStorage.
        /// </summary>
        /// <typeparam name="T">The type of the value to set</typeparam>
        /// <param name="key">The key of the value to set</param>
        /// <param name="value">The value to set</param>
        private async Task SetInLocalStorageAsync<T>(string key, T value)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                // Fixed: Correct parameter passing for JavaScript interop
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", new object[] { key, json });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to localStorage: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Generic class to store cached data with a timestamp.
    /// </summary>
    /// <typeparam name="T">The type of the cached data</typeparam>
    public class CachedData<T>
    {
        /// <summary>
        /// The cached data.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// The timestamp when the data was cached.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}