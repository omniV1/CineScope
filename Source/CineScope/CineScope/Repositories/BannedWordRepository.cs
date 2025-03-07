using CineScope.Shared.Helpers;
using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Repositories
{
    /// <summary>
    /// Repository class for managing banned words in the application
    /// Implements IBannedWordRepository interface to handle CRUD operations for banned words
    /// </summary>
    public class BannedWordRepository : IBannedWordRepository
    {
        private readonly IMongoCollection<BannedWordModel> _collection;

        /// <summary>
        /// Constructor for BannedWordRepository
        /// </summary>
        /// <param name="settings">MongoDB connection and database settings</param>
        public BannedWordRepository(MongoDBSettings settings)
        {
            // Initialize MongoDB client using the connection helper
            var client = MongoDbConnectionHelper.CreateClient(settings);

            // Get reference to the database
            var database = client.GetDatabase(settings.DatabaseName);

            // Get reference to the banned words collection
            _collection = database.GetCollection<BannedWordModel>(settings.BannedWordsCollectionName);
        }

        /// <summary>
        /// Retrieves all banned words from the database
        /// </summary>
        /// <returns>A list of all banned words</returns>
        public async Task<List<BannedWordModel>> GetAllAsync()
        {
            // Find all documents in the collection
            return await _collection.Find(_ => true).ToListAsync();
        }

        /// <summary>
        /// Finds a banned word by its unique identifier
        /// </summary>
        /// <param name="id">The ObjectId of the banned word to retrieve</param>
        /// <returns>The banned word if found, null otherwise</returns>
        public async Task<BannedWordModel> GetByIdAsync(ObjectId id)
        {
            // Find the first document matching the given id
            return await _collection.Find(word => word.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Finds a banned word by its text content
        /// </summary>
        /// <param name="word">The word text to search for</param>
        /// <returns>The banned word if found, null otherwise</returns>
        public async Task<BannedWordModel> GetByWordAsync(string word)
        {
            // Find the word, with case-insensitive comparison
            return await _collection.Find(bannedWord =>
                bannedWord.Word.ToLower() == word.ToLower()).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves all banned words of a specific category
        /// </summary>
        /// <param name="category">The category to filter by</param>
        /// <returns>A list of banned words in the specified category</returns>
        public async Task<List<BannedWordModel>> GetByCategoryAsync(string category)
        {
            // Find all documents that match the given category
            return await _collection.Find(word => word.Category == category).ToListAsync();
        }

        /// <summary>
        /// Creates a new banned word entry in the database
        /// </summary>
        /// <param name="bannedWord">The banned word to create</param>
        /// <returns>The created banned word with generated ID and timestamps</returns>
        public async Task<BannedWordModel> CreateAsync(BannedWordModel bannedWord)
        {
            // Set the current timestamp for both added and updated dates
            bannedWord.AddedAt = DateTime.UtcNow;
            bannedWord.UpdatedAt = DateTime.UtcNow;

            // Insert the new document into the collection
            await _collection.InsertOneAsync(bannedWord);

            // Return the inserted banned word (now with an ID)
            return bannedWord;
        }

        /// <summary>
        /// Updates an existing banned word in the database
        /// </summary>
        /// <param name="id">The ID of the banned word to update</param>
        /// <param name="updatedBannedWord">The updated banned word data</param>
        public async Task UpdateAsync(ObjectId id, BannedWordModel updatedBannedWord)
        {
            // Update the timestamp to track when the word was last modified
            updatedBannedWord.UpdatedAt = DateTime.UtcNow;

            // Replace the existing document with the updated one
            await _collection.ReplaceOneAsync(word => word.Id == id, updatedBannedWord);
        }

        /// <summary>
        /// Deletes a banned word from the database
        /// </summary>
        /// <param name="id">The ID of the banned word to delete</param>
        public async Task DeleteAsync(ObjectId id)
        {
            // Delete the document with the specified id
            await _collection.DeleteOneAsync(word => word.Id == id);
        }
    }
}
