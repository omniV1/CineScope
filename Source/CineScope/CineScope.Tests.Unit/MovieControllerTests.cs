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
    public class MovieControllerTests
    {
        [Fact]
        public async Task GetAllMovies_ShouldReturnAllMovies()
        {
            // Arrange
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

            var mockMovieService = new Mock<IMovieService>();
            mockMovieService
                .Setup(s => s.GetAllMoviesAsync())
                .ReturnsAsync(movies);

            var controller = new MovieController(mockMovieService.Object);

            // Act
            var result = await controller.GetAllMovies();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedMovies = Assert.IsType<List<MovieDto>>(okResult.Value);
            Assert.Equal(2, returnedMovies.Count);
            Assert.Equal("The Shawshank Redemption", returnedMovies[0].Title);
            Assert.Equal("The Godfather", returnedMovies[1].Title);
        }
    }
}