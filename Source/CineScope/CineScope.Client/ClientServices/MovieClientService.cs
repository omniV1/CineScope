using CineScope.Shared.Models;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Client.ClientServices
{
    /// <summary>
    /// Client service for handling movie-related API requests
    /// Used in Blazor WebAssembly to communicate with the server API
    /// </summary>
    public class MovieClientService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Constructor for MovieClientService
        /// </summary>
        /// <param name="httpClient">HttpClient instance configured with base address</param>
        public MovieClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Retrieves all movies from the API
        /// </summary>
        /// <returns>List of all movies, or empty list if error occurs</returns>
        public async Task<List<MovieModel>> GetAllMoviesAsync()
        {
            try
            {
                // Fetch all movies from the API endpoint
                var movies = await _httpClient.GetFromJsonAsync<List<MovieModel>>("api/movies");

                // Return the movies or an empty list if null
                return movies ?? new List<MovieModel>();
            }
            catch (Exception ex)
            {
                // Log error and return empty list to avoid null reference exceptions
                Console.WriteLine($"Error in GetAllMoviesAsync: {ex.Message}");
                return new List<MovieModel>();
            }
        }

        /// <summary>
        /// Retrieves a specific movie by its ID
        /// </summary>
        /// <param name="id">The ID of the movie to retrieve</param>
        /// <returns>The movie if found, null otherwise</returns>
        public async Task<MovieModel?> GetMovieAsync(string id)
        {
            try
            {
                // Fetch specific movie by ID from the API
                return await _httpClient.GetFromJsonAsync<MovieModel>($"api/movies/{id}");
            }
            catch (Exception ex)
            {
                // Log error and return null to indicate failure
                Console.WriteLine($"Error in GetMovieAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retrieves the highest rated movies
        /// </summary>
        /// <param name="limit">Maximum number of movies to return (default: 10)</param>
        /// <returns>List of top rated movies, or empty list if error occurs</returns>
        public async Task<List<MovieModel>> GetTopRatedMoviesAsync(int limit = 10)
        {
            try
            {
                // Fetch top rated movies with specified limit
                return await _httpClient.GetFromJsonAsync<List<MovieModel>>($"api/movies/top-rated?limit={limit}");
            }
            catch (Exception ex)
            {
                // Log error and return empty list to avoid null reference exceptions
                Console.WriteLine($"Error getting top rated movies: {ex.Message}");
                return new List<MovieModel>();
            }
        }

        /// <summary>
        /// Retrieves the most recently added movies
        /// </summary>
        /// <param name="limit">Maximum number of movies to return (default: 10)</param>
        /// <returns>List of recent movies, or empty list if error occurs</returns>
        public async Task<List<MovieModel>> GetRecentMoviesAsync(int limit = 10)
        {
            try
            {
                // Fetch recent movies with specified limit
                return await _httpClient.GetFromJsonAsync<List<MovieModel>>($"api/movies/recent?limit={limit}");
            }
            catch (Exception ex)
            {
                // Log error and return empty list to avoid null reference exceptions
                Console.WriteLine($"Error getting recent movies: {ex.Message}");
                return new List<MovieModel>();
            }
        }
    }
}
