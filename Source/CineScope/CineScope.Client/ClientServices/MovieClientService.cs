using CineScope.Shared.Models;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Client.ClientServices
{
    public class MovieClientService
    {
        private readonly HttpClient _httpClient;

        public MovieClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<MovieModel>> GetAllMoviesAsync()
        {
            try
            {
                Console.WriteLine("Calling GetAllMoviesAsync");
                var movies = await _httpClient.GetFromJsonAsync<List<MovieModel>>("api/test/movies");
                Console.WriteLine($"Retrieved {movies?.Count ?? 0} movies");
                return movies ?? new List<MovieModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllMoviesAsync: {ex.Message}");
                return new List<MovieModel>();
            }
        }

        public async Task<MovieModel?> GetMovieAsync(string id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<MovieModel>($"api/test/movies/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMovieAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<List<MovieModel>> GetTopRatedMoviesAsync(int limit = 5)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<MovieModel>>($"api/test/movies/top-rated?limit={limit}") ?? new List<MovieModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTopRatedMoviesAsync: {ex.Message}");
                return new List<MovieModel>();
            }
        }

        public async Task<List<ReviewModel>> GetMovieReviewsAsync(string movieId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<ReviewModel>>($"api/test/movies/{movieId}/reviews") ?? new List<ReviewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMovieReviewsAsync: {ex.Message}");
                return new List<ReviewModel>();
            }
        }
    }
}