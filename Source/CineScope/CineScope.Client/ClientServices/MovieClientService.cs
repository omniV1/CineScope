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
                // This is correct
                var movies = await _httpClient.GetFromJsonAsync<List<MovieModel>>("api/movies");
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
                // Changed from api/test/movies/{id} to api/movies/{id}
                return await _httpClient.GetFromJsonAsync<MovieModel>($"api/movies/{id}");
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
                // Changed from api/test/movies/top-rated to api/movies/top-rated
                return await _httpClient.GetFromJsonAsync<List<MovieModel>>($"api/movies/top-rated?limit={limit}") ?? new List<MovieModel>();
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
                // Changed from api/test/movies/{movieId}/reviews to api/reviews/movie/{movieId}
                return await _httpClient.GetFromJsonAsync<List<ReviewModel>>($"api/reviews/movie/{movieId}") ?? new List<ReviewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMovieReviewsAsync: {ex.Message}");
                return new List<ReviewModel>();
            }
        }

        public async Task<List<MovieModel>> GetRecentMoviesAsync(int limit = 5)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<MovieModel>>($"api/movies/recent?limit={limit}") ?? new List<MovieModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRecentMoviesAsync: {ex.Message}");
                return new List<MovieModel>();
            }
        }
    }
}