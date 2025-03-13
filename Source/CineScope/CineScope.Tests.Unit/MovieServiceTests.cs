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
    /// <summary>
    /// Contains unit tests for the MovieService component.
    /// These tests verify that the service correctly interacts with the database
    /// and processes movie data.
    /// </summary>
    public class MovieServiceTests
    {
        /// <summary>
        /// Tests that the GetAllMoviesAsync method returns all movies from the database.
        /// 
        /// This test verifies that:
        /// - The service correctly queries the database
        /// - The service properly maps database models to DTOs
        /// - All movies from the database are included in the result
        /// </summary>
        [Fact]
        public async Task GetAllMovies_ShouldReturnAllMovies()
        {
            // Arrange - Set up test data and dependencies
            // Create a list of sample movies to be returned from the database
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

            // Set up the cursor to return our sample movies
            var mockCursor = new Mock<IAsyncCursor<Movie>>();
            mockCursor.Setup(c => c.Current).Returns(movies);
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)   // First call returns true (has results)
                .ReturnsAsync(false); // Second call returns false (no more results)

            // Configure the collection's FindAsync method to return our cursor
            // This simulates querying the database and getting our test movies
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Movie>>(),
                    It.IsAny<FindOptions<Movie, Movie>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Mock the database service to return our collection
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<Movie>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock the settings to provide the collection name
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                MoviesCollectionName = "Movies"
            });

            // Create the service with our mocked dependencies
            var movieService = new MovieService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested
            var result = await movieService.GetAllMoviesAsync();

            // Assert - Verify the results match our expectations
            Assert.Equal(2, result.Count);  // We should get both movies

            // Verify the movie titles are mapped correctly
            Assert.Equal("The Shawshank Redemption", result[0].Title);
            Assert.Equal("The Godfather", result[1].Title);
        }
    }
}