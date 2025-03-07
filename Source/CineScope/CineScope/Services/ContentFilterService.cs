using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CineScope.Services
{
    /// <summary>
    /// Service responsible for filtering inappropriate content in user-submitted text
    /// Implements IContentFilterService to provide content moderation capabilities
    /// </summary>
    public class ContentFilterService : IContentFilterService
    {
        private readonly IBannedWordRepository _bannedWordRepository;

        // Object used for thread-safe locking when accessing the cache
        private readonly object _cacheLock = new();

        // In-memory cache of banned words to reduce database lookups
        private Dictionary<string, BannedWordModel> _bannedWordsCache = new();

        // Timestamp of when the cache was last refreshed
        private DateTime _lastCacheRefresh;

        /// <summary>
        /// Constructor for ContentFilterService
        /// </summary>
        /// <param name="bannedWordRepository">Repository for accessing banned words data</param>
        public ContentFilterService(IBannedWordRepository bannedWordRepository)
        {
            _bannedWordRepository = bannedWordRepository;
            _lastCacheRefresh = DateTime.MinValue; // Set to ensure cache refreshes on first use
        }

        /// <summary>
        /// Determines if the provided text content is approved (contains no banned words)
        /// </summary>
        /// <param name="text">The text content to check</param>
        /// <returns>True if no banned words are found, false otherwise</returns>
        public async Task<bool> IsContentApprovedAsync(string text)
        {
            var flaggedWords = await GetFlaggedWordsAsync(text);
            return flaggedWords.Count == 0;
        }

        /// <summary>
        /// Identifies and returns a list of banned words found in the provided text
        /// </summary>
        /// <param name="text">The text content to check</param>
        /// <returns>List of banned words found in the text</returns>
        public async Task<List<string>> GetFlaggedWordsAsync(string text)
        {
            // Ensure cached list of banned words is current
            await RefreshBannedWordsCacheIfNeeded();

            // Split text into individual words for checking
            var words = text.Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var flaggedWords = new List<string>();

            // Check each word against the banned words cache
            foreach (var word in words)
            {
                string cleanedWord = CleanWord(word);
                if (!string.IsNullOrEmpty(cleanedWord) && _bannedWordsCache.ContainsKey(cleanedWord.ToLower()))
                {
                    flaggedWords.Add(cleanedWord);
                }
            }

            return flaggedWords;
        }

        /// <summary>
        /// Adds a new banned word to the database and updates the cache
        /// </summary>
        /// <param name="bannedWord">The banned word model to add</param>
        /// <returns>The created banned word with its generated ID</returns>
        public async Task<BannedWordModel> AddBannedWordAsync(BannedWordModel bannedWord)
        {
            // Ensure word is stored in lowercase for case-insensitive matching
            bannedWord.Word = bannedWord.Word.ToLower();
            bannedWord.AddedAt = DateTime.UtcNow;
            bannedWord.UpdatedAt = DateTime.UtcNow;

            // Save to database
            var result = await _bannedWordRepository.CreateAsync(bannedWord);

            // Update the in-memory cache in a thread-safe manner
            lock (_cacheLock)
            {
                if (_bannedWordsCache.ContainsKey(bannedWord.Word))
                {
                    _bannedWordsCache[bannedWord.Word] = bannedWord;
                }
                else
                {
                    _bannedWordsCache.Add(bannedWord.Word, bannedWord);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all banned words from the database
        /// </summary>
        /// <returns>A list of all banned words</returns>
        public async Task<List<BannedWordModel>> GetAllBannedWordsAsync()
        {
            return await _bannedWordRepository.GetAllAsync();
        }

        /// <summary>
        /// Gets a specific banned word by its ID
        /// </summary>
        /// <param name="id">The ID of the banned word to retrieve</param>
        /// <returns>The banned word if found, null otherwise</returns>
        public async Task<BannedWordModel> GetBannedWordAsync(string id)
        {
            return await _bannedWordRepository.GetByIdAsync(new ObjectId(id));
        }

        /// <summary>
        /// Updates an existing banned word in the database and cache
        /// </summary>
        /// <param name="id">The ID of the banned word to update</param>
        /// <param name="bannedWord">The updated banned word data</param>
        public async Task UpdateBannedWordAsync(string id, BannedWordModel bannedWord)
        {
            // Ensure word is stored in lowercase for case-insensitive matching
            bannedWord.Word = bannedWord.Word.ToLower();
            bannedWord.UpdatedAt = DateTime.UtcNow;

            // Update in database
            await _bannedWordRepository.UpdateAsync(new ObjectId(id), bannedWord);

            // Update the in-memory cache in a thread-safe manner
            lock (_cacheLock)
            {
                if (_bannedWordsCache.ContainsKey(bannedWord.Word))
                {
                    _bannedWordsCache[bannedWord.Word] = bannedWord;
                }
            }
        }

        /// <summary>
        /// Deletes a banned word from the database and cache
        /// </summary>
        /// <param name="id">The ID of the banned word to delete</param>
        public async Task DeleteBannedWordAsync(string id)
        {
            // Get the word first to know what to remove from cache
            var bannedWord = await _bannedWordRepository.GetByIdAsync(new ObjectId(id));
            if (bannedWord != null)
            {
                // Delete from database
                await _bannedWordRepository.DeleteAsync(new ObjectId(id));

                // Remove from cache in a thread-safe manner
                lock (_cacheLock)
                {
                    if (_bannedWordsCache.ContainsKey(bannedWord.Word))
                    {
                        _bannedWordsCache.Remove(bannedWord.Word);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the in-memory cache of banned words if it's stale
        /// </summary>
        /// <remarks>
        /// Cache is refreshed if it's more than 5 minutes old to balance 
        /// performance with content filter accuracy
        /// </remarks>
        private async Task RefreshBannedWordsCacheIfNeeded()
        {
            // Refresh cache if it's older than 5 minutes
            if (DateTime.UtcNow.Subtract(_lastCacheRefresh).TotalMinutes > 5)
            {
                // Get all banned words from the database
                var bannedWords = await _bannedWordRepository.GetAllAsync();
                var newCache = new Dictionary<string, BannedWordModel>();

                // Filter to only include active banned words
                foreach (var word in bannedWords.Where(w => w.IsActive))
                {
                    newCache[word.Word.ToLower()] = word;
                }

                // Replace the entire cache atomically in a thread-safe manner
                lock (_cacheLock)
                {
                    _bannedWordsCache = newCache;
                    _lastCacheRefresh = DateTime.UtcNow;
                }
            }
        }

        /// <summary>
        /// Cleans a word by removing non-alphanumeric characters to improve matching
        /// </summary>
        /// <param name="word">The word to clean</param>
        /// <returns>Cleaned word with only alphanumeric characters</returns>
        private static string CleanWord(string word)
        {
            // Remove non-alphanumeric characters
            return Regex.Replace(word, @"[^\w]", "");
        }
    }
}

