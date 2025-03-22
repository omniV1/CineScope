using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq;
using MongoDB.Bson;

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
            Console.WriteLine($"Querying database for reviews with movieId: {movieId}");

            try
            {
                // Get the reviews collection
                var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);

                // Ensure valid ObjectId and create a filter
                ObjectId objectId;
                if (ObjectId.TryParse(movieId, out objectId))
                {
                    // Create a proper filter definition for ObjectId comparison
                    // Log all reviews in the collection to debug
                    var allReviews = await collection.Find(_ => true).Limit(20).ToListAsync();
                    Console.WriteLine($"Available reviews in database: {allReviews.Count}");

                    // Use the correct field name from the MongoDB document - "movieId" (camelCase)
                    var filter = Builders<Review>.Filter.Eq("movieId", objectId);

                    // Log the filter for debugging
                    Console.WriteLine($"Using filter: {{ movieId: ObjectId('{objectId}') }}");

                    // Find all reviews for the specified movie
                    var reviews = await collection.Find(filter).ToListAsync();

                    Console.WriteLine($"Found {reviews.Count} reviews in database for movie {movieId}");
                    return reviews;
                }
                else
                {
                    Console.WriteLine($"Invalid ObjectId format for movieId: {movieId}");
                    return new List<Review>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving reviews: {ex.Message}");
                return new List<Review>();
            }
        }

        /// <summary>
        /// Retrieves all reviews created by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A list of reviews by the specified user</returns>
        public async Task<List<Review>> GetReviewsByUserIdAsync(string userId)
        {
            try
            {
                // Get the reviews collection
                var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);

                // Convert userId to ObjectId if valid
                ObjectId userObjectId;
                if (ObjectId.TryParse(userId, out userObjectId))
                {
                    // Create a proper filter definition for ObjectId comparison
                    var filter = Builders<Review>.Filter.Eq("UserId", userObjectId);

                    // Find all reviews by the specified user
                    return await collection.Find(filter).ToListAsync();
                }
                else
                {
                    Console.WriteLine($"Invalid ObjectId format for userId: {userId}");
                    return new List<Review>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user reviews: {ex.Message}");
                return new List<Review>();
            }
        }

        /// <summary>
        /// Retrieves a specific review by its ID.
        /// </summary>
        /// <param name="id">The ID of the review to retrieve</param>
        /// <returns>The review, or null if not found</returns>
        public async Task<Review> GetReviewByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                throw new FormatException($"'{id}' is not a valid 24-digit hex string.");
            }

            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);
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

        /// <summary>
        /// Calculates updated average rating for a movie based on all reviews.
        /// </summary>
        /// <param name="movieId">The ID of the movie</param>
        /// <returns>Success indicator</returns>
        public async Task<bool> UpdateMovieAverageRatingAsync(string movieId)
        {
            try
            {
                // Get the reviews collection
                var reviewsCollection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);

                // Ensure valid ObjectId for movieId
                ObjectId movieObjectId;
                if (!ObjectId.TryParse(movieId, out movieObjectId))
                {
                    return false;
                }

                // Create proper filter for ObjectId comparison
                var filter = Builders<Review>.Filter.Eq("MovieId", movieObjectId);

                // Get all reviews for the movie
                var reviews = await reviewsCollection.Find(filter).ToListAsync();

                // Calculate average rating (rounded to 1 decimal place)
                double averageRating = 0;
                if (reviews.Count > 0)
                {
                    // Use double for precise calculation
                    double totalRating = reviews.Sum(r => r.Rating);
                    averageRating = Math.Round(totalRating / reviews.Count, 1);
                }

                // Get the movies collection
                var moviesCollection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

                // Update the movie with the new average rating and review count
                var update = Builders<Movie>.Update
                    .Set(m => m.AverageRating, averageRating)
                    .Set(m => m.ReviewCount, reviews.Count);

                var result = await moviesCollection.UpdateOneAsync(m => m.Id == movieId, update);

                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating movie average rating: {ex.Message}");
                return false;
            }
        }
    }
}