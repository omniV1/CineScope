using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Shared.DTOs;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Server.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly MongoDbSettings _settings;
        private readonly IMovieCacheService _cacheService;

        public MovieService(
            IMongoDbService mongoDbService,
            IOptions<MongoDbSettings> mongoDbSettings,
            IMovieCacheService cacheService)
        {
            _mongoDbService = mongoDbService;
            _settings = mongoDbSettings.Value;
            _cacheService = cacheService;
        }

        public async Task<List<MovieDto>> GetAllMoviesAsync()
        {
            // Try to get from cache first
            var cachedMovies = _cacheService.GetAllMovies();
            if (cachedMovies != null && cachedMovies.Count > 0)
            {
                return cachedMovies;
            }

            // If not in cache, get from database
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
            var movies = await collection.Find(_ => true).ToListAsync();
            
            // Convert to DTOs with all properties
            var movieDtos = movies.ConvertAll(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseDate = m.ReleaseDate,
                Genres = m.Genres ?? new List<string>(),
                Director = m.Director,
                Actors = m.Actors ?? new List<string>(),
                PosterUrl = m.PosterUrl,
                AverageRating = m.AverageRating,
                ReviewCount = m.ReviewCount
            });

            // Cache the results
            _cacheService.SetAllMovies(movieDtos);

            return movieDtos;
        }

        public async Task<MovieDto?> GetMovieByIdAsync(string id)
        {
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
            var filter = Builders<Movie>.Filter.Eq(m => m.Id, id);
            var movie = await collection.Find(filter).FirstOrDefaultAsync();

            if (movie == null)
                return null;

            return new MovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                ReleaseDate = movie.ReleaseDate,
                Genres = movie.Genres ?? new List<string>(),
                Director = movie.Director,
                Actors = movie.Actors ?? new List<string>(),
                PosterUrl = movie.PosterUrl,
                AverageRating = movie.AverageRating,
                ReviewCount = movie.ReviewCount
            };
        }

        public async Task<List<MovieDto>> GetMoviesByGenreAsync(string genre)
        {
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
            var filter = Builders<Movie>.Filter.AnyEq(m => m.Genres, genre);
            var movies = await collection.Find(filter).ToListAsync();

            return movies.ConvertAll(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseDate = m.ReleaseDate,
                Genres = m.Genres ?? new List<string>(),
                Director = m.Director,
                Actors = m.Actors ?? new List<string>(),
                PosterUrl = m.PosterUrl,
                AverageRating = m.AverageRating,
                ReviewCount = m.ReviewCount
            });
        }

        public async Task<MovieDto> CreateMovieAsync(MovieDto movieDto)
        {
            var movie = new Movie
            {
                Id = movieDto.Id,
                Title = movieDto.Title,
                Description = movieDto.Description,
                ReleaseDate = movieDto.ReleaseDate,
                Genres = movieDto.Genres ?? new List<string>(),
                Director = movieDto.Director,
                Actors = movieDto.Actors ?? new List<string>(),
                PosterUrl = movieDto.PosterUrl,
                AverageRating = movieDto.AverageRating,
                ReviewCount = movieDto.ReviewCount
            };

            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
            await collection.InsertOneAsync(movie);

            // Clear cache since we modified the data
            _cacheService.ClearCache();

            return movieDto;
        }

        public async Task<bool> UpdateMovieAsync(string id, MovieDto movieDto)
        {
            var movie = new Movie
            {
                Id = id,
                Title = movieDto.Title,
                Description = movieDto.Description,
                ReleaseDate = movieDto.ReleaseDate,
                Genres = movieDto.Genres ?? new List<string>(),
                Director = movieDto.Director,
                Actors = movieDto.Actors ?? new List<string>(),
                PosterUrl = movieDto.PosterUrl,
                AverageRating = movieDto.AverageRating,
                ReviewCount = movieDto.ReviewCount
            };

            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
            var filter = Builders<Movie>.Filter.Eq(m => m.Id, id);
            var result = await collection.ReplaceOneAsync(filter, movie);

            // Clear cache since we modified the data
            _cacheService.ClearCache();

            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMovieAsync(string id)
        {
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
            var filter = Builders<Movie>.Filter.Eq(m => m.Id, id);
            var result = await collection.DeleteOneAsync(filter);

            // Clear cache since we modified the data
            _cacheService.ClearCache();

            return result.DeletedCount > 0;
        }
    }
} 