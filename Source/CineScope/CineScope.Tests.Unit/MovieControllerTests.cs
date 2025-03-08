using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Server.Controllers;
using CineScope.Server.Interfaces;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CineScope.Tests.Unit
{
    /// <summary>
    /// Contains unit tests for the MovieController API endpoint.
    /// These tests verify that the controller correctly processes requests
    /// and returns appropriate responses.
    /// </summary>
    public class MovieControllerTests
    {
        /// <summary>
        /// Tests that the GetAllMovies endpoint returns all movies from the service.
        /// 
        /// This test verifies that:
        /// - The controller correctly calls the movie service
        /// - The controller returns an OK result with the movies
        /// - All movies from the service are included in the response
        /// </summary>
        [Fact]
        public async Task GetAllMovies_ShouldReturnAllMovies()
        {
            // Arrange - Set up test data and dependencies
            // Create a list of sample movies to be returned by the service
            var movies = new List<MovieDto>
            {
                new MovieDto
                {
                    Id = "1",
                    Title = "The Shawshank Redemption",
                    ReleaseDate = new DateTime(1994, 9, 23),
                    AverageRating = 5.0,
                    PosterUrl = "/images/placeholder.png"
                },
                new MovieDto
                {
                    Id = "2",
                    Title = "The Godfather",
                    ReleaseDate = new DateTime(1972, 3, 24),
                    AverageRating = 5.0,
                    PosterUrl = "/images/placeholder.png"
                }
            };

            // Mock the movie service to return our sample movies
            var mockMovieService = new Mock<IMovieService>();
            mockMovieService
                .Setup(s => s.GetAllMoviesAsync())
                .ReturnsAsync(movies);

            // Create the controller with our mocked service
            var controller = new MovieController(mockMovieService.Object);

            // Act - Call the controller method being tested
            var result = await controller.GetAllMovies();

            // Assert - Verify the results match our expectations
            // Check that we get an OK result
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            // Check that the value is a list of movies
            var returnedMovies = Assert.IsType<List<MovieDto>>(okResult.Value);

            // Verify we got both movies
            Assert.Equal(2, returnedMovies.Count);

            // Verify the movie titles are correct
            Assert.Equal("The Shawshank Redemption", returnedMovies[0].Title);
            Assert.Equal("The Godfather", returnedMovies[1].Title);
        }
    }
}