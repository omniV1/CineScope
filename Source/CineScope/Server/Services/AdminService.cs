using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Shared.DTOs;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CineScope.Server.Services
{
    public class AdminService
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly MongoDbSettings _settings;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IMongoDbService mongoDbService,
            IOptions<MongoDbSettings> settings,
            ILogger<AdminService> logger)
        {
            _mongoDbService = mongoDbService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<List<ReviewModerationDto>> GetFlaggedReviewsAsync()
        {
            try
            {
                var reviewsCollection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);
                var usersCollection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);
                var moviesCollection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

                // Get all flagged/non-approved reviews
                var flaggedReviews = await reviewsCollection
                    .Find(r => !r.IsApproved)
                    .ToListAsync();

                // Convert to DTOs with usernames and movie titles
                var result = new List<ReviewModerationDto>();
                foreach (var review in flaggedReviews)
                {
                    var user = await usersCollection.Find(u => u.Id == review.UserId).FirstOrDefaultAsync();
                    var movie = await moviesCollection.Find(m => m.Id == review.MovieId).FirstOrDefaultAsync();

                    result.Add(new ReviewModerationDto
                    {
                        Id = review.Id,
                        UserId = review.UserId,
                        Username = user?.Username ?? "Unknown User",
                        MovieId = review.MovieId,
                        MovieTitle = movie?.Title ?? "Unknown Movie",
                        Rating = review.Rating,
                        Text = review.Text,
                        CreatedAt = review.CreatedAt,
                        FlaggedWords = review.FlaggedWords?.ToList() ?? new List<string>(),
                        ModerationStatus = "Pending"
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flagged reviews");
                throw;
            }
        }



        /// <summary>
        /// Gets dashboard statistics for the admin dashboard.
        /// </summary>
        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            // This method already returns a DTO, so we keep it as is
            try
            {
                // Get collections
                var usersCollection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);
                var moviesCollection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
                var reviewsCollection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);

                // Count documents in collections
                var userCount = await usersCollection.CountDocumentsAsync(Builders<User>.Filter.Empty);
                var movieCount = await moviesCollection.CountDocumentsAsync(Builders<Movie>.Filter.Empty);
                var reviewCount = await reviewsCollection.CountDocumentsAsync(Builders<Review>.Filter.Empty);

                // Count flagged content
                var flaggedReviewsCount = await reviewsCollection.CountDocumentsAsync(
                    Builders<Review>.Filter.Eq(r => r.IsApproved, false));

                // Get recent activity
                var recentReviews = await reviewsCollection
                    .Find(Builders<Review>.Filter.Empty)
                    .Sort(Builders<Review>.Sort.Descending(r => r.CreatedAt))
                    .Limit(10)
                    .ToListAsync();

                // Convert to DTOs and get usernames
                var recentActivity = new List<RecentActivityDto>();
                foreach (var review in recentReviews)
                {
                    var user = await usersCollection.Find(u => u.Id == review.UserId).FirstOrDefaultAsync();
                    var movie = await moviesCollection.Find(m => m.Id == review.MovieId).FirstOrDefaultAsync();

                    recentActivity.Add(new RecentActivityDto
                    {
                        Timestamp = review.CreatedAt,
                        Username = user?.Username ?? "Unknown User",
                        ActionType = "NewReview",
                        Details = $"{review.Rating}★ review for \"{movie?.Title ?? "Unknown Movie"}\""
                    });
                }

                // Get collection statistics
                var collectionStats = await GetCollectionStatsAsync();

                return new DashboardStatsDto
                {
                    TotalUsers = (int)userCount,
                    TotalMovies = (int)movieCount,
                    TotalReviews = (int)reviewCount,
                    FlaggedContent = (int)flaggedReviewsCount,
                    RecentActivity = recentActivity,
                    CollectionStats = collectionStats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                throw;
            }
        }

        /// <summary>
        /// Gets all users with additional admin information.
        /// </summary>
        public async Task<List<UserAdminDto>> GetAllUsersAsync(string? searchTerm = null, string? role = null, string? status = null)
        {
            // This method already returns DTOs, so we keep it as is
            try
            {
                var usersCollection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);
                var reviewsCollection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);

                // Build filter based on parameters
                var filter = Builders<User>.Filter.Empty;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var searchFilter = Builders<User>.Filter.Regex(u => u.Username, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")) |
                                      Builders<User>.Filter.Regex(u => u.Email, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));
                    filter = Builders<User>.Filter.And(filter, searchFilter);
                }

                if (!string.IsNullOrEmpty(role))
                {
                    filter = Builders<User>.Filter.And(filter, Builders<User>.Filter.AnyEq(u => u.Roles, role));
                }

                var users = await usersCollection.Find(filter).ToListAsync();
                var result = new List<UserAdminDto>();

                foreach (var user in users)
                {
                    // Count reviews by this user
                    var reviewCount = await reviewsCollection.CountDocumentsAsync(
                        Builders<Review>.Filter.Eq(r => r.UserId, user.Id));

                    // Determine status
                    string userStatus = "Active";
                    if (user.IsLocked)
                    {
                        userStatus = "Suspended";
                    }
                    else if (await reviewsCollection.CountDocumentsAsync(
                        Builders<Review>.Filter.And(
                            Builders<Review>.Filter.Eq(r => r.UserId, user.Id),
                            Builders<Review>.Filter.Eq(r => r.IsApproved, false))) > 0)
                    {
                        userStatus = "Flagged";
                    }

                    // Skip if status filter is applied and doesn't match
                    if (!string.IsNullOrEmpty(status) && status != userStatus)
                    {
                        continue;
                    }

                    result.Add(new UserAdminDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        Roles = user.Roles,
                        JoinDate = user.CreatedAt,
                        ReviewCount = (int)reviewCount,
                        LastLogin = user.LastLogin,
                        Status = userStatus
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users for admin");
                throw;
            }
        }

        /// <summary>
        /// Gets all banned words.
        /// </summary>
        public async Task<List<BannedWordDto>> GetAllBannedWordsAsync(string? category = null, int? severity = null)
        {
            // Updated to return DTOs instead of domain models
            try
            {
                var bannedWordsCollection = _mongoDbService.GetCollection<BannedWord>(_settings.BannedWordsCollectionName);

                var filter = Builders<BannedWord>.Filter.Empty;

                if (!string.IsNullOrEmpty(category))
                {
                    filter = Builders<BannedWord>.Filter.And(filter,
                        Builders<BannedWord>.Filter.Eq(w => w.Category, category));
                }

                if (severity.HasValue)
                {
                    filter = Builders<BannedWord>.Filter.And(filter,
                        Builders<BannedWord>.Filter.Eq(w => w.Severity, severity.Value));
                }

                var bannedWords = await bannedWordsCollection.Find(filter).ToListAsync();

                // Map domain models to DTOs
                return bannedWords.Select(w => MapToDto(w)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting banned words");
                throw;
            }
        }

        /// <summary>
        /// Gets statistics for all MongoDB collections.
        /// </summary>
        public async Task<Dictionary<string, long>> GetCollectionStatsAsync()
        {
            var stats = new Dictionary<string, long>();

            // Count documents in each collection
            stats["Users"] = await _mongoDbService.GetCollection<User>(_settings.UsersCollectionName)
                .CountDocumentsAsync(Builders<User>.Filter.Empty);

            stats["Movies"] = await _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName)
                .CountDocumentsAsync(Builders<Movie>.Filter.Empty);

            stats["Reviews"] = await _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName)
                .CountDocumentsAsync(Builders<Review>.Filter.Empty);

            stats["BannedWords"] = await _mongoDbService.GetCollection<BannedWord>(_settings.BannedWordsCollectionName)
                .CountDocumentsAsync(Builders<BannedWord>.Filter.Empty);

            return stats;
        }

        /// <summary>
        /// Adds a new banned word to the database.
        /// </summary>
        public async Task<BannedWordDto> AddBannedWordAsync(BannedWordDto bannedWordDto)
        {
            // Updated to accept and return DTOs
            try
            {
                var bannedWordsCollection = _mongoDbService.GetCollection<BannedWord>(_settings.BannedWordsCollectionName);

                // Convert DTO to domain model
                var bannedWord = MapToModel(bannedWordDto);

                // Ensure the word doesn't already exist
                var existingWord = await bannedWordsCollection.Find(w => w.Word == bannedWord.Word).FirstOrDefaultAsync();
                if (existingWord != null)
                {
                    throw new InvalidOperationException($"The word '{bannedWord.Word}' is already in the banned words list.");
                }

                // Set creation date and ensure IsActive is set
                bannedWord.IsActive = true;

                await bannedWordsCollection.InsertOneAsync(bannedWord);

                // Map the inserted model back to DTO and return
                return MapToDto(bannedWord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding banned word: {bannedWordDto.Word}");
                throw;
            }
        }

        /// <summary>
        /// Updates a user's status (Active, Suspended).
        /// </summary>
        public async Task UpdateUserStatusAsync(string userId, string status)
        {
            try
            {
                var usersCollection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);

                var update = Builders<User>.Update;
                var updates = new List<UpdateDefinition<User>>();

                // Set IsLocked based on status
                if (status == "Suspended")
                {
                    updates.Add(update.Set(u => u.IsLocked, true));
                }
                else if (status == "Active")
                {
                    updates.Add(update.Set(u => u.IsLocked, false));
                    updates.Add(update.Set(u => u.FailedLoginAttempts, 0));
                }

                if (updates.Count > 0)
                {
                    var combinedUpdate = update.Combine(updates);
                    await usersCollection.UpdateOneAsync(u => u.Id == userId, combinedUpdate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user status for user ID: {userId}");
                throw;
            }
        }

        /// <summary>
        /// Updates a user's admin privileges
        /// </summary>
        public async Task ToggleUserAdminPrivilegesAsync(string userId)
        {
            try
            {
                var usersCollection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);

                var user = await usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                var update = Builders<User>.Update;
                var updates = new List<UpdateDefinition<User>>();

                // Toggle admin role
                if (user.Roles.Contains("Admin"))
                {
                    updates.Add(update.Pull(u => u.Roles, "Admin"));
                }
                else
                {
                    updates.Add(update.AddToSet(u => u.Roles, "Admin"));
                }

                if (updates.Count > 0)
                {
                    var combinedUpdate = update.Combine(updates);
                    await usersCollection.UpdateOneAsync(u => u.Id == userId, combinedUpdate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user privileges for user ID: {userId}");
                throw;
            }
        }

        /// <summary>
        /// Handles content moderation actions.
        /// </summary>
        public async Task ModerateContentAsync(string reviewId, ModerationActionDto action)
        {
            try
            {
                var reviewsCollection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);

                var review = await reviewsCollection.Find(r => r.Id == reviewId).FirstOrDefaultAsync();
                if (review == null)
                {
                    throw new KeyNotFoundException($"Review with ID {reviewId} not found");
                }

                switch (action.ActionType)
                {
                    case "Approve":
                        await reviewsCollection.UpdateOneAsync(
                            r => r.Id == reviewId,
                            Builders<Review>.Update
                                .Set(r => r.IsApproved, true)
                                .Set(r => r.FlaggedWords, Array.Empty<string>()));
                        break;

                    case "Reject":
                        await reviewsCollection.DeleteOneAsync(r => r.Id == reviewId);
                        break;

                    case "Modify":
                        await reviewsCollection.UpdateOneAsync(
                            r => r.Id == reviewId,
                            Builders<Review>.Update
                                .Set(r => r.Text, action.ModifiedContent)
                                .Set(r => r.IsApproved, true)
                                .Set(r => r.FlaggedWords, Array.Empty<string>())
                                .Set(r => r.UpdatedAt, DateTime.UtcNow));
                        break;

                    default:
                        throw new ArgumentException($"Unknown action type: {action.ActionType}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error moderating content for review ID: {reviewId}");
                throw;
            }
        }

        #region Mapping Methods

        /// <summary>
        /// Maps a BannedWord domain model to a BannedWordDto.
        /// </summary>
        private BannedWordDto MapToDto(BannedWord model)
        {
            return new BannedWordDto
            {
                Id = model.Id,
                Word = model.Word,
                Severity = model.Severity,
                Category = model.Category,
                IsActive = model.IsActive
            };
        }

        /// <summary>
        /// Maps a BannedWordDto to a BannedWord domain model.
        /// </summary>
        private BannedWord MapToModel(BannedWordDto dto)
        {
            return new BannedWord
            {
                Id = dto.Id,
                Word = dto.Word,
                Severity = dto.Severity,
                Category = dto.Category,
                IsActive = dto.IsActive
            };
        }

        #endregion
    }
}