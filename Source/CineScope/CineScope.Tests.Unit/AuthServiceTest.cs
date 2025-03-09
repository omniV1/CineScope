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

namespace CineScope.Tests.Unit
{
    /// <summary>
    /// Contains unit tests for the AuthService component.
    /// These tests verify the authentication functionality including login validation,
    /// registration, account lockout, and token generation.
    /// </summary>
    public class AuthServiceTests
    {
        // Test helper method to create a mock configuration with JWT settings
        private IConfiguration CreateMockConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:Secret", "very_long_secret_key_for_testing_purposes_at_least_32_characters"},
                {"JwtSettings:Issuer", "CineScope"},
                {"JwtSettings:Audience", "CineScopeUsers"},
                {"JwtSettings:ExpiryMinutes", "60"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        /// <summary>
        /// Tests successful authentication with valid credentials.
        /// </summary>
        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnSuccessResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "Test@123"
            };

            var user = new User
            {
                Id = "user123",
                Username = "testuser",
                Email = "testuser@cinescope.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                Roles = new List<string> { "User" },
                FailedLoginAttempts = 0,
                IsLocked = false
            };

            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockCursor = new Mock<IAsyncCursor<User>>();

            mockCursor.Setup(c => c.Current).Returns(new List<User> { user });
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            mockCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                UsersCollectionName = "Users"
            });

            var configuration = CreateMockConfiguration();

            var authService = new AuthService(mockMongoDbService.Object, mockSettings.Object, configuration);

            // Act
            var result = await authService.LoginAsync(loginRequest);

            // Assert
            Assert.True(result.Success);
            Assert.NotEmpty(result.Token);
            Assert.Equal("testuser", result.User.Username);
            Assert.Contains("Login successful", result.Message);
        }

        /// <summary>
        /// Tests failed authentication with invalid credentials.
        /// </summary>
        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnFailureResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "WrongPassword"
            };

            var user = new User
            {
                Id = "user123",
                Username = "testuser",
                Email = "testuser@cinescope.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                Roles = new List<string> { "User" },
                FailedLoginAttempts = 0,
                IsLocked = false
            };

            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockCursor = new Mock<IAsyncCursor<User>>();

            mockCursor.Setup(c => c.Current).Returns(new List<User> { user });
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            mockCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                UsersCollectionName = "Users"
            });

            var configuration = CreateMockConfiguration();

            var authService = new AuthService(mockMongoDbService.Object, mockSettings.Object, configuration);

            // Act
            var result = await authService.LoginAsync(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Empty(result.Token);
            Assert.Contains("Invalid username or password", result.Message);
        }

        /// <summary>
        /// Tests account lockout after multiple failed login attempts.
        /// </summary>
        [Fact]
        public async Task Login_WithThreeFailedAttempts_ShouldLockAccount()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "lockuser",
                Password = "WrongPassword"
            };

            var user = new User
            {
                Id = "user456",
                Username = "lockuser",
                Email = "lockuser@cinescope.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                Roles = new List<string> { "User" },
                FailedLoginAttempts = 2, // Already has 2 failed attempts
                IsLocked = false
            };

            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockCursor = new Mock<IAsyncCursor<User>>();

            mockCursor.Setup(c => c.Current).Returns(new List<User> { user });
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            mockCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                UsersCollectionName = "Users"
            });

            var configuration = CreateMockConfiguration();

            var authService = new AuthService(mockMongoDbService.Object, mockSettings.Object, configuration);

            // Act
            var result = await authService.LoginAsync(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Empty(result.Token);
            Assert.Contains("account has been locked", result.Message.ToLower());
        }

        /// <summary>
        /// Tests that login is rejected for already locked accounts.
        /// </summary>
        [Fact]
        public async Task Login_WithLockedAccount_ShouldRejectLogin()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "lockeduser",
                Password = "Test@123" // Correct password, but account is locked
            };

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

            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockCursor = new Mock<IAsyncCursor<User>>();

            mockCursor.Setup(c => c.Current).Returns(new List<User> { user });
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                UsersCollectionName = "Users"
            });

            var configuration = CreateMockConfiguration();

            var authService = new AuthService(mockMongoDbService.Object, mockSettings.Object, configuration);

            // Act
            var result = await authService.LoginAsync(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Empty(result.Token);
            Assert.Contains("account is locked", result.Message);
        }

        /// <summary>
        /// Tests successful user registration with valid data.
        /// This test was failing with NullReferenceException
        /// </summary>
        [Fact]
        public async Task Register_WithValidData_ShouldCreateUserAndReturnSuccess()
        {
            // Arrange
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

            // Configure collection's FindAsync for username check to return empty results
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyMockCursor.Object);

            // Configure InsertOneAsync to not throw exceptions
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

            // Mock MongoDB settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                UsersCollectionName = "Users"
            });

            // Create configuration with JWT settings
            var configuration = CreateMockConfiguration();

            // Create auth service with mocked dependencies
            var authService = new AuthService(mockMongoDbService.Object, mockSettings.Object, configuration);

            // Act
            var result = await authService.RegisterAsync(registerRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("Registration successful", result.Message);
            Assert.NotEmpty(result.Token);
            Assert.Equal("newuser", result.User.Username);
        }

        /// <summary>
        /// Tests registration rejection when username is already taken.
        /// </summary>
        [Fact]
        public async Task Register_WithExistingUsername_ShouldReturnFailure()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "existinguser",
                Email = "newemail@cinescope.test",
                Password = "NewPass@123",
                ConfirmPassword = "NewPass@123"
            };

            // Create a sample existing user with the same username
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
            var configuration = CreateMockConfiguration();

            // Mock MongoDB settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                UsersCollectionName = "Users"
            });

            // Create auth service
            var authService = new AuthService(mockMongoDbService.Object, mockSettings.Object, configuration);

            // Act
            var result = await authService.RegisterAsync(registerRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Username already exists", result.Message);
            Assert.Empty(result.Token);
        }
    }
}