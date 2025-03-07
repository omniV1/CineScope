using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CineScope.Services
{
    /// <summary>
    /// Service responsible for handling movie review operations
    /// Implements IReviewService to provide review functionality including content moderation
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IContentFilterService _contentFilterService;

        /// <summary>
        /// Constructor for ReviewService
        /// </summary>
        /// <param name="reviewRepository">Repository for accessing review data</param>
        /// <param name="movieRepository">Repository for accessing movie data</param>
        /// <param name="contentFilterService">Service for filtering inappropriate content</param>
        public ReviewService(
            IReviewRepository reviewRepository,
            IMovieRepository movieRepository,
            IContentFilterService contentFilterService)
        {
            _reviewRepository = reviewRepository;
            _movieRepository = movieRepository;
            _contentFilterService = contentFilterService;
        }

        /// <summary>
        /// Retrieves all reviews from the database
        /// </summary>
        /// <returns>A list of all reviews</returns>
        public async Task<List<ReviewModel>> GetAllReviewsAsync()
        {
            return await _reviewRepository.GetAllAsync();
        }

        /// <summary>
        /// Finds a review by its string ID
        /// </summary>
        /// <param name="id">String representation of the review's ObjectId</param>
        /// <returns>The review if found, null otherwise</returns>
        public async Task<ReviewModel> GetReviewByIdAsync(string id)
        {
            // Convert string ID to MongoDB ObjectId
            return await _reviewRepository.GetByIdAsync(new ObjectId(id));
        }

        /// <summary>
        /// Retrieves all reviews created by a specific user
        /// </summary>
        /// <param name="userId">String representation of the user's ObjectId</param>
        /// <returns>A list of reviews by the specified user</returns>
        public async Task<List<ReviewModel>> GetReviewsByUserIdAsync(string userId)
        {
            // Convert string ID to MongoDB ObjectId
            return await _reviewRepository.GetByUserIdAsync(new ObjectId(userId));
        }

        /// <summary>
        /// Retrieves all reviews for a specific movie
        /// </summary>
        /// <param name="movieId">String representation of the movie's ObjectId</param>
        /// <returns>A list of reviews for the specified movie</returns>
        public async Task<List<ReviewModel>> GetReviewsByMovieIdAsync(string movieId)
        {
            // Convert string ID to MongoDB ObjectId
            return await _reviewRepository.GetByMovieIdAsync(new ObjectId(movieId));
        }

        /// <summary>
        /// Creates a new review with content moderation
        /// </summary>
        /// <param name="review">The review to create</param>
        /// <returns>The created review with approval status and flagged words (if any)</returns>
        public async Task<ReviewModel> CreateReviewAsync(ReviewModel review)
        {
            // Check content against filter for inappropriate language
            bool isApproved = await _contentFilterService.IsContentApprovedAsync(review.Text);
            review.IsApproved = isApproved;

            // If not approved, get list of flagged words for moderation
            if (!isApproved)
            {
                review.FlaggedWords = await _contentFilterService.GetFlaggedWordsAsync(review.Text);
            }

            // Set timestamps
            review.CreatedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            // Save the review
            var result = await _reviewRepository.CreateAsync(review);

            // If review is approved, update the movie's rating
            if (isApproved)
            {
                await UpdateMovieRating(review.MovieId);
            }

            return result;
        }

        /// <summary>
        /// Updates an existing review with content re-moderation if text changed
        /// </summary>
        /// <param name="id">String representation of the review's ObjectId</param>
        /// <param name="review">The updated review data</param>
        public async Task UpdateReviewAsync(string id, ReviewModel review)
        {
            var existingReview = await _reviewRepository.GetByIdAsync(new ObjectId(id));
            if (existingReview != null)
            {
                // Re-check content filter if text was changed
                if (existingReview.Text != review.Text)
                {
                    bool isApproved = await _contentFilterService.IsContentApprovedAsync(review.Text);
                    review.IsApproved = isApproved;

                    if (!isApproved)
                    {
                        // Get updated list of flagged words
                        review.FlaggedWords = await _contentFilterService.GetFlaggedWordsAsync(review.Text);
                    }
                    else
                    {
                        // Clear any previously flagged words
                        review.FlaggedWords = new List<string>();
                    }
                }

                // Update timestamp
                review.UpdatedAt = DateTime.UtcNow;
                await _reviewRepository.UpdateAsync(new ObjectId(id), review);

                // Re-calculate movie rating if the review rating or approval status changed
                if (existingReview.Rating != review.Rating || existingReview.IsApproved != review.IsApproved)
                {
                    await UpdateMovieRating(review.MovieId);
                }
            }
        }

        /// <summary>
        /// Deletes a review and updates associated movie rating
        /// </summary>
        /// <param name="id">String representation of the review's ObjectId</param>
        public async Task DeleteReviewAsync(string id)
        {
            var review = await _reviewRepository.GetByIdAsync(new ObjectId(id));
            if (review != null)
            {
                // Store the movie ID for rating update after deletion
                var movieId = review.MovieId;

                // Delete the review
                await _reviewRepository.DeleteAsync(new ObjectId(id));

                // Update the movie's average rating since a review was removed
                await UpdateMovieRating(movieId);
            }
        }

        /// <summary>
        /// Gets reviews for a specific movie with filtering and sorting options
        /// </summary>
        /// <param name="movieId">String representation of the movie's ObjectId</param>
        /// <param name="sortBy">Sort option (rating_asc, rating_desc, date_asc, date_desc)</param>
        /// <param name="rating">Optional filter by specific rating value</param>
        /// <returns>A filtered and sorted list of reviews</returns>
        public async Task<List<ReviewModel>> GetFilteredReviewsAsync(string movieId, string sortBy, int? rating)
        {
            // Get all reviews for the movie
            var reviews = await _reviewRepository.GetByMovieIdAsync(new ObjectId(movieId));

            // Filter by specific rating if provided
            if (rating.HasValue)
            {
                // Use Math.Floor to handle decimal ratings (e.g., 4.7 would be filtered as 4)
                reviews = reviews.Where(r => Math.Floor(r.Rating) == rating.Value).ToList();
            }

            // Apply sorting based on the sortBy parameter
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
                    // Default sort is newest first
                    reviews = reviews.OrderByDescending(r => r.CreatedAt).ToList();
                    break;
            }

            return reviews;
        }

        /// <summary>
        /// Updates a movie's average rating and review count based on its approved reviews
        /// </summary>
        /// <param name="movieId">The ObjectId of the movie to update</param>
        private async Task UpdateMovieRating(ObjectId movieId)
        {
            var movie = await _movieRepository.GetByIdAsync(movieId);
            if (movie != null)
            {
                // Get all reviews for the movie
                var reviews = await _reviewRepository.GetByMovieIdAsync(movieId);

                // Filter to only include approved reviews for rating calculation
                var approvedReviews = reviews.Where(r => r.IsApproved).ToList();

                if (approvedReviews.Any())
                {
                    // Calculate new average rating from approved reviews
                    movie.AverageRating = approvedReviews.Average(r => r.Rating);
                    movie.ReviewCount = approvedReviews.Count;
                }
                else
                {
                    // Reset rating if no approved reviews exist
                    movie.AverageRating = 0;
                    movie.ReviewCount = 0;
                }

                // Update the movie with new rating information
                await _movieRepository.UpdateAsync(movieId, movie);
            }
        }
    }
}

