using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Shared.DTOs;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq;

namespace CineScope.Server.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly MongoDbSettings _settings;

        public MovieService(IMongoDbService mongoDbService, IOptions<MongoDbSettings> settings)
        {
            _mongoDbService = mongoDbService;
            _settings = settings.Value;
        }

        public async Task<List<MovieDto>> GetAllMoviesAsync()
        {
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
            var movies = await collection.Find(_ => true).ToListAsync();
            return movies.Select(MapToDto).ToList();
        }

        public async Task<MovieDto> GetMovieByIdAsync(string id)
        {
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
            var movie = await collection.Find(m => m.Id == id).FirstOrDefaultAsync();
            return movie != null ? MapToDto(movie) : null;
        }

        public async Task<List<MovieDto>> GetMoviesByGenreAsync(string genre)
        {
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
            var movies = await collection.Find(m => m.Genres.Contains(genre)).ToListAsync();
            return movies.Select(MapToDto).ToList();
        }

        public async Task<MovieDto> CreateMovieAsync(MovieDto movieDto)
        {
            var movie = MapToModel(movieDto);
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
            await collection.InsertOneAsync(movie);
            return MapToDto(movie);
        }

        public async Task<bool> UpdateMovieAsync(string id, MovieDto movieDto)
        {
            var movie = MapToModel(movieDto);
            movie.Id = id;

            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
            var result = await collection.ReplaceOneAsync(m => m.Id == id, movie);

            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMovieAsync(string id)
        {
            var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
            var result = await collection.DeleteOneAsync(m => m.Id == id);

            return result.DeletedCount > 0;
        }

        private MovieDto MapToDto(Movie movie)
        {
            return new MovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                ReleaseDate = movie.ReleaseDate,
                Genres = movie.Genres,
                Director = movie.Director,
                Actors = movie.Actors,
                PosterUrl = movie.PosterUrl,
                AverageRating = movie.AverageRating,
                ReviewCount = movie.ReviewCount
            };
        }

        private Movie MapToModel(MovieDto dto)
        {
            return new Movie
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                ReleaseDate = dto.ReleaseDate,
                Genres = dto.Genres,
                Director = dto.Director,
                Actors = dto.Actors,
                PosterUrl = dto.PosterUrl,
                AverageRating = dto.AverageRating,
                ReviewCount = dto.ReviewCount
            };
        }
    }
}