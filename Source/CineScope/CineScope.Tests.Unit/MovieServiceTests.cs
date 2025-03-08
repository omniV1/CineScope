using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using Microsoft.Extensions.Options;
using CineScope.Server.Services;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace CineScope.Tests.Unit
{
    public class MovieServiceTests
    {
        [Fact]
        public async Task GetAllMovies_ShouldReturnAllMovies()
        {
            // Arrange
            // Create test data - a list of sample movies
            var movies = new List<Movie>
            {
                new Movie
                {
                    Id = "1",
                    Title = "The Shawshank Redemption",
                    ReleaseDate = new DateTime(1994, 9, 23),
                    AverageRating = 5.0,
                    PosterUrl = "/images/placeholder.png"
                },
                new Movie
                {
                    Id = "2",
                    Title = "The Godfather",
                    ReleaseDate = new DateTime(1972, 3, 24),
                    AverageRating = 5.0,
                    PosterUrl = "/images/placeholder.png"
                }
            };

            // Create a mock MongoDB collection
            var mockCollection = new Mock<IMongoCollection<Movie>>();

            // Set up the Find method to return our test movies
            var mockCursor = new Mock<IAsyncCursor<Movie>>();
            mockCursor.Setup(c => c.Current).Returns(movies);
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Movie>>(),
                    It.IsAny<FindOptions<Movie, Movie>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Create a mock database service that returns our mock collection
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Movie>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock the settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                MoviesCollectionName = "Movies"
            });

            // Create the service to test
            var movieService = new MovieService(mockMongoDbService.Object, mockSettings.Object);

            // Act
            var result = await movieService.GetAllMoviesAsync();

            // Assert
            Assert.Equal(2, result.Count);  // We should get both movies
            Assert.Equal("The Shawshank Redemption", result[0].Title);
            Assert.Equal("The Godfather", result[1].Title);
        }
    }
}