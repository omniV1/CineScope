using System.Net.Http.Json;
using CineScope.Shared.Models;
using MongoDB.Bson;

namespace CineScope.Client.ClientServices
{
    /// <summary>
    /// Client service for handling review-related API requests in Blazor WebAssembly
    /// Provides methods to interact with the review endpoints of the server API
    /// </summary>
    public class ReviewClientService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Constructor for ReviewClientService
        /// </summary>
        /// <param name="httpClient">Configured HttpClient instance for API communication</param>
        public ReviewClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Retrieves all reviews for a specific movie
        /// </summary>
        /// <param name="movieId">ID of the movie to get reviews for</param>
        /// <returns>List of reviews for the specified movie, or empty list if none found or error occurs</returns>
        public async Task<List<ReviewModel>> GetReviewsByMovieAsync(string movieId)
        {
            return await _httpClient.GetFromJsonAsync<List<ReviewModel>>($"api/review/movie/{movieId}") ?? new List<ReviewModel>();
        }

        /// <summary>
        /// Retrieves all reviews created by a specific user
        /// </summary>
        /// <param name="userId">ID of the user whose reviews to retrieve</param>
        /// <returns>List of reviews by the specified user, or empty list if none found or error occurs</returns>
        public async Task<List<ReviewModel>> GetReviewsByUserAsync(string userId)
        {
            return await _httpClient.GetFromJsonAsync<List<ReviewModel>>($"api/review/user/{userId}") ?? new List<ReviewModel>();
        }

        /// <summary>
        /// Retrieves a specific review by its ID
        /// </summary>
        /// <param name="id">ID of the review to retrieve</param>
        /// <returns>The review if found, null otherwise</returns>
        public async Task<ReviewModel?> GetReviewAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<ReviewModel>($"api/review/{id}");
        }

        /// <summary>
        /// Creates a new review in the system
        /// </summary>
        /// <param name="review">The review data to submit</param>
        /// <returns>The created review with server-generated data if successful, null otherwise</returns>
        public async Task<ReviewModel?> CreateReviewAsync(ReviewModel review)
        {
            // Send POST request with review data
            var response = await _httpClient.PostAsJsonAsync("api/review", review);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Parse and return the created review from response
                return await response.Content.ReadFromJsonAsync<ReviewModel>();
            }

            // Return null to indicate failure
            return null;
        }

        /// <summary>
        /// Updates an existing review
        /// </summary>
        /// <param name="id">ID of the review to update</param>
        /// <param name="review">The updated review data</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public async Task<bool> UpdateReviewAsync(string id, ReviewModel review)
        {
            // Send PUT request with updated review data
            var response = await _httpClient.PutAsJsonAsync($"api/review/{id}", review);

            // Return success status
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Deletes a review from the system
        /// </summary>
        /// <param name="id">ID of the review to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public async Task<bool> DeleteReviewAsync(string id)
        {
            // Send DELETE request
            var response = await _httpClient.DeleteAsync($"api/review/{id}");

            // Return success status
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retrieves reviews with filtering and sorting options
        /// </summary>
        /// <param name="movieId">ID of the movie to get reviews for</param>
        /// <param name="sortBy">Sorting option (date_desc, date_asc, rating_desc, rating_asc)</param>
        /// <param name="rating">Optional filter by specific rating value</param>
        /// <returns>Filtered and sorted list of reviews, or empty list if none found or error occurs</returns>
        public async Task<List<ReviewModel>> GetFilteredReviewsAsync(string movieId, string sortBy = "date_desc", int? rating = null)
        {
            // Build the query URL with required parameters
            string url = $"api/review/filtered?movieId={movieId}&sortBy={sortBy}";

            // Add optional rating filter if provided
            if (rating.HasValue)
            {
                url += $"&rating={rating.Value}";
            }

            // Execute request and return results (or empty list if null)
            return await _httpClient.GetFromJsonAsync<List<ReviewModel>>(url) ?? new List<ReviewModel>();
        }
    }
}

