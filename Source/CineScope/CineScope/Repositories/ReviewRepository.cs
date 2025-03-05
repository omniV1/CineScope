using CineScope.Shared.Helpers;
using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IMongoCollection<ReviewModel> _reviews;

        public ReviewRepository(MongoDBSettings settings)
        {
            var client = MongoDbConnectionHelper.CreateClient(settings);
            var database = client.GetDatabase(settings.DatabaseName);
            _reviews = database.GetCollection<ReviewModel>(settings.ReviewsCollectionName);
        }

        public async Task<List<ReviewModel>> GetAllAsync()
        {
            return await _reviews.Find(review => true).ToListAsync();
        }

        public async Task<ReviewModel> GetByIdAsync(ObjectId id)
        {
            return await _reviews.Find(review => review.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<ReviewModel>> GetByUserIdAsync(ObjectId userId)
        {
            return await _reviews.Find(review => review.UserId == userId).ToListAsync();
        }

        public async Task<List<ReviewModel>> GetByMovieIdAsync(ObjectId movieId)
        {
            return await _reviews.Find(review => review.MovieId == movieId)
                .Sort(Builders<ReviewModel>.Sort.Descending(r => r.CreatedAt))
                .ToListAsync();
        }

        public async Task<ReviewModel> CreateAsync(ReviewModel review)
        {
            await _reviews.InsertOneAsync(review);
            return review;
        }

        public async Task UpdateAsync(ObjectId id, ReviewModel updatedReview)
        {
            await _reviews.ReplaceOneAsync(review => review.Id == id, updatedReview);
        }

        public async Task DeleteAsync(ObjectId id)
        {
            await _reviews.DeleteOneAsync(review => review.Id == id);
        }
    }
}