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
        /// </summary>
        [Fact]
        public async Task GetReviewsByMovieId_ShouldReturnAllReviewsForMovie()
        {
            // Arrange - Set up test data and dependencies
            var movieId = "movie123";
            var reviews = new List<Review>
{
    new Review
    {
        Id = "1",
        MovieId = movieId,
        UserId = "user1",
        Rating = 4, // Fix this to match the expected value in the test assertion
        Text = "Great movie!",
        CreatedAt = DateTime.UtcNow,
        Username = "User One"
    }

};

            var mockCollection = new Mock<IMongoCollection<Review>>();
            var mockCursor = new Mock<IAsyncCursor<Review>>();

            mockCursor.Setup(c => c.Current).Returns(reviews);
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Review>>(),
                    It.IsAny<FindOptions<Review, Review>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Review>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ReviewsCollectionName = "Reviews"
            });

            var reviewService = new ReviewService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested
            var result = await reviewService.GetReviewsByMovieIdAsync(movieId);

            // Assert - Verify the results match our expectations
            Assert.Equal(2, result.Count);
            Assert.Equal("Great movie!", result[0].Text);
            Assert.Equal("Excellent film!", result[1].Text);
            Assert.Equal(4, result[0].Rating); // Changed from 4.5 to match the actual test data
            Assert.Equal(5, result[1].Rating);
        }

        /// <summary>
        /// Tests that the CreateReviewAsync method properly stores a new review in the database.
        /// </summary>
        [Fact]
        public async Task CreateReview_ShouldStoreReviewAndReturnIt()
        {
            // Arrange - Set up test data and dependencies
            var review = new Review
            {
                MovieId = "movie123",
                UserId = "user1",
                Rating = 4,
                Text = "Great movie!",
                CreatedAt = DateTime.UtcNow
            };

            var mockCollection = new Mock<IMongoCollection<Review>>();

            mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Review>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Review>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ReviewsCollectionName = "Reviews"
            });

            var reviewService = new ReviewService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested
            var result = await reviewService.CreateReviewAsync(review);

            // Assert - Verify the results match our expectations
            Assert.NotNull(result);
            Assert.Equal(review.MovieId, result.MovieId);
            Assert.Equal(review.UserId, result.UserId);
            Assert.Equal(review.Rating, result.Rating);
            Assert.Equal(review.Text, result.Text);
        }

        /// <summary>
        /// Tests that the UpdateReviewAsync method successfully modifies an existing review.
        /// </summary>
        [Fact]
        public async Task UpdateReview_ShouldModifyReviewAndReturnTrue()
        {
            // Arrange - Set up test data and dependencies
            var reviewId = "review123";

            var review = new Review
            {
                Id = reviewId,
                MovieId = "movie123",
                UserId = "user1",
                Rating = 5,
                Text = "Updated review text!"
            };

            var mockCollection = new Mock<IMongoCollection<Review>>();

            mockCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Review>>(),
                    It.IsAny<Review>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Review>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ReviewsCollectionName = "Reviews"
            });

            var reviewService = new ReviewService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested
            var result = await reviewService.UpdateReviewAsync(reviewId, review);

            // Assert - Verify the result matches our expectations
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the DeleteReviewAsync method successfully removes a review from the database.
        /// </summary>
        [Fact]
        public async Task DeleteReview_ShouldRemoveReviewAndReturnTrue()
        {
            // Arrange - Set up test data and dependencies
            var reviewId = "review123";

            var mockCollection = new Mock<IMongoCollection<Review>>();

            mockCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Review>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Review>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ReviewsCollectionName = "Reviews"
            });

            var reviewService = new ReviewService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested
            var result = await reviewService.DeleteReviewAsync(reviewId);

            // Assert - Verify the result matches our expectations
            Assert.True(result);
        }
    }
}