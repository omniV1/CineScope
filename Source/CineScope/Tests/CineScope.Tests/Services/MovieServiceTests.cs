using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Server.Models;
using CineScope.Server.Services;
using CineScope.Shared.DTOs;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;
using CineScope.Server.Interfaces;
using CineScope.Server.Data;

namespace CineScope.Tests.Services
{
    public class MovieServiceTests
    {
        private readonly Mock<IMongoDbService> _mockMongoDbService;
        private readonly Mock<IMovieCacheService> _mockMovieCacheService;
        private readonly IMovieService _movieService;
        private readonly MongoDbSettings _settings;

        public MovieServiceTests()
        {
            _mockMongoDbService = new Mock<IMongoDbService>();
            _mockMovieCacheService = new Mock<IMovieCacheService>();
            _settings = new MongoDbSettings
            {
                MoviesCollectionName = "Movies",
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "CineScopeTest"
            };
            
            var mockOptions = new Mock<IOptions<MongoDbSettings>>();
            mockOptions.Setup(o => o.Value).Returns(_settings);
            
            _movieService = new MovieService(
                _mockMongoDbService.Object,
                mockOptions.Object,
                _mockMovieCacheService.Object);
        }

        [Fact]
        public async Task GetAllMoviesAsync_WhenCacheAvailable_ReturnsCachedMovies()
        {
            // Arrange
            var mockedMovies = new List<MovieDto>
            {
                new MovieDto { Id = "1", Title = "Test Movie 1", ReleaseDate = DateTime.Now, AverageRating = 4.5 },
                new MovieDto { Id = "2", Title = "Test Movie 2", ReleaseDate = DateTime.Now, AverageRating = 3.8 }
            };
            
            _mockMovieCacheService.Setup(c => c.GetAllMovies()).Returns(mockedMovies);

            // Act
            var result = await _movieService.GetAllMoviesAsync();

            // Assert
            Assert.Equal(mockedMovies.Count, result.Count);
            Assert.Equal(mockedMovies[0].Title, result[0].Title);
            Assert.Equal(mockedMovies[1].Title, result[1].Title);
            _mockMovieCacheService.Verify(c => c.GetAllMovies(), Times.Once);
            _mockMongoDbService.Verify(m => m.GetCollection<Movie>(_settings.MoviesCollectionName), Times.Never);
        }

        [Fact]
        public async Task GetAllMoviesAsync_WhenCacheEmpty_ReturnsFromDatabase()
        {
            // Arrange
            var mockedMovies = new List<Movie>
            {
                new Movie { Id = "1", Title = "Test Movie 1", ReleaseDate = DateTime.Now, AverageRating = 4.5 },
                new Movie { Id = "2", Title = "Test Movie 2", ReleaseDate = DateTime.Now, AverageRating = 3.8 }
            };

            var mockCollection = new Mock<IMongoCollection<Movie>>();
            var mockCursor = new Mock<IAsyncCursor<Movie>>();
            mockCursor.Setup(c => c.Current).Returns(mockedMovies);
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Movie>>(),
                    It.IsAny<FindOptions<Movie, Movie>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mockMongoDbService
                .Setup(m => m.GetCollection<Movie>(_settings.MoviesCollectionName))
                .Returns(mockCollection.Object);

            _mockMovieCacheService.Setup(c => c.GetAllMovies()).Returns(new List<MovieDto>());

            // Act
            var result = await _movieService.GetAllMoviesAsync();

            // Assert
            Assert.Equal(mockedMovies.Count, result.Count);
            Assert.Equal(mockedMovies[0].Title, result[0].Title);
            Assert.Equal(mockedMovies[1].Title, result[1].Title);
            _mockMovieCacheService.Verify(c => c.GetAllMovies(), Times.Once);
            _mockMongoDbService.Verify(m => m.GetCollection<Movie>(_settings.MoviesCollectionName), Times.Once);
        }

        [Fact]
        public async Task GetMovieByIdAsync_WhenMovieExists_ReturnsMovie()
        {
            // Arrange
            var movieId = "1";
            var mockedMovie = new Movie 
            { 
                Id = movieId, 
                Title = "Test Movie", 
                ReleaseDate = DateTime.Now,
                AverageRating = 4.5 
            };

            var mockCollection = new Mock<IMongoCollection<Movie>>();
            var mockCursor = new Mock<IAsyncCursor<Movie>>();
            mockCursor.Setup(c => c.Current).Returns(new List<Movie> { mockedMovie });
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Movie>>(),
                    It.IsAny<FindOptions<Movie, Movie>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mockMongoDbService
                .Setup(m => m.GetCollection<Movie>(_settings.MoviesCollectionName))
                .Returns(mockCollection.Object);

            // Act
            var result = await _movieService.GetMovieByIdAsync(movieId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(movieId, result.Id);
            Assert.Equal(mockedMovie.Title, result.Title);
        }

        [Fact]
        public async Task GetMovieByIdAsync_WhenMovieDoesNotExist_ReturnsNull()
        {
            // Arrange
            var movieId = "nonexistent";
            var mockCollection = new Mock<IMongoCollection<Movie>>();
            var mockCursor = new Mock<IAsyncCursor<Movie>>();
            mockCursor.Setup(c => c.Current).Returns(new List<Movie>());
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Movie>>(),
                    It.IsAny<FindOptions<Movie, Movie>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mockMongoDbService
                .Setup(m => m.GetCollection<Movie>(_settings.MoviesCollectionName))
                .Returns(mockCollection.Object);

            // Act
            var result = await _movieService.GetMovieByIdAsync(movieId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateMovieAsync_WhenMovieExists_UpdatesSuccessfully()
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

            var mockCollection = new Mock<IMongoCollection<Movie>>();
            mockCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Movie>>(),
                    It.IsAny<Movie>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, movieId));

            _mockMongoDbService
                .Setup(m => m.GetCollection<Movie>(_settings.MoviesCollectionName))
                .Returns(mockCollection.Object);

            // Act
            await _movieService.UpdateMovieAsync(movieId, updateMovie);

            // Assert
            mockCollection.Verify(
                c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Movie>>(),
                    It.Is<Movie>(m => m.Title == updateMovie.Title),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateMovieAsync_CreatesSuccessfully()
        {
            // Arrange
            var newMovie = new MovieDto 
            { 
                Id = "1", 
                Title = "New Movie", 
                ReleaseDate = DateTime.Now,
                AverageRating = 4.0,
                Genres = new List<string> { "Action", "Drama" }
            };

            var mockCollection = new Mock<IMongoCollection<Movie>>();
            mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Movie>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockMongoDbService
                .Setup(m => m.GetCollection<Movie>(_settings.MoviesCollectionName))
                .Returns(mockCollection.Object);

            // Act
            var result = await _movieService.CreateMovieAsync(newMovie);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newMovie.Title, result.Title);
            Assert.Equal(newMovie.Genres, result.Genres);
            mockCollection.Verify(
                c => c.InsertOneAsync(
                    It.Is<Movie>(m => m.Title == newMovie.Title),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            _mockMovieCacheService.Verify(c => c.ClearCache(), Times.Once);
        }

        [Fact]
        public async Task DeleteMovieAsync_WhenMovieExists_DeletesSuccessfully()
        {
            // Arrange
            var movieId = "1";
            var mockCollection = new Mock<IMongoCollection<Movie>>();
            mockCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Movie>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            _mockMongoDbService
                .Setup(m => m.GetCollection<Movie>(_settings.MoviesCollectionName))
                .Returns(mockCollection.Object);

            // Act
            var result = await _movieService.DeleteMovieAsync(movieId);

            // Assert
            Assert.True(result);
            mockCollection.Verify(
                c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Movie>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            _mockMovieCacheService.Verify(c => c.ClearCache(), Times.Once);
        }
    }
} 