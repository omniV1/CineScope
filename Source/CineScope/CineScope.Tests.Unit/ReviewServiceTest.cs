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
    public class ReviewServiceTests
    {
        [Fact]
        public async Task GetReviewsByMovieId_ShouldReturnAllReviewsForMovie()
        {
            // Arrange
            var movieId = "movie123";
            var reviews = new List<Review>
            {
                new Review
                {
                    Id = "1",
                    MovieId = movieId,
                    UserId = "user1",
                    Rating = 4.5,
                    Text = "Great movie!",
                    CreatedAt = DateTime.UtcNow,
                    Username = "User One"
                },
                new Review
                {
                    Id = "2",
                    MovieId = movieId,
                    UserId = "user2",
                    Rating = 5.0,
                    Text = "Excellent film!",
                    CreatedAt = DateTime.UtcNow,
                    Username = "User Two"
                }
            };

            // Mock review collection
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

            // Mock database service
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Review>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ReviewsCollectionName = "Reviews"
            });

            // Create the service to test
            var reviewService = new ReviewService(mockMongoDbService.Object, mockSettings.Object);

            // Act
            var result = await reviewService.GetReviewsByMovieIdAsync(movieId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Great movie!", result[0].Text);
            Assert.Equal("Excellent film!", result[1].Text);
            Assert.Equal(4.5, result[0].Rating);
            Assert.Equal(5.0, result[1].Rating);
        }

        [Fact]
        public async Task CreateReview_ShouldStoreReviewAndReturnIt()
        {
            // Arrange
            var review = new Review
            {
                MovieId = "movie123",
                UserId = "user1",
                Rating = 4.5,
                Text = "Great movie!",
                CreatedAt = DateTime.UtcNow
            };

            // Mock review collection for insert
            var mockCollection = new Mock<IMongoCollection<Review>>();
            mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Review>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Mock database service
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Review>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ReviewsCollectionName = "Reviews"
            });

            // Create the service to test
            var reviewService = new ReviewService(mockMongoDbService.Object, mockSettings.Object);

            // Act
            var result = await reviewService.CreateReviewAsync(review);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(review.MovieId, result.MovieId);
            Assert.Equal(review.UserId, result.UserId);
            Assert.Equal(review.Rating, result.Rating);
            Assert.Equal(review.Text, result.Text);
        }

        [Fact]
        public async Task UpdateReview_ShouldModifyReviewAndReturnTrue()
        {
            // Arrange
            var reviewId = "review123";
            var review = new Review
            {
                Id = reviewId,
                MovieId = "movie123",
                UserId = "user1",
                Rating = 5.0,
                Text = "Updated review text!"
            };

            // Mock review collection for update
            var mockCollection = new Mock<IMongoCollection<Review>>();
            mockCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Review>>(),
                    It.IsAny<Review>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

            // Mock database service
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Review>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ReviewsCollectionName = "Reviews"
            });

            // Create the service to test
            var reviewService = new ReviewService(mockMongoDbService.Object, mockSettings.Object);

            // Act
            var result = await reviewService.UpdateReviewAsync(reviewId, review);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteReview_ShouldRemoveReviewAndReturnTrue()
        {
            // Arrange
            var reviewId = "review123";

            // Mock review collection for delete
            var mockCollection = new Mock<IMongoCollection<Review>>();
            mockCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Review>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            // Mock database service
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Review>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ReviewsCollectionName = "Reviews"
            });

            // Create the service to test
            var reviewService = new ReviewService(mockMongoDbService.Object, mockSettings.Object);

            // Act
            var result = await reviewService.DeleteReviewAsync(reviewId);

            // Assert
            Assert.True(result);
        }
    }
}