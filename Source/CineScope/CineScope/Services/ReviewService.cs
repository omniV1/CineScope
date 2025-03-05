using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CineScope.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IContentFilterService _contentFilterService;

        public ReviewService(
            IReviewRepository reviewRepository,
            IMovieRepository movieRepository,
            IContentFilterService contentFilterService)
        {
            _reviewRepository = reviewRepository;
            _movieRepository = movieRepository;
            _contentFilterService = contentFilterService;
        }

        public async Task<List<ReviewModel>> GetAllReviewsAsync()
        {
            return await _reviewRepository.GetAllAsync();
        }

        public async Task<ReviewModel> GetReviewByIdAsync(string id)
        {
            return await _reviewRepository.GetByIdAsync(new ObjectId(id));
        }

        public async Task<List<ReviewModel>> GetReviewsByUserIdAsync(string userId)
        {
            return await _reviewRepository.GetByUserIdAsync(new ObjectId(userId));
        }

        public async Task<List<ReviewModel>> GetReviewsByMovieIdAsync(string movieId)
        {
            return await _reviewRepository.GetByMovieIdAsync(new ObjectId(movieId));
        }

        public async Task<ReviewModel> CreateReviewAsync(ReviewModel review)
        {
            // Check content against filter
            bool isApproved = await _contentFilterService.IsContentApprovedAsync(review.Text);
            review.IsApproved = isApproved;

            if (!isApproved)
            {
                review.FlaggedWords = await _contentFilterService.GetFlaggedWordsAsync(review.Text);
            }

            review.CreatedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            var result = await _reviewRepository.CreateAsync(review);

            // If approved, update movie rating
            if (isApproved)
            {
                await UpdateMovieRating(review.MovieId);
            }

            return result;
        }

        public async Task UpdateReviewAsync(string id, ReviewModel review)
        {
            var existingReview = await _reviewRepository.GetByIdAsync(new ObjectId(id));
            if (existingReview != null)
            {
                // Check if content changed and re-filter if needed
                if (existingReview.Text != review.Text)
                {
                    bool isApproved = await _contentFilterService.IsContentApprovedAsync(review.Text);
                    review.IsApproved = isApproved;

                    if (!isApproved)
                    {
                        review.FlaggedWords = await _contentFilterService.GetFlaggedWordsAsync(review.Text);
                    }
                    else
                    {
                        review.FlaggedWords = new List<string>();
                    }
                }

                review.UpdatedAt = DateTime.UtcNow;
                await _reviewRepository.UpdateAsync(new ObjectId(id), review);

                // Re-calculate movie rating if the review rating changed
                if (existingReview.Rating != review.Rating || existingReview.IsApproved != review.IsApproved)
                {
                    await UpdateMovieRating(review.MovieId);
                }
            }
        }

        public async Task DeleteReviewAsync(string id)
        {
            var review = await _reviewRepository.GetByIdAsync(new ObjectId(id));
            if (review != null)
            {
                var movieId = review.MovieId;
                await _reviewRepository.DeleteAsync(new ObjectId(id));
                await UpdateMovieRating(movieId);
            }
        }

        public async Task<List<ReviewModel>> GetFilteredReviewsAsync(string movieId, string sortBy, int? rating)
        {
            var reviews = await _reviewRepository.GetByMovieIdAsync(new ObjectId(movieId));

            // Filter by rating if provided
            if (rating.HasValue)
            {
                reviews = reviews.Where(r => Math.Floor(r.Rating) == rating.Value).ToList();
            }

            // Apply sorting
            switch (sortBy)
            {
                case "rating_asc":
                    reviews = reviews.OrderBy(r => r.Rating).ToList();
                    break;
                case "rating_desc":
                    reviews = reviews.OrderByDescending(r => r.Rating).ToList();
                    break;
                case "date_asc":
                    reviews = reviews.OrderBy(r => r.CreatedAt).ToList();
                    break;
                case "date_desc":
                default:
                    reviews = reviews.OrderByDescending(r => r.CreatedAt).ToList();
                    break;
            }

            return reviews;
        }

        private async Task UpdateMovieRating(ObjectId movieId)
        {
            var movie = await _movieRepository.GetByIdAsync(movieId);
            if (movie != null)
            {
                var reviews = await _reviewRepository.GetByMovieIdAsync(movieId);
                var approvedReviews = reviews.Where(r => r.IsApproved).ToList();

                if (approvedReviews.Any())
                {
                    movie.AverageRating = approvedReviews.Average(r => r.Rating);
                    movie.ReviewCount = approvedReviews.Count;
                }
                else
                {
                    movie.AverageRating = 0;
                    movie.ReviewCount = 0;
                }

                await _movieRepository.UpdateAsync(movieId, movie);
            }
        }
    }
}