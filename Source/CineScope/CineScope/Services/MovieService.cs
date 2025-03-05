using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;

        public MovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public async Task<List<MovieModel>> GetAllMoviesAsync()
        {
            return await _movieRepository.GetAllAsync();
        }

        public async Task<MovieModel> GetMovieByIdAsync(string id)
        {
            return await _movieRepository.GetByIdAsync(new ObjectId(id));
        }

        public async Task<List<MovieModel>> GetMoviesByGenreAsync(string genre)
        {
            return await _movieRepository.GetByGenreAsync(genre);
        }

        public async Task<List<MovieModel>> GetTopRatedMoviesAsync(int limit = 10)
        {
            return await _movieRepository.GetTopRatedAsync(limit);
        }

        public async Task<List<MovieModel>> GetRecentMoviesAsync(int limit = 10)
        {
            return await _movieRepository.GetRecentAsync(limit);
        }

        public async Task<MovieModel> CreateMovieAsync(MovieModel movie)
        {
            // Initialize or update any relevant fields
            if (movie.Id == ObjectId.Empty)
            {
                movie.Id = ObjectId.GenerateNewId();
            }

            // Set default values for any missing properties
            if (movie.ReleaseDate == DateTime.MinValue)
            {
                movie.ReleaseDate = DateTime.UtcNow;
            }

            if (movie.Genres == null)
            {
                movie.Genres = new List<string>();
            }

            if (movie.Actors == null)
            {
                movie.Actors = new List<string>();
            }

            return await _movieRepository.CreateAsync(movie);
        }

        public async Task UpdateMovieAsync(string id, MovieModel movie)
        {
            await _movieRepository.UpdateAsync(new ObjectId(id), movie);
        }

        public async Task DeleteMovieAsync(string id)
        {
            await _movieRepository.DeleteAsync(new ObjectId(id));
        }
    }
}