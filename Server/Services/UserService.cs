using System;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Shared.DTOs;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using CineScope.Client.Shared.Profile;

namespace CineScope.Server.Services
{
    /// <summary>
    /// Service responsible for user-related operations.
    /// Handles data access and business logic for user entities.
    /// </summary>
    public class UserService
    {
        /// <summary>
        /// Reference to the MongoDB service for database operations.
        /// </summary>
        private readonly IMongoDbService _mongoDbService;

        /// <summary>
        /// MongoDB settings from configuration.
        /// </summary>
        private readonly MongoDbSettings _settings;

        /// <summary>
        /// Initializes a new instance of the UserService.
        /// </summary>
        /// <param name="mongoDbService">Injected MongoDB service</param>
        /// <param name="settings">Injected MongoDB settings</param>
        public UserService(IMongoDbService mongoDbService, IOptions<MongoDbSettings> settings)
        {
            _mongoDbService = mongoDbService;
            _settings = settings.Value;
        }

        /// <summary>
        /// Retrieves a user's profile information.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>The user profile information</returns>
        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            // Get the users collection
            var collection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);

            // Find the user by ID
            var user = await collection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            // If user not found, return null
            if (user == null)
            {
                return null;
            }

            // Map user to profile DTO
            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl, // Added this line to include the profile picture
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };
        }

        /// <summary>
        /// Updates a user's profile information.
        /// </summary>
        /// <param name="userId">The ID of the user to update</param>
        /// <param name="updateProfileRequest">The updated profile information</param>
        /// <returns>Result indicating success or failure with details</returns>
        public async Task<ProfileUpdateResult> UpdateUserProfileAsync(string userId, UpdateProfileRequest updateProfileRequest)
        {
            // Get the users collection
            var collection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);

            // Find the user by ID
            var user = await collection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            // If user not found, return failure
            if (user == null)
            {
                return new ProfileUpdateResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Check if username is changing and if it's already taken
            if (user.Username != updateProfileRequest.Username)
            {
                var existingUsername = await collection.Find(u => u.Username == updateProfileRequest.Username && u.Id != userId).FirstOrDefaultAsync();
                if (existingUsername != null)
                {
                    return new ProfileUpdateResult
                    {
                        Success = false,
                        Message = "Username is already taken"
                    };
                }
            }

            // Check if email is changing and if it's already taken
            if (user.Email != updateProfileRequest.Email)
            {
                var existingEmail = await collection.Find(u => u.Email == updateProfileRequest.Email && u.Id != userId).FirstOrDefaultAsync();
                if (existingEmail != null)
                {
                    return new ProfileUpdateResult
                    {
                        Success = false,
                        Message = "Email is already registered"
                    };
                }
            }

            // Verify current password if changing password
            if (!string.IsNullOrEmpty(updateProfileRequest.NewPassword))
            {
                if (string.IsNullOrEmpty(updateProfileRequest.CurrentPassword))
                {
                    return new ProfileUpdateResult
                    {
                        Success = false,
                        Message = "Current password is required to change password"
                    };
                }

                bool passwordValid = BCrypt.Net.BCrypt.Verify(updateProfileRequest.CurrentPassword, user.PasswordHash);
                if (!passwordValid)
                {
                    return new ProfileUpdateResult
                    {
                        Success = false,
                        Message = "Current password is incorrect"
                    };
                }
            }

            // Build update definition
            var updateDefinition = Builders<User>.Update
                .Set(u => u.Username, updateProfileRequest.Username)
                .Set(u => u.Email, updateProfileRequest.Email)
                .Set(u => u.ProfilePictureUrl, updateProfileRequest.ProfilePictureUrl); // Added this line to update profile picture

            // Add password update if provided
            if (!string.IsNullOrEmpty(updateProfileRequest.NewPassword))
            {
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(updateProfileRequest.NewPassword);
                updateDefinition = updateDefinition.Set(u => u.PasswordHash, newPasswordHash);
            }

            // Update the user in the database
            var updateResult = await collection.UpdateOneAsync(u => u.Id == userId, updateDefinition);

            // Return result
            if (updateResult.ModifiedCount > 0)
            {
                return new ProfileUpdateResult
                {
                    Success = true,
                    Message = "Profile updated successfully"
                };
            }
            else
            {
                return new ProfileUpdateResult
                {
                    Success = false,
                    Message = "No changes were made to the profile"
                };
            }
        }

        /// <summary>
        /// Retrieves public information about a user.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>Public user information</returns>
        public async Task<PublicUserDto> GetPublicUserInfoAsync(string userId)
        {
            // Get the users collection
            var collection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);

            // Find the user by ID
            var user = await collection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            // If user not found, return null
            if (user == null)
            {
                return null;
            }

            // Map user to public DTO (expose username, profile picture and join date)
            return new PublicUserDto
            {
                Id = user.Id,
                Username = user.Username,
                ProfilePictureUrl = user.ProfilePictureUrl, // Added this line to include profile picture
                JoinDate = user.CreatedAt
            };
        }

        /// <summary>
        /// Updates just a user's profile picture.
        /// </summary>
        /// <param name="userId">The ID of the user to update</param>
        /// <param name="profilePictureUrl">The new profile picture URL</param>
        /// <returns>Result indicating success or failure</returns>
        public async Task<ProfileUpdateResult> UpdateProfilePictureAsync(string userId, string profilePictureUrl)
        {
            // Get the users collection
            var collection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);

            // Find the user by ID
            var user = await collection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            // If user not found, return failure
            if (user == null)
            {
                return new ProfileUpdateResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Build update definition for just the profile picture
            var updateDefinition = Builders<User>.Update
                .Set(u => u.ProfilePictureUrl, profilePictureUrl);

            // Update the user in the database
            var updateResult = await collection.UpdateOneAsync(u => u.Id == userId, updateDefinition);

            // Return result
            return new ProfileUpdateResult
            {
                Success = updateResult.ModifiedCount > 0,
                Message = updateResult.ModifiedCount > 0
                    ? "Profile picture updated successfully"
                    : "No changes were made to the profile picture"
            };
        }
    }

    /// <summary>
    /// Represents the result of a profile update operation.
    /// </summary>
    public class ProfileUpdateResult
    {
        /// <summary>
        /// Indicates whether the update was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// A message providing details about the result.
        /// Contains error information if update failed.
        /// </summary>
        public string Message { get; set; }
    }
}