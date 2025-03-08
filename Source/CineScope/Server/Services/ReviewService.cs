using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq;

namespace CineScope.Server.Services
{
    public class ReviewService
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly MongoDbSettings _settings;

        public ReviewService(IMongoDbService mongoDbService, IOptions<MongoDbSettings> settings)
        {
            _mongoDbService = mongoDbService;
            _settings = settings.Value;
        }

        public async Task<List<Review>> GetReviewsByMovieIdAsync(string movieId)
        {
            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);
            return await collection.Find(r => r.MovieId == movieId).ToListAsync();
        }

        public async Task<List<Review>> GetReviewsByUserIdAsync(string userId)
        {
            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);
            return await collection.Find(r => r.UserId == userId).ToListAsync();
        }

        public async Task<Review> GetReviewByIdAsync(string id)
        {
            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);
            return await collection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);
            await collection.InsertOneAsync(review);
            return review;
        }

        public async Task<bool> UpdateReviewAsync(string id, Review review)
        {
            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);
            var result = await collection.ReplaceOneAsync(r => r.Id == id, review);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteReviewAsync(string id)
        {
            var collection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);
            var result = await collection.DeleteOneAsync(r => r.Id == id);
            return result.DeletedCount > 0;
        }
    }
}