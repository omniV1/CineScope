# CineScope Admin Interface Implementation Guide

Let's go through the detailed implementation for each ticket:


Check the existing User model to ensure it has a Roles property:

```csharp
// This is already in your Server/Models/User.cs
public class User
{
    // ...
    public List<string> Roles { get; set; } = new List<string>();
    // ...
}
```

our application already has the necessary structure for roles!

### Scrum-65: Create Admin DTOs and Models

Create a new folder called `Admin` in your `Shared` project:

***Shared/Admin/AdminModels.cs***
```csharp

namespace CineScope.Shared.Admin
{
    public class DashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalMovies { get; set; }
        public int TotalReviews { get; set; }
        public int FlaggedContent { get; set; }
        public List<RecentActivityDto> RecentActivity { get; set; } = new();
        public Dictionary<string, long> CollectionStats { get; set; } = new();
    }

    public class RecentActivityDto
    {
        public DateTime Timestamp { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // "NewReview", "FlaggedReview", etc.
        public string Details { get; set; } = string.Empty;
    }

    public class FlaggedContentDto
    {
        public string Id { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty; // "Review", "User", etc.
        public string Username { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime FlaggedAt { get; set; }
    }

    public class UserAdminDto : UserDto
    {
        public DateTime JoinDate { get; set; }
        public int ReviewCount { get; set; }
        public DateTime? LastLogin { get; set; }
        public string Status { get; set; } = "Active"; // "Active", "Flagged", "Suspended"
    }

    public class ModerationAction
    {
        public string ActionType { get; set; } = string.Empty; // "Approve", "Reject", "Modify"
        public string Reason { get; set; } = string.Empty;
        public string ModifiedContent { get; set; } = string.Empty;
    }
}
```

### Scrum-66 & 70: Implement AdminService with MongoDB Integration & Add MongoDB Import/Export Functionality

Create an `AdminService.cs` file in the `Server/Services` directory:

***Server/Services/AdminService.cs***
```csharp
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Shared.Admin;
using CineScope.Shared.DTOs;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CineScope.Server.Services
{
    /// <summary>
    /// Service responsible for admin-related operations and MongoDB management.
    /// </summary>
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

        /// <summary>
        /// Gets dashboard statistics for the admin dashboard.
        /// </summary>
        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
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

                return new DashboardStats
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
        /// Gets all banned words with optional filtering.
        /// </summary>
        public async Task<List<BannedWord>> GetAllBannedWordsAsync(string? category = null, int? severity = null)
        {
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
                
                return await bannedWordsCollection.Find(filter).ToListAsync();
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
        public async Task<BannedWord> AddBannedWordAsync(BannedWord bannedWord)
        {
            try
            {
                var bannedWordsCollection = _mongoDbService.GetCollection<BannedWord>(_settings.BannedWordsCollectionName);
                
                // Ensure the word doesn't already exist
                var existingWord = await bannedWordsCollection.Find(w => w.Word == bannedWord.Word).FirstOrDefaultAsync();
                if (existingWord != null)
                {
                    throw new InvalidOperationException($"The word '{bannedWord.Word}' is already in the banned words list.");
                }
                
                // Set creation date and ensure IsActive is set
                bannedWord.IsActive = true;
                
                await bannedWordsCollection.InsertOneAsync(bannedWord);
                return bannedWord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding banned word: {bannedWord.Word}");
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
        /// Handles content moderation actions.
        /// </summary>
        public async Task ModerateContentAsync(string reviewId, ModerationAction action)
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

        /// <summary>
        /// Gets all flagged reviews that need moderation.
        /// </summary>
        public async Task<List<FlaggedContentDto>> GetFlaggedContentAsync()
        {
            try
            {
                var reviewsCollection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);
                var usersCollection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);
                var moviesCollection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
                
                // Get all reviews that are not approved
                var flaggedReviews = await reviewsCollection
                    .Find(r => r.IsApproved == false)
                    .ToListAsync();
                
                var result = new List<FlaggedContentDto>();
                
                foreach (var review in flaggedReviews)
                {
                    var user = await usersCollection.Find(u => u.Id == review.UserId).FirstOrDefaultAsync();
                    var movie = await moviesCollection.Find(m => m.Id == review.MovieId).FirstOrDefaultAsync();
                    
                    result.Add(new FlaggedContentDto
                    {
                        Id = review.Id,
                        ContentType = "Review",
                        Username = user?.Username ?? "Unknown User",
                        MovieTitle = movie?.Title ?? "Unknown Movie",
                        Reason = string.Join(", ", review.FlaggedWords),
                        FlaggedAt = review.CreatedAt
                    });
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flagged content");
                throw;
            }
        }

        /// <summary>
        /// Updates or toggles the status of a banned word.
        /// </summary>
        public async Task UpdateBannedWordStatusAsync(string wordId, bool isActive)
        {
            try
            {
                var bannedWordsCollection = _mongoDbService.GetCollection<BannedWord>(_settings.BannedWordsCollectionName);
                
                var update = Builders<BannedWord>.Update
                    .Set(w => w.IsActive, isActive);
                    
                await bannedWordsCollection.UpdateOneAsync(w => w.Id == wordId, update);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating banned word status: {wordId}");
                throw;
            }
        }
    }
}
```
### Scrum-67: Implement DataSeedService for MongoDB Initialization

Create a `DataSeedService.cs` file in the `Server/Services` directory:

***Server/Services/DataSeedService.cs***
```csharp
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CineScope.Server.Services
{
    /// <summary>
    /// Service responsible for initializing and seeding MongoDB collections.
    /// </summary>
    public class DataSeedService
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly MongoDbSettings _settings;
        private readonly ILogger<DataSeedService> _logger;

        public DataSeedService(
            IMongoDbService mongoDbService,
            IOptions<MongoDbSettings> settings,
            ILogger<DataSeedService> logger)
        {
            _mongoDbService = mongoDbService;
            _settings = settings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Seeds initial data and creates MongoDB indexes.
        /// </summary>
        public async Task SeedInitialDataAsync()
        {
            await SeedAdminUserAsync();
            await SeedBannedWordsAsync();
            await SeedMoviesIfEmptyAsync();
            await CreateDatabaseIndexesAsync();
        }

        /// <summary>
        /// Creates a default admin user if none exists.
        /// </summary>
        private async Task SeedAdminUserAsync()
        {
            try
            {
                var usersCollection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);
                
                // Check if admin user already exists
                var adminUser = await usersCollection.Find(u => u.Username == "AdminUser").FirstOrDefaultAsync();
                
                if (adminUser == null)
                {
                    // Create admin user
                    var newAdmin = new User
                    {
                        Username = "AdminUser",
                        Email = "admin@cinescope.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), // Change in production!
                        Roles = new List<string> { "Admin", "User" },
                        CreatedAt = DateTime.UtcNow,
                        IsLocked = false,
                        FailedLoginAttempts = 0,
                        ProfilePictureUrl = "/profile-pictures/avatar8.svg"
                    };
                    
                    await usersCollection.InsertOneAsync(newAdmin);
                    _logger.LogInformation("Admin user created successfully");
                }
                else
                {
                    // Ensure admin has Admin role
                    if (!adminUser.Roles.Contains("Admin"))
                    {
                        var roles = adminUser.Roles.ToList();
                        roles.Add("Admin");
                        var update = Builders<User>.Update.Set(u => u.Roles, roles);
                        await usersCollection.UpdateOneAsync(u => u.Id == adminUser.Id, update);
                        _logger.LogInformation("Admin role added to existing admin user");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding admin user");
            }
        }

        /// <summary>
        /// Seeds initial banned words if the collection is empty.
        /// </summary>
        private async Task SeedBannedWordsAsync()
        {
            try
            {
                var bannedWordsCollection = _mongoDbService.GetCollection<BannedWord>(_settings.BannedWordsCollectionName);
                
                // Check if collection is empty
                if (await bannedWordsCollection.CountDocumentsAsync(Builders<BannedWord>.Filter.Empty) > 0)
                    return;

                // These are placeholder banned words for demonstration
                // In a real app, you'd use actual banned words
                var bannedWords = new List<BannedWord>
                {
                    new BannedWord { 
                        Word = "badword1", 
                        Severity = 3, 
                        Category = "Profanity", 
                        IsActive = true 
                    },
                    new BannedWord { 
                        Word = "badword2", 
                        Severity = 2, 
                        Category = "Profanity", 
                        IsActive = true 
                    },
                    new BannedWord { 
                        Word = "badword3", 
                        Severity = 5, 
                        Category = "Hate Speech", 
                        IsActive = true 
                    },
                    new BannedWord { 
                        Word = "badword4", 
                        Severity = 4, 
                        Category = "Harassment", 
                        IsActive = true 
                    },
                };

                await bannedWordsCollection.InsertManyAsync(bannedWords);
                _logger.LogInformation("Seeded banned words");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding banned words");
            }
        }

        /// <summary>
        /// Seeds initial movies if the collection is empty.
        /// </summary>
        private async Task SeedMoviesIfEmptyAsync()
        {
            try
            {
                var moviesCollection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
                
                // Check if collection is empty
                if (await moviesCollection.CountDocumentsAsync(Builders<Movie>.Filter.Empty) > 0)
                    return;

                var movies = new List<Movie>
                {
                    new Movie
                    {
                        Title = "The Shawshank Redemption",
                        Description = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.",
                        ReleaseDate = new DateTime(1994, 9, 23),
                        Genres = new List<string> { "Drama" },
                        Director = "Frank Darabont",
                        Actors = new List<string> { "Tim Robbins", "Morgan Freeman", "Bob Gunton" },
                        PosterUrl = "https://m.media-amazon.com/images/M/MV5BNDE3ODcxYzMtY2YzZC00NmNlLWJiNDMtZDViZWM2MzIxZDYwXkEyXkFqcGdeQXVyNjAwNDUxODI@._V1_.jpg",
                        AverageRating = 4.7,
                        ReviewCount = 0
                    },
                    new Movie
                    {
                        Title = "The Godfather",
                        Description = "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant son.",
                        ReleaseDate = new DateTime(1972, 3, 24),
                        Genres = new List<string> { "Crime", "Drama" },
                        Director = "Francis Ford Coppola",
                        Actors = new List<string> { "Marlon Brando", "Al Pacino", "James Caan" },
                        PosterUrl = "https://m.media-amazon.com/images/M/MV5BM2MyNjYxNmUtYTAwNi00MTYxLWJmNWYtYzZlODY3ZTk3OTFlXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg",
                        AverageRating = 4.8,
                        ReviewCount = 0
                    },
                    new Movie
                    {
                        Title = "The Dark Knight",
                        Description = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.",
                        ReleaseDate = new DateTime(2008, 7, 18),
                        Genres = new List<string> { "Action", "Crime", "Drama" },
                        Director = "Christopher Nolan",
                        Actors = new List<string> { "Christian Bale", "Heath Ledger", "Aaron Eckhart" },
                        PosterUrl = "https://m.media-amazon.com/images/M/MV5BMTMxNTMwODM0NF5BMl5BanBnXkFtZTcwODAyMTk2Mw@@._V1_.jpg",
                        AverageRating = 4.6,
                        ReviewCount = 0
                    }
                };

                await moviesCollection.InsertManyAsync(movies);
                _logger.LogInformation("Seeded initial movies");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding movies");
            }
        }

        /// <summary>
        /// Creates MongoDB indexes for better query performance.
        /// </summary>
        private async Task CreateDatabaseIndexesAsync()
        {
            try
            {
                // Users collection indexes
                var usersCollection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);
                
                // Create index on Username (unique)
                var usernameIndexModel = new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(u => u.Username),
                    new CreateIndexOptions { Unique = true, Name = "UniqueUsername" }
                );
                
                // Create index on Email (unique)
                var emailIndexModel = new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(u => u.Email),
                    new CreateIndexOptions { Unique = true, Name = "UniqueEmail" }
                );
                
                await usersCollection.Indexes.CreateOneAsync(usernameIndexModel);
                await usersCollection.Indexes.CreateOneAsync(emailIndexModel);
                
                // Reviews collection indexes
                var reviewsCollection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);
                
                // Create index on MovieId for faster review lookups by movie
                var reviewMovieIndexModel = new CreateIndexModel<Review>(
                    Builders<Review>.IndexKeys.Ascending(r => r.MovieId),
                    new CreateIndexOptions { Name = "MovieIdIndex" }
                );
                
                // Create index on UserId for faster user reviews lookups
                var reviewUserIndexModel = new CreateIndexModel<Review>(
                    Builders<Review>.IndexKeys.Ascending(r => r.UserId),
                    new CreateIndexOptions { Name = "UserIdIndex" }
                );
                
                // Create index on IsApproved for quick flagged content lookups
                var reviewApprovalIndexModel = new CreateIndexModel<Review>(
                    Builders<Review>.IndexKeys.Ascending(r => r.IsApproved),
                    new CreateIndexOptions { Name = "IsApprovedIndex" }
                );
                
                await reviewsCollection.Indexes.CreateOneAsync(reviewMovieIndexModel);
                await reviewsCollection.Indexes.CreateOneAsync(reviewUserIndexModel);
                await reviewsCollection.Indexes.CreateOneAsync(reviewApprovalIndexModel);
                
                // Movies collection indexes
                var moviesCollection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
                
                // Create text index on Title for search functionality
                var movieTitleIndexModel = new CreateIndexModel<Movie>(
                    Builders<Movie>.IndexKeys.Text(m => m.Title),
                    new CreateIndexOptions { Name = "TitleTextIndex" }
                );
                
                // Create index on Genres for faster genre-specific lookups
                var movieGenreIndexModel = new CreateIndexModel<Movie>(
                    Builders<Movie>.IndexKeys.Ascending("Genres"),
                    new CreateIndexOptions { Name = "GenresIndex" }
                );
                
                await moviesCollection.Indexes.CreateOneAsync(movieTitleIndexModel);
                await moviesCollection.Indexes.CreateOneAsync(movieGenreIndexModel);
                
                _logger.LogInformation("Created MongoDB indexes for better performance");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MongoDB indexes");
            }
        }
    }
}
```
### Scrum-68: Create Admin API Controller

Create an `AdminController.cs` file in the `Server/Controllers` directory:

***Server/Controllers/AdminController.cs***
```csharp
using CineScope.Server.Models;
using CineScope.Server.Services;
using CineScope.Shared.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CineScope.Server.Controllers
{
    /// <summary>
    /// API controller for admin-related operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/Admin/dashboard
        /// Gets dashboard statistics for the admin dashboard.
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardStats>> GetDashboardStats()
        {
            try
            {
                var stats = await _adminService.GetDashboardStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new { Message = "Error retrieving dashboard statistics", Error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/Admin/collection-stats
        /// Gets statistics for all MongoDB collections.
        /// </summary>
        [HttpGet("collection-stats")]
        public async Task<ActionResult<Dictionary<string, long>>> GetCollectionStats()
        {
            try
            {
                var stats = await _adminService.GetCollectionStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collection stats");
                return StatusCode(500, new { Message = "Error retrieving collection statistics", Error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/Admin/users
        /// Gets all users with optional filtering.
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult<List<UserAdminDto>>> GetUsers([FromQuery] string? search = null, [FromQuery] string? role = null, [FromQuery] string? status = null)
        {
            try
            {
                var users = await _adminService.GetAllUsersAsync(search, role, status);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for admin");
                return StatusCode(500, new { Message = "Error retrieving user data", Error = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/Admin/users/{userId}/status
        /// Updates a user's status (Active, Suspended).
        /// </summary>
        [HttpPut("users/{userId}/status")]
        public async Task<IActionResult> UpdateUserStatus(string userId, [FromBody] string status)
        {
            try
            {
                await _adminService.UpdateUserStatusAsync(userId, status);
                return Ok(new { Message = $"User status updated to {status}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for user {userId}");
                return StatusCode(500, new { Message = "Error updating user status", Error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/Admin/banned-words
        /// Gets all banned words with optional filtering.
        /// </summary>
        [HttpGet("banned-words")]
        public async Task<ActionResult<List<BannedWord>>> GetBannedWords([FromQuery] string? category = null, [FromQuery] int? severity = null)
        {
            try
            {
                var bannedWords = await _adminService.GetAllBannedWordsAsync(category, severity);
                return Ok(bannedWords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting banned words");
                return StatusCode(500, new { Message = "Error retrieving banned words", Error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/Admin/banned-words
        /// Adds a new banned word.
        /// </summary>
        [HttpPost("banned-words")]
        public async Task<ActionResult<BannedWord>> AddBannedWord([FromBody] BannedWord bannedWord)
        {
            try
            {
                var result = await _adminService.AddBannedWordAsync(bannedWord);
                return CreatedAtAction(nameof(GetBannedWords), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding banned word");
                return StatusCode(500, new { Message = "Error adding banned word", Error = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/Admin/banned-words/{wordId}/status
        /// Updates a banned word's status.
        /// </summary>
        [HttpPut("banned-words/{wordId}/status")]
        public async Task<IActionResult> UpdateBannedWordStatus(string wordId, [FromBody] bool isActive)
        {
            try
            {
                await _adminService.UpdateBannedWordStatusAsync(wordId, isActive);
                return Ok(new { Message = $"Banned word status updated to {(isActive ? "active" : "inactive")}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating banned word status {wordId}");
                return StatusCode(500, new { Message = "Error updating banned word status", Error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/Admin/flagged-content
        /// Gets all flagged content that needs moderation.
        /// </summary>
        [HttpGet("flagged-content")]
        public async Task<ActionResult<List<FlaggedContentDto>>> GetFlaggedContent()
        {
            try
            {
                var flaggedContent = await _adminService.GetFlaggedContentAsync();
                return Ok(flaggedContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flagged content");
                return StatusCode(500, new { Message = "Error retrieving flagged content", Error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/Admin/moderate/{reviewId}
        /// Moderates a review (approve, reject, modify).
        /// </summary>
        [HttpPost("moderate/{reviewId}")]
        public async Task<IActionResult> ModerateContent(string reviewId, [FromBody] ModerationAction action)
        {
            try
            {
                await _adminService.ModerateContentAsync(reviewId, action);
                return Ok(new { Message = $"Review {reviewId} moderated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error moderating content for review {reviewId}");
                return StatusCode(500, new { Message = "Error moderating content", Error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/Admin/export/{collectionName}
        /// Exports a MongoDB collection as JSON.
        /// </summary>
        [HttpGet("export/{collectionName}")]
        public async Task<IActionResult> ExportCollection(string collectionName)
        {
            try
            {
                // Validate collection name
                if (!new[] { "Users", "Movies", "Reviews", "BannedWords" }.Contains(collectionName))
                {
                    return BadRequest("Invalid collection name");
                }

                // Get the collection data based on the name
                object data = collectionName switch
                {
                    "Users" => await _adminService.GetAllUsersAsync(),
                    "BannedWords" => await _adminService.GetAllBannedWordsAsync(),
                    "FlaggedContent" => await _adminService.GetFlaggedContentAsync(),
                    _ => null
                };

                if (data == null)
                    return NotFound();

                // Serialize to JSON
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(data, jsonOptions);
                
                // Return as downloadable file
                var bytes = Encoding.UTF8.GetBytes(json);
                return File(bytes, "application/json", $"{collectionName}_{DateTime.Now:yyyyMMdd}.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting {collectionName}");
                return StatusCode(500, new { Message = "Error exporting data", Error = ex.Message });
            }
        }
    }
}
```


### Scrum-73: Create Admin Shared Components

First, let's create a shared breadcrumb component for admin pages:

***Client/Components/Admin/AdminBreadcrumb.razor***
```csharp
@using Microsoft.AspNetCore.Components
@inject NavigationManager NavigationManager

<MudBreadcrumbs Items="@breadcrumbs" Separator="→"></MudBreadcrumbs>

@code {
    private List<BreadcrumbItem> breadcrumbs = new List<BreadcrumbItem>();

    [Parameter]
    public string CurrentPage { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Admin", href: "/admin", icon: Icons.Material.Filled.Dashboard)
        };

        if (!string.IsNullOrEmpty(CurrentPage))
        {
            breadcrumbs.Add(new BreadcrumbItem(CurrentPage, href: null, disabled: true));
        }
    }
}
```


Now, let's create a confirmation dialog component that we'll use for dangerous actions in the admin interface:

***Client/Components/Admin/MudConfirmationDialog.razor***
```csharp
@inject MudBlazor.IDialogService DialogService

<MudDialog>
    <DialogContent>
        <MudText>@ContentText</MudText>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="@Color" Variant="Variant.Filled" OnClick="Submit">@ButtonText</MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }

    [Parameter] public string ContentText { get; set; }
    [Parameter] public string ButtonText { get; set; } = "OK";
    [Parameter] public Color Color { get; set; } = Color.Primary;

    void Submit() => MudDialog.Close(DialogResult.Ok(true));
    void Cancel() => MudDialog.Cancel();
}
```


### Scrum-69: Create Admin Layout

Let's create a dedicated layout for admin pages:

***Client/Shared/AdminLayout.razor***
```csharp
@inherits LayoutComponentBase
@using CineScope.Client.Components.Admin
@using Microsoft.AspNetCore.Components.Authorization
@inject NavigationManager NavigationManager
@inject ISnackbar Snackbar
@inject AuthenticationStateProvider AuthStateProvider

<AuthorizeView Roles="Admin">
    <Authorized>
        <MudLayout>
            <MudAppBar Elevation="1" Color="Color.Error">
                <MudIconButton Icon="@Icons.Material.Filled.Menu" Color="Color.Inherit" Edge="Edge.Start" OnClick="@ToggleDrawer" />
                <MudText Typo="Typo.h6" Class="ml-2">CineScope Admin</MudText>
                <MudSpacer />
                <MudText>@context.User.Identity.Name</MudText>
                <MudIconButton Icon="@Icons.Material.Filled.ExitToApp" Color="Color.Inherit" Edge="Edge.End" OnClick="@ReturnToMain" />
            </MudAppBar>

            <MudDrawer @bind-Open="@drawerOpen" Elevation="1" ClipMode="DrawerClipMode.Always">
                <MudNavMenu Color="Color.Error">
                    <MudText Typo="Typo.subtitle2" Class="px-4 py-2">ADMIN PANEL</MudText>
                    <MudDivider Class="mb-2" />
                    <MudNavLink Href="/admin" Match="NavLinkMatch.All" Icon="@Icons.Material.Filled.Dashboard">Dashboard</MudNavLink>
                    <MudNavLink Href="/admin/manage" Match="NavLinkMatch.Prefix" Icon="@Icons.Material.Filled.Settings">Management</MudNavLink>
                    <MudDivider Class="my-2" />
                    <MudNavLink OnClick="@ReturnToMain" Icon="@Icons.Material.Filled.ArrowBack">Return to Main Site</MudNavLink>
                </MudNavMenu>
            </MudDrawer>

            <MudMainContent>
                <MudContainer MaxWidth="MaxWidth.ExtraExtraLarge" Class="pa-4">
                    @Body
                </MudContainer>
            </MudMainContent>
        </MudLayout>
    </Authorized>
    <NotAuthorized>
        <MudContainer MaxWidth="MaxWidth.Small" Class="d-flex flex-column align-center justify-center" Style="height: 100vh;">
            <MudAlert Severity="Severity.Error" Class="mb-4">
                <MudText Typo="Typo.h5">Access Denied</MudText>
                <MudText Typo="Typo.body1">You don't have permission to access the admin area.</MudText>
            </MudAlert>
            <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="@ReturnToMain">Return to Main Site</MudButton>
        </MudContainer>
    </NotAuthorized>
</AuthorizeView>

@code {
    private bool drawerOpen = true;

    private void ToggleDrawer()
    {
        drawerOpen = !drawerOpen;
    }

    private void ReturnToMain()
    {
        NavigationManager.NavigateTo("/");
    }
}

```
### Scrum-73: Implement Admin Dashboard Page

Create the admin dashboard page:

***Client/Pages/Admin/AdminDashboard.razor***
```csharp
@page "/admin"
@attribute [Authorize(Roles = "Admin")]
@using CineScope.Shared.Admin
@using System.Net.Http.Json
@using CineScope.Client.Components.Admin
@inject HttpClient Http
@inject ISnackbar Snackbar
@inject NavigationManager NavigationManager

<PageTitle>CineScope - Admin Dashboard</PageTitle>

<AdminBreadcrumb />

<MudText Typo="Typo.h4" Class="mb-4">Dashboard Overview</MudText>

@if (isLoading)
{
    <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
}
else if (stats != null)
{
    <MudGrid>
        <!-- Stats Cards -->
        <MudItem xs="12" sm="6" md="3">
            <MudPaper Class="pa-4 d-flex flex-column" Elevation="2">
                <MudText Typo="Typo.subtitle1">Total Users</MudText>
                <MudText Typo="Typo.h3" Color="Color.Primary">@stats.TotalUsers</MudText>
            </MudPaper>
        </MudItem>
        <MudItem xs="12" sm="6" md="3">
            <MudPaper Class="pa-4 d-flex flex-column" Elevation="2">
                <MudText Typo="Typo.subtitle1">Total Movies</MudText>
                <MudText Typo="Typo.h3" Color="Color.Primary">@stats.TotalMovies</MudText>
            </MudPaper>
        </MudItem>
        <MudItem xs="12" sm="6" md="3">
            <MudPaper Class="pa-4 d-flex flex-column" Elevation="2">
                <MudText Typo="Typo.subtitle1">Total Reviews</MudText>
                <MudText Typo="Typo.h3" Color="Color.Primary">@stats.TotalReviews</MudText>
            </MudPaper>
        </MudItem>
        <MudItem xs="12" sm="6" md="3">
            <MudPaper Class="pa-4 d-flex flex-column" Elevation="2">
                <MudText Typo="Typo.subtitle1">Flagged Content</MudText>
                <MudText Typo="Typo.h3" Color="Color.Error">@stats.FlaggedContent</MudText>
                @if (stats.FlaggedContent > 0)
                {
                    <MudButton Variant="Variant.Text" 
                              Color="Color.Error" 
                              Size="Size.Small"
                              OnClick="@(() => NavigationManager.NavigateTo("/admin/manage?tab=2"))">
                        Moderate Now
                    </MudButton>
                }
            </MudPaper>
        </MudItem>

        <!-- MongoDB Collection Statistics -->
        <MudItem xs="12">
            <MudCard Elevation="2" Class="mt-4">
                <MudCardHeader>
                    <CardHeaderContent>
                        <MudText Typo="Typo.h6">MongoDB Collection Statistics</MudText>
                    </CardHeaderContent>
                </MudCardHeader>
                <MudCardContent>
                    <MudSimpleTable>
                        <thead>
                            <tr>
                                <th>Collection</th>
                                <th>Document Count</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            @if (stats.CollectionStats != null)
                            {
                                @foreach (var stat in stats.CollectionStats)
                                {
                                    <tr>
                                        <td>@stat.Key</td>
                                        <td>@stat.Value</td>
                                        <td>
                                            <MudButton Size="Size.Small" 
                                                     Variant="Variant.Outlined" 
                                                     Color="Color.Primary"
                                                     OnClick="@(() => NavigateToManageCollection(stat.Key))">
                                                Manage
                                            </MudButton>
                                        </td>
                                    </tr>
                                }
                            }
                        </tbody>
                    </MudSimpleTable>
                </MudCardContent>
            </MudCard>
        </MudItem>

        <!-- Recent Activity Section -->
        <MudItem xs="12">
            <MudCard Elevation="2" Class="mt-4">
                <MudCardHeader>
                    <CardHeaderContent>
                        <MudText Typo="Typo.h6">Recent Activity</MudText>
                    </CardHeaderContent>
                    <CardHeaderActions>
                        <MudIconButton Icon="@Icons.Material.Filled.Refresh" Color="Color.Default" OnClick="@RefreshDashboard" />
                    </CardHeaderActions>
                </MudCardHeader>
                <MudCardContent>
                    <MudTable Items="@stats.RecentActivity" Hover="true" Breakpoint="Breakpoint.Sm">
                        <HeaderContent>
                            <MudTh>Timestamp</MudTh>
                            <MudTh>User</MudTh>
                            <MudTh>Action</MudTh>
                            <MudTh>Details</MudTh>
                        </HeaderContent>
                        <RowTemplate>
                            <MudTd DataLabel="Timestamp">@context.Timestamp.ToString("MMM dd, yyyy HH:mm")</MudTd>
                            <MudTd DataLabel="User">@context.Username</MudTd>
                            <MudTd DataLabel="Action">
                                <MudChip Size="Size.Small" Color="@GetActionColor(context.ActionType)">@context.ActionType</MudChip>
                            </MudTd>
                            <MudTd DataLabel="Details">@context.Details</MudTd>
                        </RowTemplate>
                    </MudTable>
                </MudCardContent>
            </MudCard>
        </MudItem>
    </MudGrid>
}
else
{
    <MudAlert Severity="Severity.Warning">Failed to load dashboard data. Please try refreshing.</MudAlert>
    <MudButton Class="mt-3" Variant="Variant.Filled" Color="Color.Primary" OnClick="@RefreshDashboard">Refresh</MudButton>
}

@code {
    private DashboardStats stats;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadDashboardData();
    }

    private async Task LoadDashboardData()
    {
        try
        {
            isLoading = true;
            var response = await Http.GetAsync("api/admin/dashboard");
            
            if (response.IsSuccessStatusCode)
            {
                stats = await response.Content.ReadFromJsonAsync<DashboardStats>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error loading dashboard: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task RefreshDashboard()
    {
        await LoadDashboardData();
    }

    private Color GetActionColor(string actionType) => actionType switch
    {
        "NewReview" => Color.Success,
        "FlaggedReview" => Color.Error,
        "NewAccount" => Color.Info,
        _ => Color.Default
    };
    
    private void NavigateToManageCollection(string collectionName)
    {
        var tabIndex = collectionName switch
        {
            "Users" => 0,
            "BannedWords" => 1,
            "FlaggedContent" => 2,
            _ => 0
        };
        
        NavigationManager.NavigateTo($"/admin/manage?tab={tabIndex}");
    }
}

```

### Scrum-69: Implement Admin Management Page

Create the admin management page with tabs:

***Client/Pages/Admin/AdminManagement.razor***
```csharp
@page "/admin/manage"
@attribute [Authorize(Roles = "Admin")]
@using CineScope.Shared.Admin
@using CineScope.Shared.DTOs
@using CineScope.Server.Models
@using System.Net.Http.Json
@using CineScope.Client.Components.Admin
@using Microsoft.AspNetCore.WebUtilities
@inject HttpClient Http
@inject ISnackbar Snackbar
@inject IDialogService DialogService
@inject IJSRuntime JSRuntime
@inject NavigationManager NavigationManager

<PageTitle>CineScope - Admin Management</PageTitle>

<AdminBreadcrumb CurrentPage="Management" />

<MudText Typo="Typo.h4" Class="mb-4">Management</MudText>

<MudTabs Elevation="2" Rounded="true" ApplyEffectsToContainer="true" PanelClass="pa-6" @bind-ActivePanelIndex="activeTab">
    <!-- USERS TAB -->
    <MudTabPanel Text="Users" Icon="@Icons.Material.Filled.People">
        <MudText Typo="Typo.h5" Class="mb-4">User Management</MudText>
        
        <MudGrid>
            <MudItem xs="12" sm="6" md="4">
                <MudTextField @bind-Value="userSearchTerm" Label="Search users..." 
                             Adornment="Adornment.End" AdornmentIcon="@Icons.Material.Filled.Search"
                             OnKeyDown="@SearchUsers" />
            </MudItem>
            <MudItem xs="12" sm="6" md="4">
                <MudSelect T="string" Label="Filter by Role" @bind-Value="selectedRole">
                    <MudSelectItem Value="@string.Empty">All Roles</MudSelectItem>
                    <MudSelectItem Value="Admin">Admin</MudSelectItem>
                    <MudSelectItem Value="User">User</MudSelectItem>
                </MudSelect>
            </MudItem>
            <MudItem xs="12" sm="6" md="4">
                <MudSelect T="string" Label="Filter by Status" @bind-Value="selectedStatus">
                    <MudSelectItem Value="@string.Empty">All Statuses</MudSelectItem>
                    <MudSelectItem Value="Active">Active</MudSelectItem>
                    <MudSelectItem Value="Flagged">Flagged</MudSelectItem>
                    <MudSelectItem Value="Suspended">Suspended</MudSelectItem>
                </MudSelect>
            </MudItem>
            <MudItem xs="12">
                <MudButton Variant="Variant.Filled" Color="Color.Primary" 
                          OnClick="@LoadUsers" Class="mt-2">Apply Filters</MudButton>
            </MudItem>
        </MudGrid>
        
        <MudDivider Class="my-4" />
        
        @if (isLoadingUsers)
        {
            <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
        }
        else if (users != null && users.Any())
        {
            <MudTable Items="@users" Hover="true" Breakpoint="Breakpoint.Sm" 
                     Loading="@isLoadingUsers" LoadingProgressColor="Color.Info">
                <HeaderContent>
                    <MudTh>Username</MudTh>
                    <MudTh>Email</MudTh>
                    <MudTh>Join Date</MudTh>
                    <MudTh>Role</MudTh>
                    <MudTh>Status</MudTh>
                    <MudTh>Actions</MudTh>
                </HeaderContent>
                <RowTemplate>
                    <MudTd DataLabel="Username">@context.Username</MudTd>
                    <MudTd DataLabel="Email">@context.Email</MudTd>
                    <MudTd DataLabel="Join Date">@context.JoinDate.ToString("MMM dd, yyyy")</MudTd>
                    <MudTd DataLabel="Role">@string.Join(", ", context.Roles)</MudTd>
                    <MudTd DataLabel="Status">
                        <MudChip Size="Size.Small" Color="@GetStatusColor(context.Status)">@context.Status</MudChip>
                    </MudTd>
                    <MudTd DataLabel="Actions">
                        <MudButton Size="Size.Small" Variant="Variant.Outlined" 
                                  OnClick="@(() => ViewUserDetails(context))">View</MudButton>
                        @if (context.Status != "Suspended")
                        {
                            <MudButton Size="Size.Small" Variant="Variant.Filled" Color="Color.Error"
                                      OnClick="@(() => SuspendUser(context.Id))">Suspend</MudButton>
                        }
                        else
                        {
                            <MudButton Size="Size.Small" Variant="Variant.Filled" Color="Color.Success"
                                      OnClick="@(() => ActivateUser(context.Id))">Activate</MudButton>
                        }
                    </MudTd>
                </RowTemplate>
            </MudTable>
        }
        else
        {
            <MudAlert Severity="Severity.Info">No users found matching the criteria.</MudAlert>
        }
    </MudTabPanel>
    
    <!-- BANNED WORDS TAB -->
    <MudTabPanel Text="Content Filtering" Icon="@Icons.Material.Filled.Security">
        <MudText Typo="Typo.h5" Class="mb-4">Banned Words Management</MudText>
        
        <MudGrid>
            <MudItem xs="12" md="6">
                <MudCard Elevation="2">
                    <MudCardHeader>
                        <CardHeaderContent>
                            <MudText Typo="Typo.h6">Add New Banned Word</MudText>
                        </CardHeaderContent>
                    </MudCardHeader>
                    <MudCardContent>
                        <MudTextField @bind-Value="newBannedWord.Word" Label="Word or phrase" Required="true" />
                        <MudSelect T="string" @bind-Value="newBannedWord.Category" Label="Category" Class="mt-3" Required="true">
                            <MudSelectItem Value="Profanity">Profanity</MudSelectItem>
                            <MudSelectItem Value="Hate Speech">Hate Speech</MudSelectItem>
                            <MudSelectItem Value="Harassment">Harassment</MudSelectItem>
                        </MudSelect>
                        <MudSelect T="int" @bind-Value="newBannedWord.Severity" Label="Severity" Class="mt-3" Required="true">
                            <MudSelectItem Value="1">1 - Low (Warning only)</MudSelectItem>
                            <MudSelectItem Value="3">3 - Medium (Manual review)</MudSelectItem>
                            <MudSelectItem Value="5">5 - High (Auto-reject)</MudSelectItem>
                        </MudSelect>
                    </MudCardContent>
                    <MudCardActions>
                        <MudButton Variant="Variant.Filled" Color="Color.Primary" 
                                  OnClick="@AddBannedWord" Disabled="@(string.IsNullOrEmpty(newBannedWord.Word))">
                            Add Word
                        </MudButton>
                    </MudCardActions>
                </MudCard>
            </MudItem>
            
            <MudItem xs="12" md="6">
                <MudCard Elevation="2">
                    <MudCardHeader>
                        <CardHeaderContent>
                            <MudText Typo="Typo.h6">Filter</MudText>
                        </CardHeaderContent>
                    </MudCardHeader>
                    <MudCardContent>
                        <MudSelect T="string" @bind-Value="bannedWordsCategory" Label="Category" Class="mt-3">
                            <MudSelectItem Value="">All Categories</MudSelectItem>
                            <MudSelectItem Value="Profanity">Profanity</MudSelectItem>
                            <MudSelectItem Value="Hate Speech">Hate Speech</MudSelectItem>
                            <MudSelectItem Value="Harassment">Harassment</MudSelectItem>
                        </MudSelect>
                        <MudSelect T="int?" @bind-Value="bannedWordsSeverity" Label="Severity" Class="mt-3">
                            <MudSelectItem Value="@null">All Severity Levels</MudSelectItem>
                            <MudSelectItem Value="1">1 - Low (Warning only)</MudSelectItem>
                            <MudSelectItem Value="3">3 - Medium (Manual review)</MudSelectItem>
                            <MudSelectItem Value="5">5 - High (Auto-reject)</MudSelectItem>
                        </MudSelect>
                    </MudCardContent>
                    <MudCardActions>
                        <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="@LoadBannedWords">
                            Apply Filters
                        </MudButton>
                    </MudCardActions>
                </MudCard>
            </MudItem>
            
            <MudItem xs="12">
                <MudDivider Class="my-4" />
                
                @if (isLoadingBannedWords)
                {
                    <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
                }
                else if (bannedWords != null && bannedWords.Any())
                {
                    <MudTable Items="@bannedWords" Hover="true" Breakpoint="Breakpoint.Sm"
                             Loading="@isLoadingBannedWords" LoadingProgressColor="Color.Info">
                        <HeaderContent>
                            <MudTh>Banned Word</MudTh>
                            <MudTh>Category</MudTh>
                            <MudTh>Severity</MudTh>
                            <MudTh>Status</MudTh>
                            <MudTh>Actions</MudTh>
                        </HeaderContent>
                        <RowTemplate>
                            <MudTd DataLabel="Banned Word">@context.Word</MudTd>
                            <MudTd DataLabel="Category">@context.Category</MudTd>
                            <MudTd DataLabel="Severity">@context.Severity</MudTd>
                            <MudTd DataLabel="Status">
                                <MudChip Size="Size.Small" Color="@(context.IsActive ? Color.Success : Color.Error)">
                                    @(context.IsActive ? "Active" : "Inactive")
                                </MudChip>
                            </MudTd>
                            <MudTd DataLabel="Actions">
                                <MudButton Size="Size.Small" Variant="Variant.Filled" 
                                          Color="@(context.IsActive ? Color.Error : Color.Success)"
                                          OnClick="@(() => ToggleBannedWordStatus(context.Id, !context.IsActive))">
                                    @(context.IsActive ? "Deactivate" : "Activate")
                                </MudButton>
                            </MudTd>
                        </RowTemplate>
                    </MudTable>
                }
                else
                {
                    <MudAlert Severity="Severity.Info">No banned words found matching the criteria.</MudAlert>
                }
            </MudItem>
        </MudGrid>
    </MudTabPanel>

    <!-- FLAGGED CONTENT TAB -->
    <MudTabPanel Text="Flagged Content" Icon="@Icons.Material.Filled.Report">
        <MudText Typo="Typo.h5" Class="mb-4">Content Moderation</MudText>
        
        @if (isLoadingFlaggedContent)
        {
            <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
        }
        else if (flaggedContent != null && flaggedContent.Any())
        {
            <MudTable Items="@flaggedContent" Hover="true" Breakpoint="Breakpoint.Sm"
                     Loading="@isLoadingFlaggedContent" LoadingProgressColor="Color.Info">
                <HeaderContent>
                    <MudTh>Content Type</MudTh>
                    <MudTh>User</MudTh>
                    <MudTh>Content</MudTh>
                    <MudTh>Reason</MudTh>
                    <MudTh>Date</MudTh>
                    <MudTh>Actions</MudTh>
                </HeaderContent>
                <RowTemplate>
                    <MudTd DataLabel="Content Type">@context.ContentType</MudTd>
                    <MudTd DataLabel="User">@context.Username</MudTd>
                    <MudTd DataLabel="Content">@context.MovieTitle</MudTd>
                    <MudTd DataLabel="Reason">@context.Reason</MudTd>
                    <MudTd DataLabel="Date">@context.FlaggedAt.ToString("MMM dd, yyyy")</MudTd>
                    <MudTd DataLabel="Actions">
                        <MudButton Size="Size.Small" Variant="Variant.Outlined" Color="Color.Primary"
                                  OnClick="@(() => ViewFlaggedContent(context))">
                            View
                        </MudButton>
                        <MudButton Size="Size.Small" Variant="Variant.Filled" Color="Color.Success"
                                  OnClick="@(() => ApproveContent(context.Id))">
                            Approve
                        </MudButton>
                        <MudButton Size="Size.Small" Variant="Variant.Filled" Color="Color.Error"
                                  OnClick="@(() => RejectContent(context.Id))">
                            Reject
                        </MudButton>
                    </MudTd>
                </RowTemplate>
            </MudTable>
        }
        else
        {
            <MudAlert Severity="Severity.Success">No flagged content requiring moderation.</MudAlert>
        }
    </MudTabPanel>

    <!-- DATA MANAGEMENT TAB -->
    <MudTabPanel Text="Database" Icon="@Icons.Material.Filled.Storage">
        <MudText Typo="Typo.h5" Class="mb-4">MongoDB Database Management</MudText>
        
        <MudGrid>
            <MudItem xs="12" md="6">
                <MudCard Elevation="2">
                    <MudCardHeader>
                        <CardHeaderContent>
                            <MudText Typo="Typo.h6">Export Collection</MudText>
                        </CardHeaderContent>
                    </MudCardHeader>
                    <MudCardContent>
                        <MudSelect T="string" @bind-Value="selectedExportCollection" Label="Select Collection">
                            <MudSelectItem Value="Users">Users</MudSelectItem>
                            <MudSelectItem Value="Movies">Movies</MudSelectItem>
                            <MudSelectItem Value="Reviews">Reviews</MudSelectItem>
                            <MudSelectItem Value="BannedWords">Banned Words</MudSelectItem>
                        </MudSelect>
                    </MudCardContent>
                    <MudCardActions>
                        <MudButton Variant="Variant.Filled" Color="Color.Primary" 
                                  OnClick="@ExportCollection" 
                                  Disabled="@string.IsNullOrEmpty(selectedExportCollection)">
                            Export
                        </MudButton>
                    </MudCardActions>
                </MudCard>
            </MudItem>
            
            <MudItem xs="12" md="6">
                <MudCard Elevation="2">
                    <MudCardHeader>
                        <CardHeaderContent>
                            <MudText Typo="Typo.h6">MongoDB Status</MudText>
                        </CardHeaderContent>
                    </MudCardHeader>
                    <MudCardContent>
                        <MudAlert Severity="Severity.Info" Class="mb-3">
                            MongoDB connection is configured to:
                            <code>mongodb+srv://username:***@cluster.mongodb.net/CineScopeDb</code>
                        </MudAlert>
                        <MudButton Variant="Variant.Filled" Color="Color.Primary" 
                                  OnClick="@RefreshCollectionStats">
                            Refresh Collection Stats
                        </MudButton>
                    </MudCardContent>
                </MudCard>
            </MudItem>

            <MudItem xs="12" Class="mt-4">
                <MudText Typo="Typo.h6">Collection Statistics</MudText>
                
                @if (isLoadingCollectionStats)
                {
                    <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
                }
                else if (collectionStats != null && collectionStats.Any())
                {
                    <MudSimpleTable Hover="true" FixedHeader="true">
                        <thead>
                            <tr>
                                <th>Collection</th>
                                <th>Documents</th>
                            </tr>
                        </thead>
                        <tbody>
                            @foreach (var stat in collectionStats)
                            {
                                <tr>
                                    <td>@stat.Key</td>
                                    <td>@stat.Value</td>
                                </tr>
                            }
                        </tbody>
                    </MudSimpleTable>
                }
            </MudItem>
            
            <MudItem xs="12" Class="mt-6">
                <MudAlert Severity="Severity.Warning">
                    <MudText>Warning: Database operations can affect data integrity. Always backup your data before making changes.</MudText>
                </MudAlert>
            </MudItem>
        </MudGrid>
    </MudTabPanel>
</MudTabs>

@code {
    // Tab index
    private int activeTab = 0;

    // Users tab
    private List<UserAdminDto> users;
    private bool isLoadingUsers = true;
    private string userSearchTerm = string.Empty;
    private string selectedRole = string.Empty;
    private string selectedStatus = string.Empty;

    // Banned words tab
    private List<BannedWord> bannedWords;
    private bool isLoadingBannedWords = true;
    private string bannedWordsCategory = string.Empty;
    private int? bannedWordsSeverity = null;
    
    // New banned word form
    private BannedWord newBannedWord = new BannedWord
    {
        Word = string.Empty,
        Category = "Profanity",
        Severity = 3,
        IsActive = true
    };

    // Flagged content tab
    private List<FlaggedContentDto> flaggedContent;
    private bool isLoadingFlaggedContent = true;

    // Database tab
    private string selectedExportCollection;
    private Dictionary<string, long> collectionStats;
    private bool isLoadingCollectionStats = true;

    protected override async Task OnInitializedAsync()
    {
        // Parse query string parameters for active tab
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("tab", out var tabValue) && 
            int.TryParse(tabValue, out var tabIndex))
        {
            activeTab = tabIndex;
        }

        await LoadData();
    }

    private async Task LoadData()
    {
        switch (activeTab)
        {
            case 0:
                await LoadUsers();
                break;
            case 1:
                await LoadBannedWords();
                break;
            case 2:
                await LoadFlaggedContent();
                break;
            case 3:
                await LoadCollectionStats();
                break;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        // Load data for all tabs
        await LoadUsers();
        await LoadBannedWords();
        await LoadFlaggedContent();
        await LoadCollectionStats();
    }

    private async Task LoadUsers()
    {
        try
        {
            isLoadingUsers = true;
            
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(userSearchTerm))
                queryParams.Add($"search={Uri.EscapeDataString(userSearchTerm)}");
            if (!string.IsNullOrEmpty(selectedRole))
                queryParams.Add($"role={Uri.EscapeDataString(selectedRole)}");
            if (!string.IsNullOrEmpty(selectedStatus))
                queryParams.Add($"status={Uri.EscapeDataString(selectedStatus)}");
                
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            
            var response = await Http.GetAsync($"api/admin/users{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                users = await response.Content.ReadFromJsonAsync<List<UserAdminDto>>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error loading users: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoadingUsers = false;
        }
    }

    private async Task LoadBannedWords()
    {
        try
        {
            isLoadingBannedWords = true;
            
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(bannedWordsCategory))
                queryParams.Add($"category={Uri.EscapeDataString(bannedWordsCategory)}");
            if (bannedWordsSeverity.HasValue)
                queryParams.Add($"severity={bannedWordsSeverity.Value}");
                
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            
            var response = await Http.GetAsync($"api/admin/banned-words{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                bannedWords = await response.Content.ReadFromJsonAsync<List<BannedWord>>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error loading banned words: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoadingBannedWords = false;
        }
    }

    private async Task LoadFlaggedContent()
    {
        try
        {
            isLoadingFlaggedContent = true;
            
            var response = await Http.GetAsync("api/admin/flagged-content");
            
            if (response.IsSuccessStatusCode)
            {
                flaggedContent = await response.Content.ReadFromJsonAsync<List<FlaggedContentDto>>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error loading flagged content: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoadingFlaggedContent = false;
        }
    }

    private async Task LoadCollectionStats()
    {
        try
        {
            isLoadingCollectionStats = true;
            
            var response = await Http.GetAsync("api/admin/collection-stats");
            
            if (response.IsSuccessStatusCode)
            {
                collectionStats = await response.Content.ReadFromJsonAsync<Dictionary<string, long>>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error loading collection stats: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoadingCollectionStats = false;
        }
    }

    private async Task SearchUsers(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await LoadUsers();
        }
    }

    private void ViewUserDetails(UserAdminDto user)
    {
        // Implement user details view (e.g., dialog)
        Snackbar.Add($"Viewing details for user: {user.Username}", Severity.Info);
    }

    private async Task SuspendUser(string userId)
    {
        try
        {
            // Show confirmation dialog
            var parameters = new DialogParameters
            {
                { "ContentText", "Are you sure you want to suspend this user? They will not be able to log in until reactivated." },
                { "ButtonText", "Suspend User" },
                { "Color", Color.Error }
            };
            
            var options = new DialogOptions() { CloseOnEscapeKey = true };
            var dialog = await DialogService.Show<MudConfirmationDialog>("Confirm Suspension", parameters, options).Result;
            
            if (dialog.Cancelled)
                return;
                
            var response = await Http.PutAsJsonAsync($"api/admin/users/{userId}/status", "Suspended");
            
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("User suspended successfully", Severity.Success);
                await LoadUsers();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error suspending user: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task ActivateUser(string userId)
    {
        try
        {
            var response = await Http.PutAsJsonAsync($"api/admin/users/{userId}/status", "Active");
            
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("User activated successfully", Severity.Success);
                await LoadUsers();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error activating user: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task AddBannedWord()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(newBannedWord.Word))
            {
                Snackbar.Add("Word cannot be empty", Severity.Warning);
                return;
            }
            
            var response = await Http.PostAsJsonAsync("api/admin/banned-words", newBannedWord);
            
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Banned word added successfully", Severity.Success);
                newBannedWord = new BannedWord
                {
                    Word = string.Empty,
                    Category = "Profanity",
                    Severity = 3,
                    IsActive = true
                };
                await LoadBannedWords();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error adding banned word: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task ToggleBannedWordStatus(string wordId, bool isActive)
    {
        try
        {
            var response = await Http.PutAsJsonAsync($"api/admin/banned-words/{wordId}/status", isActive);
            
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add($"Banned word {(isActive ? "activated" : "deactivated")} successfully", Severity.Success);
                await LoadBannedWords();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error updating banned word status: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private void ViewFlaggedContent(FlaggedContentDto content)
    {
        // Implement content view (e.g., dialog with content details)
        Snackbar.Add($"Viewing flagged content by {content.Username}", Severity.Info);
    }

    private async Task ApproveContent(string contentId)
    {
        try
        {
            var action = new ModerationAction
            {
                ActionType = "Approve",
                Reason = "Content manually approved by admin"
            };
            
            var response = await Http.PostAsJsonAsync($"api/admin/moderate/{contentId}", action);
            
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Content approved successfully", Severity.Success);
                await LoadFlaggedContent();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error approving content: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task RejectContent(string contentId)
    {
        try
        {
            // Show confirmation dialog
            var parameters = new DialogParameters
            {
                { "ContentText", "Are you sure you want to reject and remove this content? This action cannot be undone." },
                { "ButtonText", "Reject Content" },
                { "Color", Color.Error }
            };
            
            var options = new DialogOptions() { CloseOnEscapeKey = true };
            var dialog = await DialogService.Show<MudConfirmationDialog>("Confirm Rejection", parameters, options).Result;
            
            if (dialog.Cancelled)
                return;
                
            var action = new ModerationAction
            {
                ActionType = "Reject",
                Reason = "Content rejected by admin"
            };
            
            var response = await Http.PostAsJsonAsync($"api/admin/moderate/{contentId}", action);
            
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Content rejected and removed successfully", Severity.Success);
                await LoadFlaggedContent();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error rejecting content: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task ExportCollection()
    {
        try
        {
            if (string.IsNullOrEmpty(selectedExportCollection))
                return;
                
            // Use JSRuntime to trigger file download via API
            var url = $"/api/admin/export/{selectedExportCollection}";
            await JSRuntime.InvokeVoidAsync("window.open", url, "_blank");
            Snackbar.Add($"Exporting {selectedExportCollection}...", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task RefreshCollectionStats()
    {
        await LoadCollectionStats();
    }

    private Color GetStatusColor(string status) => status switch
    {
        "Active" => Color.Success,
        "Flagged" => Color.Warning,
        "Suspended" => Color.Error,
        _ => Color.Default
    };
}
```

### Scrum-71: Update Navigation and Configure Services

Update the navigation to include the admin link:

Source/CineScope/Client/Shared/Nav/NavMenu.razor (Find the appropriate location to add this section)
```csharp
<AuthorizeView Roles="Admin">
    <MudNavLink Href="/admin" Match="NavLinkMatch.Prefix" Icon="@Icons.Material.Filled.AdminPanelSettings">
        Admin Dashboard
    </MudNavLink>
</AuthorizeView>
```

Register the services in Program.cs:
Source/CineScope/Server/Program.cs (Add to the existing services registration section)
```csharp
// Register admin and data seed services
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<DataSeedService>();
```

Update the App.razor to handle admin routes:
Source/CineScope/Client/App.razor (Update the routing section)
```csharp
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            @{
                var layout = routeData.PageType.Name.StartsWith("Admin") || 
                             routeData.PageType.Namespace?.Contains("Admin") == true
                             ? typeof(AdminLayout) 
                             : typeof(AuthenticationLayout);
            }
            <RouteView RouteData="@routeData" DefaultLayout="@layout" />
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
        <NotFound>
            <!-- Not found content... -->
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```
Initialize MongoDB data at application startup:
File Path: Source/CineScope/Server/Program.cs (Add before app.Run())
```csharp
// Seed data and create indexes
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<DataSeedService>();
    await seedService.SeedInitialDataAsync();
}
```
