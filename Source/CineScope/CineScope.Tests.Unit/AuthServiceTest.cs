using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Server.Services;
using CineScope.Shared.Auth;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CineScope.Shared.DTOs;

namespace CineScope.Tests.Unit
{
    /// <summary>
    /// Contains unit tests for the AuthService component.
    /// These tests verify the authentication functionality including login validation,
    /// registration, account lockout, and token generation.
    /// </summary>
    public class AuthServiceTests
    {
        /// <summary>
        /// Tests successful authentication with valid credentials.
        /// 
        /// This test verifies that when a user provides valid credentials,
        /// the system authenticates them and returns an appropriate response
        /// with an auth token and user details.
        /// </summary>
        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnSuccessResponse()
        {
            // Arrange - Set up test data and dependencies
            // Define login credentials
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "Test@123"
            };

            // Create a sample user with hashed password
            var user = new User
            {
                Id = "user123",
                Username = "testuser",
                Email = "testuser@cinescope.test",
                // BCrypt hash for "Test@123"
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                Roles = new List<string> { "User" },
                FailedLoginAttempts = 0,
                IsLocked = false
            };

            // Mock the MongoDB collection for user queries
            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockCursor = new Mock<IAsyncCursor<User>>();

            // Configure the cursor to return our test user
            mockCursor.Setup(c => c.Current).Returns(new List<User> { user });
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)   // First call returns true (has results)
                .ReturnsAsync(false); // Second call returns false (no more results)

            // Configure the collection's FindAsync method to return our cursor
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Configure the collection's UpdateOneAsync method for updating the last login time
            mockCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            // Mock the database service to return our collection
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock configuration with JWT settings
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:Secret", "very_long_secret_key_for_testing_purposes_at_least_32_characters"},
                {"JwtSettings:Issuer", "CineScope"},
                {"JwtSettings:Audience", "CineScopeUsers"},
                {"JwtSettings:ExpiryMinutes", "60"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Mock the settings to provide the collection name
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                UsersCollectionName = "Users"
            });

            // Create the service with our mocked dependencies
            var authService = new AuthService(mockMongoDbService.Object, mockSettings.Object, configuration);

            // Act - Call the method being tested
            var result = await authService.LoginAsync(loginRequest);

            // Assert - Verify the results match our expectations
            Assert.True(result.Success);                         // Login should be successful
            Assert.NotEmpty(result.Token);                       // Token should be generated
            Assert.Equal("testuser", result.User.Username);      // Username should match
            Assert.Contains("Login successful", result.Message); // Message should indicate success
        }

        /// <summary>
        /// Tests failed authentication with invalid credentials.
        /// 
        /// This test verifies that when a user provides invalid credentials,
        /// the system rejects them and increments the failed login attempts counter.
        /// </summary>
        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnFailureResponse()
        {
            // Arrange - Set up test data and dependencies
            // Define login credentials with incorrect password
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "WrongPassword"
            };

            // Create a sample user with hashed password
            var user = new User
            {
                Id = "user123",
                Username = "testuser",
                Email = "testuser@cinescope.test",
                // BCrypt hash for "Test@123" (not matching the login request)
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                Roles = new List<string> { "User" },
                FailedLoginAttempts = 0,
                IsLocked = false
            };

            // Mock the MongoDB collection for user queries
            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockCursor = new Mock<IAsyncCursor<User>>();

            // Configure the cursor to return our test user
            mockCursor.Setup(c => c.Current).Returns(new List<User> { user });
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)   // First call returns true (has results)
                .ReturnsAsync(false); // Second call returns false (no more results)

            // Configure the collection's FindAsync method to return our cursor
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Configure the collection's UpdateOneAsync method for updating failed attempts
            mockCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            // Mock the database service to return our collection
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock configuration with JWT settings
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:Secret", "very_long_secret_key_for_testing_purposes_at_least_32_characters"},
                {"JwtSettings:Issuer", "CineScope"},
                {"JwtSettings:Audience", "CineScopeUsers"},
                {"JwtSettings:ExpiryMinutes", "60"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Mock the settings to provide the collection name
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                UsersCollectionName = "Users"
            });

            // Create the service with our mocked dependencies
            var authService = new AuthService(mockMongoDbService.Object, mockSettings.Object, configuration);

            // Act - Call the method being tested
            var result = await authService.LoginAsync(loginRequest);

            // Assert - Verify the results match our expectations
            Assert.False(result.Success);                         // Login should fail
            Assert.Empty(result.Token);                           // No token should be generated
            Assert.Equal(new UserDto().Username, result.User.Username); // User should be empty/default
            Assert.Contains("Invalid username or password", result.Message); // Message should indicate failure
        }

        /// <summary>
        /// Tests account lockout after multiple failed login attempts.
        /// 
        /// This test verifies that when a user has multiple failed login attempts,
        /// the account is locked after reaching the threshold (3 attempts).
        /// </summary>
        [Fact]
        public async Task Login_WithThreeFailedAttempts_ShouldLockAccount()
        {
            // Arrange - Set up test data and dependencies
            // Define login credentials with incorrect password
            var loginRequest = new LoginRequest
            {
                Username = "lockuser",
                Password = "WrongPassword"
            };

            // Create a sample user with 2 previous failed attempts
            var user = new User
            {
                Id = "user456",
                Username = "lockuser",
                Email = "lockuser@cinescope.test",
                // BCrypt hash for "Test@123" (not matching the login request)
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                Roles = new List<string> { "User" },
                FailedLoginAttempts = 2, // Already has 2 failed attempts
                IsLocked = false
            };

            // Mock the MongoDB collection and cursor for user data
            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockCursor = new Mock<IAsyncCursor<User>>();

            // Configure the cursor to return our test user
            mockCursor.Setup(c => c.Current).Returns(new List<User> { user });
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)   // First call returns true (has results)
                .ReturnsAsync(false); // Second call returns false (no more results)

            // Configure collection's FindAsync to return our cursor
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Track the update operations to verify account locking
            UpdateDefinition<User> capturedUpdate = null;

            // Configure UpdateOneAsync to capture the update operation
            mockCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<FilterDefinition<User>, UpdateDefinition<User>, UpdateOptions, CancellationToken>(
                    (filter, update, options, token) => capturedUpdate = update)
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            // Mock the database service
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock configuration with JWT settings
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:Secret", "very_long_secret_key_for_testing_purposes_at_least_32_characters"},
                {"JwtSettings:Issuer", "CineScope"},
                {"JwtSettings:Audience", "CineScopeUsers"},
                {"JwtSettings:ExpiryMinutes", "60"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Mock MongoDB settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                UsersCollectionName = "Users"
            });

            // Create auth service with mocked dependencies
            var authService = new AuthService(mockMongoDbService.Object, mockSettings.Object, configuration);

            // Act - Call the login method
            var result = await authService.LoginAsync(loginRequest);

            // Assert - Verify the results
            Assert.False(result.Success);                         // Login should fail
            Assert.Empty(result.Token);                           // No token should be generated
            Assert.Contains("account has been locked", result.Message.ToLower()); // Message should indicate account locked

            // Note: In a more complete test, we would also verify that the update operation
            // set FailedLoginAttempts = 3 and IsLocked = true, but that would require more
            // complex testing setup to inspect the captured update definition.
        }

        /// <summary>
        /// Tests that login is rejected for already locked accounts.
        /// 
        /// This test verifies that when a user attempts to log in to a locked account,
        /// the system immediately rejects the attempt regardless of password correctness.
        /// </summary>
        [Fact]
        public async Task Login_WithLockedAccount_ShouldRejectLogin()
        {
            // Arrange - Set up test data and dependencies
            // Define login credentials
            var loginRequest = new LoginRequest
            {
                Username = "lockeduser",
                Password = "Test@123" // Correct password, but account is locked
            };

            // Create a sample user with a locked account
            var user = new User
            {
                Id = "user789",
                Username = "lockeduser",
                Email = "lockeduser@cinescope.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                Roles = new List<string> { "User" },
                FailedLoginAttempts = 3,
                IsLocked = true // Account is already locked
            };

            // Mock the MongoDB collection and cursor
            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockCursor = new Mock<IAsyncCursor<User>>();

            // Configure the cursor to return our locked user
            mockCursor.Setup(c => c.Current).Returns(new List<User> { user });
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            // Configure collection's FindAsync
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Mock the database service
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock configuration
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:Secret", "very_long_secret_key_for_testing_purposes_at_least_32_characters"},
                {"JwtSettings:Issuer", "CineScope"},
                {"JwtSettings:Audience", "CineScopeUsers"},
                {"JwtSettings:ExpiryMinutes", "60"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Mock MongoDB settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                UsersCollectionName = "Users"
            });

            // Create auth service
            var authService = new AuthService(mockMongoDbService.Object, mockSettings.Object, configuration);

            // Act - Call the login method
            var result = await authService.LoginAsync(loginRequest);

            // Assert - Verify the results
            Assert.False(result.Success);                         // Login should fail
            Assert.Empty(result.Token);                           // No token should be generated
            Assert.Contains("account is locked", result.Message); // Message should indicate locked account
        }

        /// <summary>
        /// Tests successful user registration with valid data.
        /// 
        /// This test verifies that when a user provides valid registration information,
        /// the system creates a new user account and returns a success response.
        /// </summary>
        [Fact]
        public async Task Register_WithValidData_ShouldCreateUserAndReturnSuccess()
        {
            // Arrange - Set up test data and dependencies
            // Define registration request
            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                Email = "newuser@cinescope.test",
                Password = "NewPass@123",
                ConfirmPassword = "NewPass@123"
            };

            // Mock the MongoDB collection for checking existing users
            var mockCollection = new Mock<IMongoCollection<User>>();
            var emptyMockCursor = new Mock<IAsyncCursor<User>>();

            // Configure the cursor to return no existing users (username/email not taken)
            emptyMockCursor.Setup(c => c.Current).Returns(new List<User>());
            emptyMockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Configure collection's FindAsync for username check
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyMockCursor.Object);

            // Configure collection's InsertOneAsync for user creation
            mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<User>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Mock the database service
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock configuration
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:Secret", "very_long_secret_key_for_testing_purposes_at_least_32_characters"},
                {"JwtSettings:Issuer", "CineScope"},
                {"JwtSettings:Audience", "CineScopeUsers"},
                {"JwtSettings:ExpiryMinutes", "60"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Mock MongoDB settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                UsersCollectionName = "Users"
            });

            // Create auth service
            var authService = new AuthService(mockMongoDbService.Object, mockSettings.Object, configuration);

            // Act - Call the register method
            var result = await authService.RegisterAsync(registerRequest);

            // Assert - Verify the results
            Assert.True(result.Success);                           // Registration should succeed
            Assert.Contains("Registration successful", result.Message); // Message should indicate success
            Assert.NotEmpty(result.Token);                         // Token should be generated
            Assert.Equal("newuser", result.User.Username);         // Username should match
        }

        /// <summary>
        /// Tests registration rejection when username is already taken.
        /// 
        /// This test verifies that the system prevents duplicate usernames
        /// by rejecting registration attempts with existing usernames.
        /// </summary>
        [Fact]
        public async Task Register_WithExistingUsername_ShouldReturnFailure()
        {
            // Arrange - Set up test data and dependencies
            // Define registration request with existing username
            var registerRequest = new RegisterRequest
            {
                Username = "existinguser",
                Email = "newemail@cinescope.test",
                Password = "NewPass@123",
                ConfirmPassword = "NewPass@123"
            };

            // Create a sample existing user
            var existingUser = new User
            {
                Id = "user101",
                Username = "existinguser", // Same username as in request
                Email = "existing@cinescope.test",
                PasswordHash = "hashedpassword",
                Roles = new List<string> { "User" }
            };

            // Mock the MongoDB collection and cursor
            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockCursor = new Mock<IAsyncCursor<User>>();

            // Configure the cursor to return the existing user
            mockCursor.Setup(c => c.Current).Returns(new List<User> { existingUser });
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            // Configure collection's FindAsync
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Mock the database service
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock configuration
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:Secret", "very_long_secret_key_for_testing_purposes_at_least_32_characters"},
                {"JwtSettings:Issuer", "CineScope"},
                {"JwtSettings:Audience", "CineScopeUsers"},
                {"JwtSettings:ExpiryMinutes", "60"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Mock MongoDB settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                UsersCollectionName = "Users"
            });

            // Create auth service
            var authService = new AuthService(mockMongoDbService.Object, mockSettings.Object, configuration);

            // Act - Call the register method
            var result = await authService.RegisterAsync(registerRequest);

            // Assert - Verify the results
            Assert.False(result.Success);                        // Registration should fail
            Assert.Contains("Username already exists", result.Message); // Message should indicate duplicate username
            Assert.Empty(result.Token);                          // No token should be generated
        }
    }
}