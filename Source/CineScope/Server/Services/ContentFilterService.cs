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
    public class ContentFilterService
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly MongoDbSettings _settings;

        public ContentFilterService(IMongoDbService mongoDbService, IOptions<MongoDbSettings> settings)
        {
            _mongoDbService = mongoDbService;
            _settings = settings.Value;
        }

        public async Task<ContentFilterResult> ValidateContentAsync(string content)
        {
            var bannedWords = await GetActiveBannedWordsAsync();
            var result = new ContentFilterResult { IsApproved = true };

            // Convert content to lowercase for case-insensitive matching
            var lowerContent = content.ToLower();

            foreach (var bannedWord in bannedWords)
            {
                // No need to check IsActive again since GetActiveBannedWordsAsync already filters
                if (lowerContent.Contains(bannedWord.Word.ToLower()))
                {
                    result.IsApproved = false;
                    result.ViolationWords.Add(bannedWord.Word);
                }
            }

            return result;
        }

        private async Task<List<BannedWord>> GetActiveBannedWordsAsync()
        {
            var collection = _mongoDbService.GetCollection<BannedWord>(_settings.BannedWordsCollectionName);
            // Get only active banned words
            return await collection.Find(w => w.IsActive).ToListAsync();
        }
    }

    public class ContentFilterResult
    {
        public bool IsApproved { get; set; }
        public List<string> ViolationWords { get; set; } = new List<string>();
    }
}