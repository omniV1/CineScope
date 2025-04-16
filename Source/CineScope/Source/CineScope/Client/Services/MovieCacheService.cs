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
        private readonly SemaphoreSlim _posterSemaphore = new(3); // Limit concurrent poster downloads
        private Task _initTask;
        private bool _initialized;

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
            _initTask = InitializeIndexedDB();
        }

        private async Task InitializeIndexedDB()
        {
            if (_initialized) return;

            try
            {
                await _jsRuntime.InvokeVoidAsync("eval", @"
                    if (!window.indexedDB) {
                        console.log('IndexedDB not supported');
                    }
                    else if (!window.cineScopeDB) {
                        window.cineScopeDB = new Promise((resolve, reject) => {
                            let request = indexedDB.open('CineScopeCache', 1);
                            
                            request.onerror = () => reject(request.error);
                            request.onsuccess = () => resolve(request.result);
                            
                            request.onupgradeneeded = (event) => {
                                let db = event.target.result;
                                if (!db.objectStoreNames.contains('posters')) {
                                    db.createObjectStore('posters');
                                }
                            };
                        });
                    }");
                
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing IndexedDB: {ex.Message}");
                // Fall back to localStorage for posters if IndexedDB fails
                _initialized = true;
            }
        }

        private async Task EnsureInitialized()
        {
            if (!_initialized)
            {
                await _initTask;
            }
        }

        // Movie Data Caching Methods
        public async Task<List<MovieDto>?> GetCachedMoviesAsync()
        {
            await EnsureInitialized();
            return await GetFromLocalStorageAsync<List<MovieDto>>(ALL_MOVIES_CACHE_KEY);
        }

        public async Task CacheMoviesAsync(List<MovieDto> movies)
        {
            await EnsureInitialized();
            await SetInLocalStorageAsync(ALL_MOVIES_CACHE_KEY, movies);
        }

        public async Task<MovieDto?> GetCachedMovieByIdAsync(string id)
        {
            await EnsureInitialized();
            return await GetFromLocalStorageAsync<MovieDto>($"{MOVIE_BY_ID_CACHE_KEY_PREFIX}{id}");
        }

        public async Task CacheMovieByIdAsync(string id, MovieDto movie)
        {
            await EnsureInitialized();
            await SetInLocalStorageAsync($"{MOVIE_BY_ID_CACHE_KEY_PREFIX}{id}", movie);
        }

        public async Task<List<MovieDto>?> GetCachedMoviesByGenreAsync(string genre)
        {
            await EnsureInitialized();
            return await GetFromLocalStorageAsync<List<MovieDto>>($"{MOVIES_BY_GENRE_CACHE_KEY_PREFIX}{genre}");
        }

        public async Task CacheMoviesByGenreAsync(string genre, List<MovieDto> movies)
        {
            await EnsureInitialized();
            await SetInLocalStorageAsync($"{MOVIES_BY_GENRE_CACHE_KEY_PREFIX}{genre}", movies);
        }

        // Optimized Poster Image Caching Methods
        public async Task<string?> GetCachedPosterAsync(string movieId)
        {
            await EnsureInitialized();
            try
            {
                var result = await _jsRuntime.InvokeAsync<string>("eval", @"
                    (async () => {
                        try {
                            if (!window.cineScopeDB) return null;
                            const db = await window.cineScopeDB;
                            return new Promise((resolve, reject) => {
                                const tx = db.transaction('posters', 'readonly');
                                const store = tx.objectStore('posters');
                                const request = store.get('" + movieId + @"');
                                request.onsuccess = () => resolve(request.result || null);
                                request.onerror = () => reject(request.error);
                            });
                        } catch (e) {
                            console.error('Error getting poster:', e);
                            return null;
                        }
                    })()");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving poster from IndexedDB: {ex.Message}");
                return null;
            }
        }

        public async Task CachePosterAsync(string movieId, string posterUrl)
        {
            await EnsureInitialized();
            try
            {
                await _jsRuntime.InvokeVoidAsync("eval", @"
                    (async () => {
                        try {
                            if (!window.cineScopeDB) return;
                            const db = await window.cineScopeDB;
                            return new Promise((resolve, reject) => {
                                const tx = db.transaction('posters', 'readwrite');
                                const store = tx.objectStore('posters');
                                const request = store.put('" + posterUrl + @"', '" + movieId + @"');
                                request.onsuccess = () => resolve();
                                request.onerror = () => reject(request.error);
                            });
                        } catch (e) {
                            console.error('Error caching poster:', e);
                        }
                    })()");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error caching poster in IndexedDB: {ex.Message}");
            }
        }

        public async Task<string> GetPosterWithCachingAsync(string movieId, string posterUrl)
        {
            await EnsureInitialized();

            // Try to get from cache first
            var cachedPoster = await GetCachedPosterAsync(movieId);
            if (!string.IsNullOrEmpty(cachedPoster))
            {
                return cachedPoster;
            }

            try
            {
                // Use semaphore to limit concurrent downloads
                await _posterSemaphore.WaitAsync();
                
                // Double-check cache after acquiring semaphore
                cachedPoster = await GetCachedPosterAsync(movieId);
                if (!string.IsNullOrEmpty(cachedPoster))
                {
                    return cachedPoster;
                }

                // Store the original URL in cache
                await CachePosterAsync(movieId, posterUrl);
                return posterUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching poster for movie {movieId}: {ex.Message}");
                return posterUrl;
            }
            finally
            {
                _posterSemaphore.Release();
            }
        }

        public async Task PreloadPostersAsync(List<MovieDto> movies)
        {
            await EnsureInitialized();
            try
            {
                // Process movies in batches
                for (int i = 0; i < movies.Count; i += POSTER_BATCH_SIZE)
                {
                    var batch = movies.Skip(i).Take(POSTER_BATCH_SIZE);
                    var tasks = batch.Select(async movie =>
                    {
                        if (string.IsNullOrEmpty(movie.PosterUrl)) return;
                        await GetPosterWithCachingAsync(movie.Id, movie.PosterUrl);
                    });
                    
                    await Task.WhenAll(tasks);
                    
                    // Small delay between batches to prevent overwhelming
                    if (i + POSTER_BATCH_SIZE < movies.Count)
                    {
                        await Task.Delay(100);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error preloading posters: {ex.Message}");
            }
        }

        // Cache Management Methods
        public async Task ClearAllCacheAsync()
        {
            try
            {
                var allKeys = await _jsRuntime.InvokeAsync<string[]>("eval", @"
                    (function() {
                        var keys = [];
                        for (var i = 0; i < localStorage.length; i++) {
                            keys.push(localStorage.key(i));
                        }
                        return keys;
                    })()");

                foreach (var key in allKeys)
                {
                    if (key.StartsWith(MOVIE_BY_ID_CACHE_KEY_PREFIX) ||
                        key.StartsWith(MOVIES_BY_GENRE_CACHE_KEY_PREFIX) ||
                        key.StartsWith(POSTER_CACHE_KEY_PREFIX) ||
                        key == ALL_MOVIES_CACHE_KEY)
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
                    }
                }

                _logger.LogInformation("Cleared all movie and poster cache entries");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing cache: {ex.Message}");
            }
        }

        private async Task<T?> GetFromLocalStorageAsync<T>(string key)
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                if (string.IsNullOrEmpty(json)) return default;

                var data = JsonSerializer.Deserialize<CacheEntry<T>>(json);
                if (data == null || IsCacheExpired(data.Timestamp, 
                    key.StartsWith(POSTER_CACHE_KEY_PREFIX) ? POSTER_CACHE_DURATION_MINUTES : MOVIE_CACHE_DURATION_MINUTES))
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
                    return default;
                }

                return data.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving from cache: {ex.Message}");
                return default;
            }
        }

        private async Task SetInLocalStorageAsync<T>(string key, T value)
        {
            try
            {
                var cacheEntry = new CacheEntry<T>
                {
                    Value = value,
                    Timestamp = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(cacheEntry);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting cache: {ex.Message}");
            }
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
            _posterSemaphore.Dispose();
        }
    }
} 