using CineScope.Interfaces;
using CineScope.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Repositories
{
    public class BannedWordRepository : IBannedWordRepository
    {
        private readonly IMongoCollection<BannedWordModel> _collection;

        public BannedWordRepository(MongoDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<BannedWordModel>(settings.BannedWordsCollectionName);
        }

        public async Task<List<BannedWordModel>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<BannedWordModel> GetByIdAsync(ObjectId id)
        {
            return await _collection.Find(word => word.Id == id).FirstOrDefaultAsync();
        }

        public async Task<BannedWordModel> GetByWordAsync(string word)
        {
            return await _collection.Find(bannedWord =>
                bannedWord.Word.ToLower() == word.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<List<BannedWordModel>> GetByCategoryAsync(string category)
        {
            return await _collection.Find(word => word.Category == category).ToListAsync();
        }

        public async Task<BannedWordModel> CreateAsync(BannedWordModel bannedWord)
        {
            bannedWord.AddedAt = DateTime.UtcNow;
            bannedWord.UpdatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(bannedWord);
            return bannedWord;
        }

        public async Task UpdateAsync(ObjectId id, BannedWordModel updatedBannedWord)
        {
            updatedBannedWord.UpdatedAt = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(word => word.Id == id, updatedBannedWord);
        }

        public async Task DeleteAsync(ObjectId id)
        {
            await _collection.DeleteOneAsync(word => word.Id == id);
        }
    }
}