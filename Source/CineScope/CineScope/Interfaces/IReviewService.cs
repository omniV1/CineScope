using CineScope.Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Interfaces
{
    public interface IReviewService
    {
        Task<List<ReviewModel>> GetAllReviewsAsync();
        Task<ReviewModel> GetReviewByIdAsync(string id);
        Task<List<ReviewModel>> GetReviewsByUserIdAsync(string userId);
        Task<List<ReviewModel>> GetReviewsByMovieIdAsync(string movieId);
        Task<ReviewModel> CreateReviewAsync(ReviewModel review);
        Task UpdateReviewAsync(string id, ReviewModel review);
        Task DeleteReviewAsync(string id);
        Task<List<ReviewModel>> GetFilteredReviewsAsync(string movieId, string sortBy, int? rating);
    }
}