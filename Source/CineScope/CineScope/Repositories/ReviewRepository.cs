using CineScope.Shared.Helpers;
using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Repositories
{
    /// <summary>
    /// Repository class for managing movie reviews in the application
    /// Implements IReviewRepository interface to handle CRUD operations for reviews
    /// </summary>
    public class ReviewRepository : IReviewRepository
    {
        private readonly IMongoCollection<ReviewModel> _reviews;

        /// <summary>
        /// Constructor for ReviewRepository
        /// </summary>
        /// <param name="settings">MongoDB connection and database settings</param>
        public ReviewRepository(MongoDBSettings settings)
        {
            // Initialize MongoDB client using the connection helper
            var client = MongoDbConnectionHelper.CreateClient(settings);

            // Get reference to the database
            var database = client.GetDatabase(settings.DatabaseName);

            // Get reference to the reviews collection
            _reviews = database.GetCollection<ReviewModel>(settings.ReviewsCollectionName);
        }

        /// <summary>
        /// Retrieves all reviews from the database
        /// </summary>
        /// <returns>A list of all reviews</returns>
        public async Task<List<ReviewModel>> GetAllAsync()
        {
            // Find all documents in the collection
            return await _reviews.Find(review => true).ToListAsync();
        }

        /// <summary>
        /// Finds a review by its unique identifier
        /// </summary>
        /// <param name="id">The ObjectId of the review to retrieve</param>
        /// <returns>The review if found, null otherwise</returns>
        public async Task<ReviewModel> GetByIdAsync(ObjectId id)
        {
            // Find the first document matching the given id
            return await _reviews.Find(review => review.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves all reviews created by a specific user
        /// </summary>
        /// <param name="userId">The ID of the user whose reviews to retrieve</param>
        /// <returns>A list of reviews created by the specified user</returns>
        public async Task<List<ReviewModel>> GetByUserIdAsync(ObjectId userId)
        {
            // Find all reviews that match the given user ID
            return await _reviews.Find(review => review.UserId == userId).ToListAsync();
        }

        /// <summary>
        /// Retrieves all reviews for a specific movie, sorted by creation date (newest first)
        /// </summary>
        /// <param name="movieId">The ID of the movie whose reviews to retrieve</param>
        /// <returns>A list of reviews for the specified movie</returns>
        public async Task<List<ReviewModel>> GetByMovieIdAsync(ObjectId movieId)
        {
            // Find all reviews for the given movie ID
            // Sort them by creation date in descending order (newest first)
            return await _reviews.Find(review => review.MovieId == movieId)
                .Sort(Builders<ReviewModel>.Sort.Descending(r => r.CreatedAt))
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new review entry in the database
        /// </summary>
        /// <param name="review">The review to create</param>
        /// <returns>The created review with generated ID</returns>
        public async Task<ReviewModel> CreateAsync(ReviewModel review)
        {
            // Insert the new review into the collection
            await _reviews.InsertOneAsync(review);

            // Return the inserted review (now with an ID)
            return review;
        }

        /// <summary>
        /// Updates an existing review in the database
        /// </summary>
        /// <param name="id">The ID of the review to update</param>
        /// <param name="updatedReview">The updated review data</param>
        public async Task UpdateAsync(ObjectId id, ReviewModel updatedReview)
        {
            // Replace the existing review document with the updated one
            await _reviews.ReplaceOneAsync(review => review.Id == id, updatedReview);
        }

        /// <summary>
        /// Deletes a review from the database
        /// </summary>
        /// <param name="id">The ID of the review to delete</param>
        public async Task DeleteAsync(ObjectId id)
        {
            // Delete the review document with the specified id
            await _reviews.DeleteOneAsync(review => review.Id == id);
        }
    }
}

