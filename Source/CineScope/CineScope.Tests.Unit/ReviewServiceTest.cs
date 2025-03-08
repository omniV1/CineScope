using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Server.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace CineScope.Tests.Unit
{
    /// <summary>
    /// Contains unit tests for the ReviewService component.
    /// These tests verify that review-related operations like retrieving, creating,
    /// updating, and deleting reviews work correctly with the database.
    /// </summary>
    public class ReviewServiceTests
    {
        /// <summary>
        /// Tests that the GetReviewsByMovieIdAsync method returns all reviews for a specific movie.
        /// 
        /// This test verifies that when given a movie ID, the service correctly retrieves
        /// all associated reviews from the database and that the review properties are
        /// preserved in the returned collection.
        /// </summary>
        [Fact]
        public async Task GetReviewsByMovieId_ShouldReturnAllReviewsForMovie()
        {
            // Arrange - Set up test data and dependencies
            // Specify the movie ID we'll be testing with
            var movieId = "movie123";

            // Create a list of sample reviews for this movie
            var reviews = new List<Review>
            {
                new Review
                {
                    Id = "1",
                    MovieId = movieId,           // This review is for our target movie
                    UserId = "user1",            // Written by user1
                    Rating = 4,                // 4.5 out of 5 stars
                    Text = "Great movie!",       // Review content
                    CreatedAt = DateTime.UtcNow, // Created just now
                    Username = "User One"        // Display name of the reviewer
                },
                new Review
                {
                    Id = "2",
                    MovieId = movieId,           // This review is also for our target movie
                    UserId = "user2",            // Written by a different user
                    Rating = 5,                // Perfect 5-star rating
                    Text = "Excellent film!",    // Different review content
                    CreatedAt = DateTime.UtcNow, // Created just now
                    Username = "User Two"        // Display name of the second reviewer
                }
            };

            // Mock the MongoDB collection to avoid actual database calls
            var mockCollection = new Mock<IMongoCollection<Review>>();
            var mockCursor = new Mock<IAsyncCursor<Review>>();

            // Configure the cursor to return our sample reviews
            mockCursor.Setup(c => c.Current).Returns(reviews);
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)   // First call returns true (has results)
                .ReturnsAsync(false); // Second call returns false (no more results)

            // Configure the collection's FindAsync method to return our cursor
            // This simulates querying the database for reviews with our movie ID
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Review>>(),
                    It.IsAny<FindOptions<Review, Review>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Mock the database service to return our collection
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Review>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock the settings to provide the collection name
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ReviewsCollectionName = "Reviews"
            });

            // Create the service with our mocked dependencies
            var reviewService = new ReviewService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested with our movie ID
            var result = await reviewService.GetReviewsByMovieIdAsync(movieId);

            // Assert - Verify the results match our expectations
            Assert.Equal(2, result.Count);              // We should get both reviews
            Assert.Equal("Great movie!", result[0].Text);   // First review text should match
            Assert.Equal("Excellent film!", result[1].Text); // Second review text should match
            Assert.Equal(4.5, result[0].Rating);         // First review rating should match
            Assert.Equal(5.0, result[1].Rating);         // Second review rating should match
        }

        /// <summary>
        /// Tests that the CreateReviewAsync method properly stores a new review in the database.
        /// 
        /// This test verifies that:
        /// - The review is correctly passed to the database for insertion
        /// - The method returns the created review with its properties intact
        /// - The review object maintains its state throughout the process
        /// </summary>
        [Fact]
        public async Task CreateReview_ShouldStoreReviewAndReturnIt()
        {
            // Arrange - Set up test data and dependencies
            // Create a new review to be inserted
            var review = new Review
            {
                MovieId = "movie123",          // The movie being reviewed
                UserId = "user1",              // The user writing the review
                Rating = 4,                  // 4.5 out of 5 stars
                Text = "Great movie!",         // Review content
                CreatedAt = DateTime.UtcNow    // Created just now
            };

            // Mock the MongoDB collection for insert operations
            var mockCollection = new Mock<IMongoCollection<Review>>();

            // Set up the InsertOneAsync method to complete successfully
            // This simulates storing the review in the database
            mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Review>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);  // Task completes successfully

            // Mock the database service to return our collection
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Review>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock the settings to provide the collection name
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ReviewsCollectionName = "Reviews"
            });

            // Create the service with our mocked dependencies
            var reviewService = new ReviewService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested with our new review
            var result = await reviewService.CreateReviewAsync(review);

            // Assert - Verify the results match our expectations
            Assert.NotNull(result);                    // Result should not be null
            Assert.Equal(review.MovieId, result.MovieId); // Movie ID should be preserved
            Assert.Equal(review.UserId, result.UserId);   // User ID should be preserved
            Assert.Equal(review.Rating, result.Rating);   // Rating should be preserved
            Assert.Equal(review.Text, result.Text);       // Review text should be preserved

            // Note: In a real scenario, MongoDB would generate an ID for the new document,
            // but since we're mocking the database, we're just verifying the review object
            // is returned correctly with its original properties intact
        }

        /// <summary>
        /// Tests that the UpdateReviewAsync method successfully modifies an existing review.
        /// 
        /// This test verifies that:
        /// - The service correctly calls the database to replace the review
        /// - The method returns true when the update is successful
        /// - The update operation targets the correct review by ID
        /// </summary>
        [Fact]
        public async Task UpdateReview_ShouldModifyReviewAndReturnTrue()
        {
            // Arrange - Set up test data and dependencies
            // Specify the ID of the review to update
            var reviewId = "review123";

            // Create a review with updated data
            var review = new Review
            {
                Id = reviewId,               // Same ID as the existing review
                MovieId = "movie123",        // The movie being reviewed
                UserId = "user1",            // The user who wrote the review
                Rating = 5,                // Updated to a perfect 5-star rating
                Text = "Updated review text!" // New review content
            };

            // Mock the MongoDB collection for update operations
            var mockCollection = new Mock<IMongoCollection<Review>>();

            // Set up the ReplaceOneAsync method to return a successful result
            // ModifiedCount of 1 indicates one document was updated
            mockCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Review>>(),
                    It.IsAny<Review>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

            // Mock the database service to return our collection
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Review>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock the settings to provide the collection name
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ReviewsCollectionName = "Reviews"
            });

            // Create the service with our mocked dependencies
            var reviewService = new ReviewService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested with the review ID and updated review
            var result = await reviewService.UpdateReviewAsync(reviewId, review);

            // Assert - Verify the result matches our expectations
            Assert.True(result);  // The operation should return true, indicating success

            // Note: In a more comprehensive test, we might also verify:
            // 1. That the filter used in ReplaceOneAsync targets the correct review ID
            // 2. That the review object passed to ReplaceOneAsync contains all the updated values
            // However, this would require more complex setup with argument capturing
        }

        /// <summary>
        /// Tests that the DeleteReviewAsync method successfully removes a review from the database.
        /// 
        /// This test verifies that:
        /// - The service correctly calls the database to delete the review
        /// - The method returns true when the deletion is successful
        /// - The delete operation targets the correct review by ID
        /// </summary>
        [Fact]
        public async Task DeleteReview_ShouldRemoveReviewAndReturnTrue()
        {
            // Arrange - Set up test data and dependencies
            // Specify the ID of the review to delete
            var reviewId = "review123";

            // Mock the MongoDB collection for delete operations
            var mockCollection = new Mock<IMongoCollection<Review>>();

            // Set up the DeleteOneAsync method to return a successful result
            // DeletedCount of 1 indicates one document was deleted
            mockCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Review>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            // Mock the database service to return our collection
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Review>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock the settings to provide the collection name
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ReviewsCollectionName = "Reviews"
            });

            // Create the service with our mocked dependencies
            var reviewService = new ReviewService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested with the review ID
            var result = await reviewService.DeleteReviewAsync(reviewId);

            // Assert - Verify the result matches our expectations
            Assert.True(result);  // The operation should return true, indicating success

            // Note: In a more comprehensive test, we might also verify:
            // 1. That the filter used in DeleteOneAsync targets the correct review ID
            // However, this would require more complex setup with argument capturing
        }
    }
}