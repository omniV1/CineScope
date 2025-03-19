# CineScope Admin Interface Implementation Guide

This comprehensive guide walks you through implementing a complete admin interface for the CineScope application with MongoDB integration. By following these steps, your team will be able to create an intuitive admin dashboard with content moderation, user management, and database functionality.

## Overview

The admin interface will consist of:
- A dashboard with key statistics
- User management functionality
- Content moderation tools
- Banned word management
- Database management with export capabilities
- MongoDB integration throughout

## Prerequisites

- CineScope codebase cloned and running
- MongoDB connection configured in appsettings.json
- Familiarity with C#, Blazor, and MudBlazor components

## Step 1: Add admin authorization.



. **Add admin authorization policy** to `Program.cs`:

```csharp
// In Program.cs, add before the existing auth setup
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
});
```

## Step 2:Step 2: Create Admin DTOs in the Shared Project

Create a new folder in your Shared project.

**Create file:** `Source/CineScope/Shared/Admin/AdminDtos.cs`
```csharp

using CineScope.Shared.DTOs;
using System;
using System.Collections.Generic;

namespace CineScope.Shared.Admin
{
    /// <summary>
    /// DTO for admin dashboard statistics.
    /// </summary>
    public class DashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalMovies { get; set; }
        public int TotalReviews { get; set; }
        public int FlaggedContent { get; set; }
        public List<RecentActivityDto> RecentActivity { get; set; } = new();
        public Dictionary<string, long> CollectionStats { get; set; } = new();
    }

    /// <summary>
    /// DTO for recent user activity.
    /// </summary>
    public class RecentActivityDto
    {
        public DateTime Timestamp { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // "NewReview", "FlaggedReview", etc.
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for flagged content requiring moderation.
    /// </summary>
    public class FlaggedContentDto
    {
        public string Id { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty; // "Review", "User", etc.
        public string Username { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime FlaggedAt { get; set; }
    }

    /// <summary>
    /// Extended user DTO with admin-specific properties.
    /// </summary>
    public class UserAdminDto : UserDto
    {
        public DateTime JoinDate { get; set; }
        public int ReviewCount { get; set; }
        public DateTime? LastLogin { get; set; }
        public string Status { get; set; } = "Active"; // "Active", "Flagged", "Suspended"
    }

    /// <summary>
    /// DTO for moderation actions.
    /// </summary>
    public class ModerationAction
    {
        public string ActionType { get; set; } = string.Empty; // "Approve", "Reject", "Modify"
        public string Reason { get; set; } = string.Empty;
        public string ModifiedContent { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for filter parameters on the user management page.
    /// </summary>
    public class UserFilterParams
    {
        public string SearchTerm { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// DTO for filter parameters on the banned words page.
    /// </summary>
    public class BannedWordFilterParams
    {
        public string Category { get; set; } = string.Empty;
        public int? Severity { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// DTO for user status update.
    /// </summary>
    public class UserStatusUpdateDto
    {
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for import/export operation results.
    /// </summary>
    public class DataOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RecordsAffected { get; set; }
        public string CollectionName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
```

Now, let's create the server-side MongoDB models for admin functionality:

Create file: Source/CineScope/Server/Models/Admin/DashboardStats.cs
```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
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
]
```

Create file: Source/CineScope/Server/Models/Admin/ModerationRecord.cs
```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace CineScope.Server.Models.Admin
{
    public class ModerationAction
    {
        public string ActionType { get; set; } = string.Empty; // "Approve", "Reject", "Modify"
        public string Reason { get; set; } = string.Empty;
        public string ModifiedContent { get; set; } = string.Empty;
    }
}
```



## Step 3: Create Admin Service with MongoDB Integration

This service will handle all admin-specific operations using MongoDB.

**Create file:** `Source/CineScope/Server/Services/AdminService.cs`

```csharp
// Source/CineScope/Server/Services/AdminService.cs
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
        /// Gets all banned words.
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
    }
}
```

## Step 4: Create Data Seeding Service for MongoDB

This service will initialize the MongoDB database with sample data and indexes.

**Create file:** `Source/CineScope/Server/Services/DataSeedService.cs`

```csharp
// Source/CineScope/Server/Services/DataSeedService.cs
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CineScope.Server.Services
{
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

        public async Task SeedInitialDataAsync()
        {
            await SeedAdminUserAsync();
            await SeedBannedWordsAsync();
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

                var bannedWords = new List<BannedWord>
                {
                    new BannedWord { 
                        Word = "profanity1", 
                        Severity = 3, 
                        Category = "Profanity", 
                        IsActive = true 
                    },
                    new BannedWord { 
                        Word = "profanity2", 
                        Severity = 2, 
                        Category = "Profanity", 
                        IsActive = true 
                    },
                    new BannedWord { 
                        Word = "hatespeech1", 
                        Severity = 5, 
                        Category = "Hate Speech", 
                        IsActive = true 
                    },
                    new BannedWord { 
                        Word = "harassment1", 
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
        /// Creates MongoDB indexes for better query performance.
        /// </summary>
        private async Task CreateDatabaseIndexesAsync()
        {
            try
            {
                // Users collection indexes
                var usersCollection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);
                await usersCollection.Indexes.CreateOneAsync(
                    new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Username), 
                    new CreateIndexOptions { Unique = true }));
                await usersCollection.Indexes.CreateOneAsync(
                    new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Email), 
                    new CreateIndexOptions { Unique = true }));
                
                // Reviews collection indexes
                var reviewsCollection = _mongoDbService.GetCollection<Review>(_settings.ReviewsCollectionName);
                await reviewsCollection.Indexes.CreateOneAsync(
                    Builders<Review>.IndexKeys.Ascending(r => r.MovieId));
                await reviewsCollection.Indexes.CreateOneAsync(
                    Builders<Review>.IndexKeys.Ascending(r => r.UserId));
                
                // Movies collection indexes
                var moviesCollection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);
                await moviesCollection.Indexes.CreateOneAsync(
                    Builders<Movie>.IndexKeys.Text(m => m.Title));
                
                _logger.LogInformation("Created MongoDB indexes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MongoDB indexes");
            }
        }
    }
}
```

## Step 5: Create Admin API Controller

Create a controller for handling admin API requests.

**Create file:** `Source/CineScope/Server/Controllers/AdminController.cs`

```csharp
// Source/CineScope/Server/Controllers/AdminController.cs
using CineScope.Server.Models;
using CineScope.Server.Services;
using CineScope.Shared.Admin;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CineScope.Server.Controllers
{
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

                // Get the collection data from the admin service
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

## Step 6: Create Admin UI Components

Start with creating some shared components for the admin UI.

### Step 6.1: Create Admin Breadcrumb Component

**Create file:** `Source/CineScope/Client/Components/Admin/AdminBreadcrumb.razor`

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

### Step 6.2: Create Confirmation Dialog Component

**Create file:** `Source/CineScope/Client/Components/Admin/MudConfirmationDialog.razor`

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

## Step 7: Create Admin Layout

Now create a custom layout for admin pages.

**Create file:** `Source/CineScope/Client/Shared/AdminLayout.razor`

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

## Step 8: Create Admin Dashboard Page

Create the main admin dashboard page.

**Create file:** `Source/CineScope/Client/Pages/Admin/AdminDashboard.razor`

```csharp
// Source/CineScope/Client/Pages/Admin/AdminDashboard.razor
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

## Step 9: Create Admin Management Page

Create the multi-tabbed admin management page.

**Create file:** `Source/CineScope/Client/Pages/Admin/AdminManagement.razor`

```csharp
// Source/CineScope/Client/Pages/Admin/AdminManagement.razor

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
```

## Step 10: Update App.razor for Admin Layout

Update `App.razor` to use the admin layout for admin pages:

```csharp
@using Microsoft.AspNetCore.Components.Routing
@using MudBlazor
@using CineScope.Client.Shared

<!-- MudBlazor providers... -->

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

## Step 11: Add Admin Link to the Main Navigation

Update the navigation menu to include an admin link:

**Edit file:** `Source/CineScope/Client/Shared/Nav/NavMenu.razor`

Add this section to your existing navigation menu items:

```csharp
<AuthorizeView Roles="Admin">
    <MudNavLink Href="/admin" Match="NavLinkMatch.Prefix" Icon="@Icons.Material.Filled.AdminPanelSettings">
        Admin Dashboard
    </MudNavLink>
</AuthorizeView>
```

## Step 12: Register Services and Initialize Data

Update `Program.cs` to register the services and initialize the database:

**Edit file:** `Source/CineScope/Server/Program.cs`

Add to the services section:

```csharp
// Register admin and data seed services
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<DataSeedService>();
```

Add before `app.Run()`:

```csharp
// Seed data and create indexes
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<DataSeedService>();
    await seedService.SeedInitialDataAsync();
}
```

## Step 13: Testing the Admin Interface

1. Start your application and navigate to the homepage
2. Log in as the admin user (username: AdminUser, password: Admin123!)
3. You should now see the admin link in the navigation menu
4. Click the admin link to access the admin dashboard
5. Test all the functionality in the dashboard and management pages
