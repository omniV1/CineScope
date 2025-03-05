using System.Net.Http.Json;
using CineScope.Shared.Models;

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
                // Use relative URL to avoid confusion
                string apiUrl = "api/test/movies";
                Console.WriteLine($"Calling API endpoint: {apiUrl}");

                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                Console.WriteLine($"Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<MovieModel>>() ?? new List<MovieModel>();
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response: {content.Substring(0, Math.Min(500, content.Length))}");
                    return new List<MovieModel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllMoviesAsync: {ex}");
                return new List<MovieModel>();
            }
        }

        public async Task<MovieModel?> GetMovieAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<MovieModel>($"api/test/movies/{id}");
        }

        public async Task<List<MovieModel>> GetTopRatedMoviesAsync(int limit = 5)
        {
            return await _httpClient.GetFromJsonAsync<List<MovieModel>>($"api/test/movies/top-rated?limit={limit}") ?? new List<MovieModel>();
        }

        public async Task<List<ReviewModel>> GetMovieReviewsAsync(string movieId)
        {
            return await _httpClient.GetFromJsonAsync<List<ReviewModel>>($"api/test/movies/{movieId}/reviews") ?? new List<ReviewModel>();
        }
    }
}