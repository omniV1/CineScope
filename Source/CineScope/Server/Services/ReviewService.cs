using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq;

namespace CineScope.Server.Services
{
    /// <summary>
    /// Service responsible for managing review-related operations.
    /// Handles data access and business logic for review entities.
    /// </summary>
    public class ReviewService
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
        /// Initializes a new instance of the ReviewService.
        /// </summary>
        /// <param name="mongoDbService">Injected MongoDB service</param>
        /// <param name="settings">Injected MongoDB settings</param>
        public ReviewService(IMongoDbService mongoDbService, IOptions<MongoDbSettings> settings)
        {
            _mongoDbService = mongoDbService;
            _settings = settings.Value;
        }

        /// <summary>
        /// Retrieves all reviews for a specific movie.
        /// </summary>
        /// <param name="movieId">The ID of the movie</param>
        /// <returns>A list of reviews for the specified movie</returns>
        public async Task<List<Review>> GetReviewsByMovieIdAsync(string movieId)
        {
            // Get the reviews collection
            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);

            // Find all reviews for the specified movie
            return await collection.Find(r => r.MovieId == movieId).ToListAsync();
        }

        /// <summary>
        /// Retrieves all reviews created by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A list of reviews by the specified user</returns>
        public async Task<List<Review>> GetReviewsByUserIdAsync(string userId)
        {
            // Get the reviews collection
            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);

            // Find all reviews by the specified user
            return await collection.Find(r => r.UserId == userId).ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific review by its ID.
        /// </summary>
        /// <param name="id">The ID of the review to retrieve</param>
        /// <returns>The review, or null if not found</returns>
        public async Task<Review> GetReviewByIdAsync(string id)
        {
            // Get the reviews collection
            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);

            // Find the review with the specified ID
            return await collection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Creates a new review in the database.
        /// </summary>
        /// <param name="review">The review data to create</param>
        /// <returns>The created review with assigned ID</returns>
        public async Task<Review> CreateReviewAsync(Review review)
        {
            // Get the reviews collection
            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);

            // Insert the new review into the database
            await collection.InsertOneAsync(review);

            // Return the created review (now with an ID)
            return review;
        }

        /// <summary>
        /// Updates an existing review in the database.
        /// </summary>
        /// <param name="id">The ID of the review to update</param>
        /// <param name="review">The updated review data</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public async Task<bool> UpdateReviewAsync(string id, Review review)
        {
            // Get the reviews collection
            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);

            // Replace the existing review document with the updated one
            var result = await collection.ReplaceOneAsync(r => r.Id == id, review);

            // Return true if at least one document was modified
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Deletes a review from the database.
        /// </summary>
        /// <param name="id">The ID of the review to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public async Task<bool> DeleteReviewAsync(string id)
        {
            // Get the reviews collection
            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);

            // Delete the review with the specified ID
            var result = await collection.DeleteOneAsync(r => r.Id == id);

            // Return true if at least one document was deleted
            return result.DeletedCount > 0;
        }
    }
}