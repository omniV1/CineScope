using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace CineScope.Client.Services
{
    /// <summary>
    /// Dedicated service for caching movie poster URLs and their load status.
    /// This service reduces database and network requests for frequently accessed images.
    /// </summary>
    public class MoviePosterCacheService
    {
        private readonly IJSRuntime _jsRuntime;

        // Cache parameters
        private const string POSTER_CACHE_KEY = "MoviePosters";
        private const int CACHE_SIZE_LIMIT = 100; // Maximum number of posters to cache
        private const int POSTER_CACHE_DAYS = 7; // Cache expiration in days

        // In-memory cache for the current session
        private Dictionary<string, PosterCacheEntry> _inMemoryCache;
        private bool _isInitialized = false;

        public MoviePosterCacheService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _inMemoryCache = new Dictionary<string, PosterCacheEntry>();
        }

        /// <summary>
        /// Gets a cached poster URL for a movie, or returns the original URL if not cached.
        /// Also updates cache statistics for the URL.
        /// </summary>
        /// <param name="movieId">ID of the movie</param>
        /// <param name="originalUrl">Original URL of the poster</param>
        /// <returns>Cached URL or original URL</returns>
        public async Task<string> GetPosterUrlAsync(string movieId, string originalUrl)
        {
            // Initialize cache from localStorage if not already done
            if (!_isInitialized)
            {
                await InitializeCacheAsync();
            }

            // If we have a valid cache entry, return it and update stats
            if (_inMemoryCache.TryGetValue(movieId, out var entry) && !string.IsNullOrEmpty(entry.CachedUrl))
            {
                // Update access count and time
                entry.AccessCount++;
                entry.LastAccessed = DateTime.UtcNow;

                // Save updated stats to in-memory cache
                _inMemoryCache[movieId] = entry;

                // Asynchrously update localStorage without awaiting
                _ = Task.Run(async () => await SaveCacheEntryAsync(movieId, entry));

                Console.WriteLine($"Poster cache hit for movie {movieId}");
                return entry.CachedUrl;
            }

            // If not cached, create a new cache entry
            if (!_inMemoryCache.ContainsKey(movieId))
            {
                var newEntry = new PosterCacheEntry
                {
                    MovieId = movieId,
                    OriginalUrl = originalUrl,
                    CachedUrl = originalUrl, // Initially set to the original URL
                    FirstCached = DateTime.UtcNow,
                    LastAccessed = DateTime.UtcNow,
                    AccessCount = 1,
                    LoadSuccessful = null // Not yet determined
                };

                _inMemoryCache[movieId] = newEntry;

                // Save to localStorage without awaiting
                _ = Task.Run(async () => await SaveCacheEntryAsync(movieId, newEntry));

                Console.WriteLine($"Created new poster cache entry for movie {movieId}");
            }

            return originalUrl;
        }

        /// <summary>
        /// Records that a poster has loaded successfully.
        /// This prevents future attempts to load from alternative sources unnecessarily.
        /// </summary>
        /// <param name="movieId">ID of the movie</param>
        /// <param name="loadSuccessful">Whether the poster loaded successfully</param>
        public async Task RecordPosterLoadResultAsync(string movieId, bool loadSuccessful)
        {
            if (!_isInitialized)
            {
                await InitializeCacheAsync();
            }

            if (_inMemoryCache.TryGetValue(movieId, out var entry))
            {
                entry.LoadSuccessful = loadSuccessful;
                entry.LastAccessed = DateTime.UtcNow;

                // If load failed, and we have a fallback URL, use it
                if (!loadSuccessful && !string.IsNullOrEmpty(entry.FallbackUrl))
                {
                    entry.CachedUrl = entry.FallbackUrl;
                }

                _inMemoryCache[movieId] = entry;

                // Save to localStorage without awaiting
                _ = Task.Run(async () => await SaveCacheEntryAsync(movieId, entry));

                Console.WriteLine($"Recorded poster load result for movie {movieId}: {loadSuccessful}");
            }
        }

        /// <summary>
        /// Sets a fallback URL for a movie poster when the original URL fails to load.
        /// </summary>
        /// <param name="movieId">ID of the movie</param>
        /// <param name="fallbackUrl">Fallback URL to use if original fails</param>
        public async Task SetFallbackUrlAsync(string movieId, string fallbackUrl)
        {
            if (!_isInitialized)
            {
                await InitializeCacheAsync();
            }

            if (_inMemoryCache.TryGetValue(movieId, out var entry))
            {
                entry.FallbackUrl = fallbackUrl;

                // If we already know the load failed, use the fallback immediately
                if (entry.LoadSuccessful == false)
                {
                    entry.CachedUrl = fallbackUrl;
                }

                _inMemoryCache[movieId] = entry;

                // Save to localStorage without awaiting
                _ = Task.Run(async () => await SaveCacheEntryAsync(movieId, entry));
            }
            else
            {
                // Create a new entry with the fallback
                var newEntry = new PosterCacheEntry
                {
                    MovieId = movieId,
                    OriginalUrl = string.Empty, // We don't know the original URL yet
                    FallbackUrl = fallbackUrl,
                    CachedUrl = string.Empty, // Will be set when GetPosterUrlAsync is called
                    FirstCached = DateTime.UtcNow,
                    LastAccessed = DateTime.UtcNow,
                    AccessCount = 0,
                    LoadSuccessful = null
                };

                _inMemoryCache[movieId] = newEntry;

                // Save to localStorage without awaiting
                _ = Task.Run(async () => await SaveCacheEntryAsync(movieId, newEntry));
            }
        }

        /// <summary>
        /// Clears the poster cache entirely.
        /// </summary>
        public async Task ClearCacheAsync()
        {
            _inMemoryCache.Clear();
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", POSTER_CACHE_KEY);
            _isInitialized = false;

            Console.WriteLine("Poster cache cleared");
        }

        /// <summary>
        /// Initializes the cache from localStorage.
        /// </summary>
        private async Task InitializeCacheAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", POSTER_CACHE_KEY);

                if (!string.IsNullOrEmpty(json))
                {
                    var cachedEntries = JsonSerializer.Deserialize<Dictionary<string, PosterCacheEntry>>(json);

                    if (cachedEntries != null)
                    {
                        // Clean up expired entries
                        var now = DateTime.UtcNow;
                        var expirationDate = now.AddDays(-POSTER_CACHE_DAYS);

                        foreach (var entry in cachedEntries)
                        {
                            if (entry.Value.LastAccessed > expirationDate)
                            {
                                _inMemoryCache[entry.Key] = entry.Value;
                            }
                        }

                        Console.WriteLine($"Loaded {_inMemoryCache.Count} poster cache entries from localStorage");

                        // If we have too many entries, prune the cache
                        if (_inMemoryCache.Count > CACHE_SIZE_LIMIT)
                        {
                            PruneCache();
                        }
                    }
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing poster cache: {ex.Message}");
                _inMemoryCache = new Dictionary<string, PosterCacheEntry>();
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Saves the entire cache to localStorage.
        /// </summary>
        private async Task SaveCacheAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_inMemoryCache);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", POSTER_CACHE_KEY, json);

                Console.WriteLine($"Saved {_inMemoryCache.Count} poster cache entries to localStorage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving poster cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves a single cache entry to localStorage by updating the entire cache.
        /// </summary>
        private async Task SaveCacheEntryAsync(string movieId, PosterCacheEntry entry)
        {
            try
            {
                // Get current cache from localStorage
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", POSTER_CACHE_KEY);
                Dictionary<string, PosterCacheEntry> cachedEntries;

                if (!string.IsNullOrEmpty(json))
                {
                    cachedEntries = JsonSerializer.Deserialize<Dictionary<string, PosterCacheEntry>>(json);
                }
                else
                {
                    cachedEntries = new Dictionary<string, PosterCacheEntry>();
                }

                // Update or add the entry
                cachedEntries[movieId] = entry;

                // Prune if necessary
                if (cachedEntries.Count > CACHE_SIZE_LIMIT)
                {
                    // Find least recently accessed entries
                    var sorted = cachedEntries.OrderBy(e => e.Value.LastAccessed).ToList();

                    // Remove entries to get down to 80% of the limit
                    int targetCount = (int)(CACHE_SIZE_LIMIT * 0.8);
                    int removeCount = cachedEntries.Count - targetCount;

                    for (int i = 0; i < removeCount; i++)
                    {
                        cachedEntries.Remove(sorted[i].Key);
                    }
                }

                // Save back to localStorage
                json = JsonSerializer.Serialize(cachedEntries);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", POSTER_CACHE_KEY, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving poster cache entry: {ex.Message}");
            }
        }

        /// <summary>
        /// Prunes the cache to stay within size limits.
        /// Removes least recently accessed entries.
        /// </summary>
        private void PruneCache()
        {
            try
            {
                // Sort entries by last access time
                var sorted = _inMemoryCache.OrderBy(e => e.Value.LastAccessed).ToList();

                // Keep only the most recently accessed entries (80% of limit)
                int targetCount = (int)(CACHE_SIZE_LIMIT * 0.8);
                int removeCount = _inMemoryCache.Count - targetCount;

                for (int i = 0; i < removeCount; i++)
                {
                    _inMemoryCache.Remove(sorted[i].Key);
                }

                Console.WriteLine($"Pruned poster cache from {sorted.Count} to {_inMemoryCache.Count} entries");

                // Save the pruned cache
                _ = Task.Run(async () => await SaveCacheAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error pruning poster cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets statistics about the current poster cache.
        /// </summary>
        public PosterCacheStats GetCacheStats()
        {
            return new PosterCacheStats
            {
                TotalEntries = _inMemoryCache.Count,
                SuccessfulLoads = _inMemoryCache.Count(e => e.Value.LoadSuccessful == true),
                FailedLoads = _inMemoryCache.Count(e => e.Value.LoadSuccessful == false),
                UndeterminedLoads = _inMemoryCache.Count(e => e.Value.LoadSuccessful == null),
                AverageAccessCount = _inMemoryCache.Count > 0
                    ? _inMemoryCache.Average(e => e.Value.AccessCount)
                    : 0
            };
        }
    }

    /// <summary>
    /// Represents a cached movie poster entry.
    /// </summary>
    public class PosterCacheEntry
    {
        /// <summary>
        /// ID of the movie this poster belongs to.
        /// </summary>
        public string MovieId { get; set; }

        /// <summary>
        /// Original URL of the poster from the database.
        /// </summary>
        public string OriginalUrl { get; set; }

        /// <summary>
        /// Fallback URL to use if the original URL fails to load.
        /// </summary>
        public string FallbackUrl { get; set; }

        /// <summary>
        /// The currently used URL (either original or fallback).
        /// </summary>
        public string CachedUrl { get; set; }

        /// <summary>
        /// When this poster was first cached.
        /// </summary>
        public DateTime FirstCached { get; set; }

        /// <summary>
        /// When this poster was last accessed.
        /// </summary>
        public DateTime LastAccessed { get; set; }

        /// <summary>
        /// Number of times this poster has been accessed.
        /// </summary>
        public int AccessCount { get; set; }

        /// <summary>
        /// Whether the poster loaded successfully from its URL.
        /// Null means we don't know yet.
        /// </summary>
        public bool? LoadSuccessful { get; set; }
    }

    /// <summary>
    /// Statistics about the poster cache.
    /// </summary>
    public class PosterCacheStats
    {
        public int TotalEntries { get; set; }
        public int SuccessfulLoads { get; set; }
        public int FailedLoads { get; set; }
        public int UndeterminedLoads { get; set; }
        public double AverageAccessCount { get; set; }
    }
}