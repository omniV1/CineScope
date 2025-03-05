using CineScope.Client.Models;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Interfaces
{
    public interface IReviewRepository
    {
        Task<List<ReviewModel>> GetAllAsync();
        Task<ReviewModel> GetByIdAsync(ObjectId id);
        Task<List<ReviewModel>> GetByUserIdAsync(ObjectId userId);
        Task<List<ReviewModel>> GetByMovieIdAsync(ObjectId movieId);
        Task<ReviewModel> CreateAsync(ReviewModel review);
        Task UpdateAsync(ObjectId id, ReviewModel review);
        Task DeleteAsync(ObjectId id);
    }
}