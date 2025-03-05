using CineScope.Client.Models;
using System.Net.Http.Json;

namespace CineScope.Client.Services
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
            return await _httpClient.GetFromJsonAsync<List<MovieModel>>("api/test/movies") ?? new List<MovieModel>();
        }

        public async Task<MovieModel?> GetMovieAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<MovieModel>($"api/test/movies/{id}");
        }

        public async Task<List<MovieModel>> GetTopRatedMoviesAsync(int limit = 5)
        {
            return await _httpClient.GetFromJsonAsync<List<MovieModel>>($"api/test/movies/top-rated?limit={limit}") ?? new List<MovieModel>();
        }
    }
}