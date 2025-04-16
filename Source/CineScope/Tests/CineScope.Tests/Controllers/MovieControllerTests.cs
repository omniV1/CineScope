using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Server.Controllers;
using CineScope.Server.Interfaces;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CineScope.Tests.Controllers
{
    public class MovieControllerTests
    {
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly MovieController _controller;

        public MovieControllerTests()
        {
            _mockMovieService = new Mock<IMovieService>();
            _controller = new MovieController(_mockMovieService.Object);
        }

        [Fact]
        public async Task GetAllMovies_WhenMoviesExist_ReturnsOkResultWithMovies()
        {
            // Arrange
            var mockedMovies = new List<MovieDto>
            {
                new MovieDto { Id = "1", Title = "Test Movie 1", ReleaseDate = DateTime.Now, AverageRating = 4.5 },
                new MovieDto { Id = "2", Title = "Test Movie 2", ReleaseDate = DateTime.Now, AverageRating = 3.8 }
            };
            
            _mockMovieService.Setup(s => s.GetAllMoviesAsync())
                .ReturnsAsync(mockedMovies);

            // Act
            var result = await _controller.GetAllMovies();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<MovieDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal("Test Movie 1", returnValue[0].Title);
            Assert.Equal("Test Movie 2", returnValue[1].Title);
        }

        [Fact]
        public async Task GetAllMovies_WhenNoMoviesExist_ReturnsNoContent()
        {
            // Arrange
            _mockMovieService.Setup(s => s.GetAllMoviesAsync())
                .ReturnsAsync(new List<MovieDto>());

            // Act
            var result = await _controller.GetAllMovies();

            // Assert
            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public async Task GetMovieById_WhenMovieExists_ReturnsOkResultWithMovie()
        {
            // Arrange
            var movieId = "1";
            var mockedMovie = new MovieDto 
            { 
                Id = movieId, 
                Title = "Test Movie", 
                ReleaseDate = DateTime.Now,
                AverageRating = 4.5 
            };

            _mockMovieService.Setup(s => s.GetMovieByIdAsync(movieId))
                .ReturnsAsync(mockedMovie);

            // Act
            var result = await _controller.GetMovieById(movieId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<MovieDto>(okResult.Value);
            Assert.Equal(movieId, returnValue.Id);
            Assert.Equal(mockedMovie.Title, returnValue.Title);
        }

        [Fact]
        public async Task GetMovieById_WhenMovieDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var movieId = "nonexistent";
            _mockMovieService.Setup(s => s.GetMovieByIdAsync(movieId))
                .ReturnsAsync((MovieDto)null);

            // Act
            var result = await _controller.GetMovieById(movieId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateMovie_WhenMovieExists_ReturnsOkResult()
        {
            // Arrange
            var movieId = "1";
            var updateMovie = new MovieDto 
            { 
                Id = movieId, 
                Title = "Updated Movie", 
                ReleaseDate = DateTime.Now,
                AverageRating = 4.8 
            };

            _mockMovieService.Setup(s => s.UpdateMovieAsync(movieId, updateMovie))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateMovie(movieId, updateMovie);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockMovieService.Verify(s => s.UpdateMovieAsync(movieId, updateMovie), Times.Once);
        }

        [Fact]
        public async Task UpdateMovie_WhenIdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var movieId = "1";
            var updateMovie = new MovieDto 
            { 
                Id = "2", // Different ID
                Title = "Updated Movie"
            };

            // Act
            var result = await _controller.UpdateMovie(movieId, updateMovie);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _mockMovieService.Verify(s => s.UpdateMovieAsync(It.IsAny<string>(), It.IsAny<MovieDto>()), Times.Never);
        }

        [Fact]
        public async Task GetMoviesByGenre_WhenMoviesExist_ReturnsOkResultWithMovies()
        {
            // Arrange
            var genre = "Action";
            var mockedMovies = new List<MovieDto>
            {
                new MovieDto { Id = "1", Title = "Action Movie 1", Genres = new List<string> { genre } },
                new MovieDto { Id = "2", Title = "Action Movie 2", Genres = new List<string> { genre } }
            };

            _mockMovieService.Setup(s => s.GetMoviesByGenreAsync(genre))
                .ReturnsAsync(mockedMovies);

            // Act
            var result = await _controller.GetMoviesByGenre(genre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<MovieDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.All(returnValue, movie => Assert.Contains(genre, movie.Genres));
        }

        [Fact]
        public async Task GetMoviesByGenre_WhenNoMoviesExist_ReturnsNoContent()
        {
            // Arrange
            var genre = "NonexistentGenre";
            _mockMovieService.Setup(s => s.GetMoviesByGenreAsync(genre))
                .ReturnsAsync(new List<MovieDto>());

            // Act
            var result = await _controller.GetMoviesByGenre(genre);

            // Assert
            Assert.IsType<NoContentResult>(result.Result);
        }
    }
} 