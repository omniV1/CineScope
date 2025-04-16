using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CineScope.Server;
using CineScope.Server.Models;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace CineScope.Tests.Integration
{
    public class MovieIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly IMongoDatabase _database;
        private readonly string _databaseName = "CineScopeIntegrationTest";

        public MovieIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configure test database
                    var mongoClient = new MongoClient("mongodb://localhost:27017");
                    _database = mongoClient.GetDatabase(_databaseName);
                    
                    services.AddSingleton<IMongoDatabase>(_database);
                });
            });

            _client = _factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            // Setup test data
            var collection = _database.GetCollection<Movie>("Movies");
            await collection.InsertManyAsync(new[]
            {
                new Movie
                {
                    Id = "1",
                    Title = "Test Movie 1",
                    ReleaseDate = DateTime.Now,
                    AverageRating = 4.5,
                    Genres = new List<string> { "Action", "Drama" }
                },
                new Movie
                {
                    Id = "2",
                    Title = "Test Movie 2",
                    ReleaseDate = DateTime.Now,
                    AverageRating = 3.8,
                    Genres = new List<string> { "Comedy" }
                }
            });
        }

        public async Task DisposeAsync()
        {
            // Cleanup test data
            await _database.DropCollectionAsync("Movies");
        }

        [Fact]
        public async Task GetAllMovies_ReturnsAllMovies()
        {
            // Act
            var response = await _client.GetAsync("/api/Movie");
            response.EnsureSuccessStatusCode();
            var movies = await response.Content.ReadFromJsonAsync<List<MovieDto>>();

            // Assert
            Assert.NotNull(movies);
            Assert.Equal(2, movies.Count);
            Assert.Contains(movies, m => m.Title == "Test Movie 1");
            Assert.Contains(movies, m => m.Title == "Test Movie 2");
        }

        [Fact]
        public async Task GetMovieById_WithValidId_ReturnsMovie()
        {
            // Act
            var response = await _client.GetAsync("/api/Movie/1");
            response.EnsureSuccessStatusCode();
            var movie = await response.Content.ReadFromJsonAsync<MovieDto>();

            // Assert
            Assert.NotNull(movie);
            Assert.Equal("Test Movie 1", movie.Title);
            Assert.Equal(4.5, movie.AverageRating);
        }

        [Fact]
        public async Task GetMovieById_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/Movie/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetMoviesByGenre_ReturnsMatchingMovies()
        {
            // Act
            var response = await _client.GetAsync("/api/Movie/genre/Action");
            response.EnsureSuccessStatusCode();
            var movies = await response.Content.ReadFromJsonAsync<List<MovieDto>>();

            // Assert
            Assert.NotNull(movies);
            Assert.Single(movies);
            Assert.Equal("Test Movie 1", movies[0].Title);
            Assert.Contains("Action", movies[0].Genres);
        }

        [Fact]
        public async Task UpdateMovie_WithValidData_UpdatesMovie()
        {
            // Arrange
            var updateMovie = new MovieDto
            {
                Id = "1",
                Title = "Updated Test Movie 1",
                ReleaseDate = DateTime.Now,
                AverageRating = 4.8,
                Genres = new List<string> { "Action", "Drama", "Thriller" }
            };

            // Act
            var updateResponse = await _client.PutAsJsonAsync("/api/Movie/1", updateMovie);
            updateResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync("/api/Movie/1");
            getResponse.EnsureSuccessStatusCode();
            var updatedMovie = await getResponse.Content.ReadFromJsonAsync<MovieDto>();

            // Assert
            Assert.NotNull(updatedMovie);
            Assert.Equal(updateMovie.Title, updatedMovie.Title);
            Assert.Equal(updateMovie.AverageRating, updatedMovie.AverageRating);
            Assert.Equal(updateMovie.Genres.Count, updatedMovie.Genres.Count);
        }

        [Fact]
        public async Task UpdateMovie_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var updateMovie = new MovieDto
            {
                Id = "999",
                Title = "Invalid Update"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/Movie/1", updateMovie);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
} 