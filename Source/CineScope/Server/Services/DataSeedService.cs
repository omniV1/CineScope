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
                    // Add more banned words as needed
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