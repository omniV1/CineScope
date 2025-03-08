using System.Collections.Generic;
using System.Linq;
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
        /// Validates content against the list of banned words.
        /// </summary>
        /// <param name="content">The text content to validate</param>
        /// <returns>A result object indicating if the content is approved and any violation details</returns>
        public async Task<ContentFilterResult> ValidateContentAsync(string content)
        {
            // Get the list of active banned words
            var bannedWords = await GetActiveBannedWordsAsync();

            // Initialize the result with default approval
            var result = new ContentFilterResult { IsApproved = true };

            // Convert content to lowercase for case-insensitive matching
            var lowerContent = content.ToLower();

            // Check content against each banned word
            foreach (var bannedWord in bannedWords)
            {
                // If the content contains the banned word
                if (lowerContent.Contains(bannedWord.Word.ToLower()))
                {
                    // Mark the content as not approved
                    result.IsApproved = false;

                    // Add the violating word to the list
                    result.ViolationWords.Add(bannedWord.Word);
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves all active banned words from the database.
        /// </summary>
        /// <returns>A list of active banned words</returns>
        private async Task<List<BannedWord>> GetActiveBannedWordsAsync()
        {
            // Get the banned words collection
            var collection = _mongoDbService.GetCollection<BannedWord>(_settings.BannedWordsCollectionName);

            // Find only active banned words
            return await collection.Find(w => w.IsActive).ToListAsync();
        }
    }

    /// <summary>
    /// Represents the result of a content filter validation.
    /// Contains approval status and details of any violations.
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
        public List<string> ViolationWords { get; set; } = new List<string>();
    }
}