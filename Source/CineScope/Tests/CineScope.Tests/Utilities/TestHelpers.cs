using System;
using System.Collections.Generic;
using System.IO;
using CineScope.Server.Models;
using CineScope.Shared.DTOs;
using Microsoft.Extensions.Configuration;

namespace CineScope.Tests.Utilities
{
    public static class TestHelpers
    {
        public static IConfiguration GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static T GetOptions<T>(string sectionName) where T : new()
        {
            var options = new T();
            var configuration = GetIConfigurationRoot();
            var section = configuration.GetSection(sectionName);
            section.Bind(options);
            return options;
        }

        public static Movie CreateTestMovie(string id = null)
        {
            return new Movie
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Title = $"Test Movie {id ?? "New"}",
                ReleaseDate = DateTime.Now,
                AverageRating = 4.0,
                Genres = new List<string> { "Action", "Drama" },
                Director = "Test Director",
                Actors = new List<string> { "Actor 1", "Actor 2" },
                Description = "Test movie description",
                PosterUrl = "https://example.com/poster.jpg",
                ReviewCount = 0
            };
        }

        public static MovieDto CreateTestMovieDto(string id = null)
        {
            return new MovieDto
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Title = $"Test Movie {id ?? "New"}",
                ReleaseDate = DateTime.Now,
                AverageRating = 4.0,
                Genres = new List<string> { "Action", "Drama" },
                Director = "Test Director",
                Actors = new List<string> { "Actor 1", "Actor 2" },
                Description = "Test movie description",
                PosterUrl = "https://example.com/poster.jpg"
            };
        }

        public static List<Movie> CreateTestMovies(int count)
        {
            var movies = new List<Movie>();
            for (int i = 1; i <= count; i++)
            {
                movies.Add(CreateTestMovie(i.ToString()));
            }
            return movies;
        }

        public static List<MovieDto> CreateTestMovieDtos(int count)
        {
            var movies = new List<MovieDto>();
            for (int i = 1; i <= count; i++)
            {
                movies.Add(CreateTestMovieDto(i.ToString()));
            }
            return movies;
        }

        public static Review CreateTestReview(string movieId = null, string userId = null)
        {
            return new Review
            {
                Id = Guid.NewGuid().ToString(),
                MovieId = movieId ?? Guid.NewGuid().ToString(),
                UserId = userId ?? Guid.NewGuid().ToString(),
                Rating = 4,
                Text = "Test review text",
                CreatedAt = DateTime.UtcNow
            };
        }

        public static List<Review> CreateTestReviews(string movieId, int count)
        {
            var reviews = new List<Review>();
            for (int i = 0; i < count; i++)
            {
                reviews.Add(CreateTestReview(movieId));
            }
            return reviews;
        }
    }
} 