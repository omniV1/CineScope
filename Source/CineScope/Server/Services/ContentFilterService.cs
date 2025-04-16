using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CineScope.Server.Services
{
    /// <summary>
    /// Service responsible for filtering user-generated content.
    /// Identifies and flags inappropriate content based on a list of banned words.
    /// </summary>
    public class ContentFilterService
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
        /// In-memory cache of banned words to improve performance.
        /// </summary>
        private List<BannedWord>? _cachedBannedWords;

        /// <summary>
        /// Timestamp of when the banned words cache was last updated.
        /// </summary>
        private DateTime _cacheLastUpdated = DateTime.MinValue;

        /// <summary>
        /// Cache duration in minutes before refreshing banned words.
        /// </summary>
        private const int CACHE_DURATION_MINUTES = 15;

        /// <summary>
        /// Initializes a new instance of the ContentFilterService.
        /// </summary>
        /// <param name="mongoDbService">Injected MongoDB service</param>
        /// <param name="settings">Injected MongoDB settings</param>
        public ContentFilterService(IMongoDbService mongoDbService, IOptions<MongoDbSettings> settings)
        {
            _mongoDbService = mongoDbService;
            _settings = settings.Value;
        }

        /// <summary>
        /// Validates content against the list of banned words and patterns.
        /// Implements enhanced detection techniques including word boundaries and common evasion tactics.
        /// </summary>
        /// <param name="content">The text content to validate</param>
        /// <returns>A result object indicating if the content is approved and any violation details</returns>
        public async Task<ContentFilterResult> ValidateContentAsync(string content)
        {
            // Initialize the result with default approval
            var result = new ContentFilterResult { IsApproved = true };

            // Skip validation for empty content
            if (string.IsNullOrWhiteSpace(content))
            {
                return result;
            }

            // Get the list of active banned words with caching
            var bannedWords = await GetActiveBannedWordsWithCachingAsync();

            // Preprocessing content to handle common evasion tactics
            var normalizedContent = NormalizeContent(content);

            // Check normalized content against each banned word
            foreach (var bannedWord in bannedWords)
            {
                var normalizedBannedWord = NormalizeContent(bannedWord.Word);

                // Use word boundary detection for more accurate matches
                // This avoids false positives where a banned word is part of a longer word
                var pattern = $"\\b{Regex.Escape(normalizedBannedWord)}\\b";

                if (Regex.IsMatch(normalizedContent, pattern, RegexOptions.IgnoreCase))
                {
                    // Mark the content as not approved
                    result.IsApproved = false;

                    // Add the violating word to the list
                    result.ViolationWords.Add(bannedWord.Word);

                    // Calculate the severity score based on the banned word's severity
                    result.SeverityScore += bannedWord.Severity;
                }
            }

            // Secondary checks for complex patterns (profanity with special characters, etc.)
            if (result.IsApproved)
            {
                result = await PerformSecondaryValidationAsync(normalizedContent, result);
            }

            return result;
        }

        /// <summary>
        /// Normalizes content to handle common evasion tactics.
        /// </summary>
        /// <param name="content">The content to normalize</param>
        /// <returns>Normalized content string</returns>
        private static string NormalizeContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            // Convert to lowercase
            string normalized = content.ToLowerInvariant();

            // Replace numbers and special characters used to bypass filters
            // 1 → l, 4 → a, 3 → e, 0 → o, $ → s, @ → a, etc.
            normalized = normalized
                .Replace("1", "i")
                .Replace("!", "i")
                .Replace("4", "a")
                .Replace("3", "e")
                .Replace("0", "o")
                .Replace("$", "s")
                .Replace("@", "a")
                .Replace("5", "s")
                .Replace("6", "g")
                .Replace("7", "t")
                .Replace("8", "b")
                .Replace("9", "g");

            // Remove spaces and special characters between letters (e.g., "b a d" → "bad")
            normalized = Regex.Replace(normalized, @"(\w)\s+(\w)", "$1$2");
            normalized = Regex.Replace(normalized, @"(\w)[^\w](\w)", "$1$2");

            // Remove repeated characters (e.g., "baaad" → "bad")
            normalized = Regex.Replace(normalized, @"(\w)\1{2,}", "$1");

            return normalized;
        }

        /// <summary>
        /// Performs additional validation checks for more complex patterns.
        /// </summary>
        /// <param name="normalizedContent">The normalized content</param>
        /// <param name="result">The current filter result</param>
        /// <returns>Updated filter result</returns>
        private static async Task<ContentFilterResult> PerformSecondaryValidationAsync(string normalizedContent, ContentFilterResult result)
        {
            // Check for combinations of words that together form inappropriate content
            // This could be implemented with a separate collection of banned phrases
            // For now, we'll use a simplified approach with hard-coded patterns

            // Example: Check for common offensive combinations without using explicit banned words
            string[] offensiveCombinations = new string[]
            {
                "hate.*people",
                "death to",
                "kill.*yourself",
                // Add more patterns as needed
            };

            foreach (var pattern in offensiveCombinations)
            {
                if (Regex.IsMatch(normalizedContent, pattern, RegexOptions.IgnoreCase))
                {
                    result.IsApproved = false;
                    result.ViolationWords.Add("Offensive phrase detected");
                    result.SeverityScore += 5; // High severity for offensive combinations
                    break;
                }
            }

            // Complete the task since this is an async method
            await Task.CompletedTask;

            return result;
        }

        /// <summary>
        /// Retrieves all active banned words from the database with caching.
        /// </summary>
        /// <returns>A list of active banned words</returns>
        private async Task<List<BannedWord>> GetActiveBannedWordsWithCachingAsync()
        {
            // Check if cache is valid
            if (_cachedBannedWords != null && DateTime.UtcNow.Subtract(_cacheLastUpdated).TotalMinutes < CACHE_DURATION_MINUTES)
            {
                return _cachedBannedWords;
            }

            // Cache expired or not initialized, fetch from database
            var collection = _mongoDbService.GetCollection<BannedWord>(_settings.BannedWordsCollectionName);
            var bannedWords = await collection.Find(w => w.IsActive).ToListAsync();

            // Update cache
            _cachedBannedWords = bannedWords;
            _cacheLastUpdated = DateTime.UtcNow;

            return bannedWords;
        }

        /// <summary>
        /// Manually refreshes the banned words cache.
        /// Useful after admin updates to banned word list.
        /// </summary>
        public async Task RefreshCacheAsync()
        {
            var collection = _mongoDbService.GetCollection<BannedWord>(_settings.BannedWordsCollectionName);
            _cachedBannedWords = await collection.Find(w => w.IsActive).ToListAsync();
            _cacheLastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Represents the result of a content filter validation.
    /// Contains approval status and details of any violations.
    /// Enhanced with severity scoring for graduated responses.
    /// </summary>
    public class ContentFilterResult
    {
        /// <summary>
        /// Indicates whether the content is approved (true) or rejected (false).
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// List of banned words found in the content.
        /// Empty if no violations are found.
        /// </summary>
        public List<string> ViolationWords { get; set; } = new();

        /// <summary>
        /// Cumulative severity score of all violations.
        /// Can be used for graduated responses (warning vs. rejection).
        /// </summary>
        public int SeverityScore { get; set; } = 0;

        /// <summary>
        /// Gets a user-friendly message explaining the reason for rejection.
        /// </summary>
        /// <returns>A message suitable for displaying to users</returns>
        public string GetUserFriendlyMessage()
        {
            if (IsApproved)
            {
                return "Content approved";
            }

            // Customize messages based on severity
            if (SeverityScore >= 10)
            {
                return "Your content contains highly inappropriate language and has been rejected.";
            }
            else if (SeverityScore >= 5)
            {
                return "Your content contains inappropriate language. Please revise before submitting.";
            }
            else
            {
                return "Your content may contain inappropriate language. Please review before submitting.";
            }
        }
    }
}