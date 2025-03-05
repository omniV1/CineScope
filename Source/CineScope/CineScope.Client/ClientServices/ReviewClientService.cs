using System.Net.Http.Json;
using CineScope.Shared.Models;
using MongoDB.Bson;

namespace CineScope.Client.ClientServices
{
    public class ReviewClientService
    {
        private readonly HttpClient _httpClient;

        public ReviewClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ReviewModel>> GetReviewsByMovieAsync(string movieId)
        {
            return await _httpClient.GetFromJsonAsync<List<ReviewModel>>($"api/review/movie/{movieId}") ?? new List<ReviewModel>();
        }

        public async Task<List<ReviewModel>> GetReviewsByUserAsync(string userId)
        {
            return await _httpClient.GetFromJsonAsync<List<ReviewModel>>($"api/review/user/{userId}") ?? new List<ReviewModel>();
        }

        public async Task<ReviewModel?> GetReviewAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<ReviewModel>($"api/review/{id}");
        }

        public async Task<ReviewModel?> CreateReviewAsync(ReviewModel review)
        {
            var response = await _httpClient.PostAsJsonAsync("api/review", review);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ReviewModel>();
            }

            return null;
        }

        public async Task<bool> UpdateReviewAsync(string id, ReviewModel review)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/review/{id}", review);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteReviewAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"api/review/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ReviewModel>> GetFilteredReviewsAsync(string movieId, string sortBy = "date_desc", int? rating = null)
        {
            string url = $"api/review/filtered?movieId={movieId}&sortBy={sortBy}";

            if (rating.HasValue)
            {
                url += $"&rating={rating.Value}";
            }

            return await _httpClient.GetFromJsonAsync<List<ReviewModel>>(url) ?? new List<ReviewModel>();
        }
    }
}