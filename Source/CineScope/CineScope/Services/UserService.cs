using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CineScope.Services
{
    /// <summary>
    /// Service responsible for user management and authentication
    /// Implements IUserService to provide user operations and security features
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        // Maximum number of failed login attempts before account lockout
        private const int MAX_FAILED_ATTEMPTS = 3;

        /// <summary>
        /// Constructor for UserService
        /// </summary>
        /// <param name="userRepository">Repository for accessing user data</param>
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Retrieves all users from the database
        /// </summary>
        /// <returns>A list of all users</returns>
        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        /// <summary>
        /// Finds a user by their string ID
        /// </summary>
        /// <param name="id">String representation of the user's ObjectId</param>
        /// <returns>The user if found, null otherwise</returns>
        public async Task<UserModel> GetUserByIdAsync(string id)
        {
            // Convert string ID to MongoDB ObjectId
            return await _userRepository.GetByIdAsync(new ObjectId(id));
        }

        /// <summary>
        /// Finds a user by their username
        /// </summary>
        /// <param name="username">Username to search for</param>
        /// <returns>The user if found, null otherwise</returns>
        public async Task<UserModel> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        /// <summary>
        /// Finds a user by their email address
        /// </summary>
        /// <param name="email">Email address to search for</param>
        /// <returns>The user if found, null otherwise</returns>
        public async Task<UserModel> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        /// <summary>
        /// Creates a new user with security setup
        /// </summary>
        /// <param name="user">The user to create</param>
        /// <returns>The created user with hashed password and initialized security fields</returns>
        public async Task<UserModel> CreateUserAsync(UserModel user)
        {
            // Hash the password before storing for security
            user.PasswordHash = HashPassword(user.PasswordHash);

            // Set default values for new user
            user.CreatedAt = DateTime.UtcNow;
            user.LastLogin = null;  // No login yet
            user.IsLocked = false;  // Account starts unlocked
            user.FailedLoginAttempts = 0;  // No failed attempts yet

            return await _userRepository.CreateAsync(user);
        }

        /// <summary>
        /// Updates an existing user in the database
        /// </summary>
        /// <param name="id">String representation of the user's ObjectId</param>
        /// <param name="user">The updated user data</param>
        public async Task UpdateUserAsync(string id, UserModel user)
        {
            // Convert string ID to MongoDB ObjectId
            await _userRepository.UpdateAsync(new ObjectId(id), user);
        }

        /// <summary>
        /// Deletes a user from the database
        /// </summary>
        /// <param name="id">String representation of the user's ObjectId</param>
        public async Task DeleteUserAsync(string id)
        {
            // Convert string ID to MongoDB ObjectId
            await _userRepository.DeleteAsync(new ObjectId(id));
        }

        /// <summary>
        /// Authenticates a user with their username and password
        /// </summary>
        /// <param name="username">The username for login</param>
        /// <param name="password">The provided password</param>
        /// <returns>True if authentication succeeds, false otherwise</returns>
        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            // Immediately fail if user doesn't exist or account is locked
            if (user == null || user.IsLocked)
                return false;

            // Verify password by hashing the provided password and comparing with stored hash
            if (VerifyPassword(password, user.PasswordHash))
            {
                // On successful login, update login timestamp and reset failed attempts
                user.LastLogin = DateTime.UtcNow;
                user.FailedLoginAttempts = 0;
                await _userRepository.UpdateAsync(user.Id, user);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a user account is locked due to too many failed login attempts
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <returns>True if the account is locked, false otherwise</returns>
        public async Task<bool> IsAccountLockedAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user != null && user.IsLocked;
        }

        /// <summary>
        /// Records a failed login attempt and locks the account if threshold is exceeded
        /// </summary>
        /// <param name="username">The username that failed login</param>
        public async Task RecordFailedLoginAttemptAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user != null)
            {
                // Increment failed attempts counter
                user.FailedLoginAttempts++;

                // Lock account if maximum failed attempts reached
                if (user.FailedLoginAttempts >= MAX_FAILED_ATTEMPTS)
                {
                    user.IsLocked = true;
                }

                // Update user with new failed attempt count and lock status
                await _userRepository.UpdateAsync(user.Id, user);
            }
        }

        /// <summary>
        /// Resets failed login attempts and unlocks a user account
        /// </summary>
        /// <param name="username">The username of the account to unlock</param>
        public async Task ResetFailedLoginAttemptsAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user != null)
            {
                // Reset security flags
                user.FailedLoginAttempts = 0;
                user.IsLocked = false;

                // Update the user record
                await _userRepository.UpdateAsync(user.Id, user);
            }
        }

        /// <summary>
        /// Hashes a password using SHA-256
        /// </summary>
        /// <param name="password">The plain text password to hash</param>
        /// <returns>The password hash as a lowercase hex string</returns>
        /// <remarks>
        /// Note: For production systems, a more secure hashing algorithm with salt
        /// like PBKDF2, Bcrypt, or Argon2 would be recommended
        /// </remarks>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // Convert password to bytes and hash it
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert bytes to hex string without dashes and in lowercase
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Verifies a provided password against a stored hash
        /// </summary>
        /// <param name="providedPassword">The password to verify</param>
        /// <param name="storedHash">The stored hash to compare against</param>
        /// <returns>True if the password matches, false otherwise</returns>
        private bool VerifyPassword(string providedPassword, string storedHash)
        {
            // Hash the provided password and compare with stored hash
            var hashedProvidedPassword = HashPassword(providedPassword);
            return hashedProvidedPassword == storedHash;
        }

        /// <summary>
        /// Public method for testing password hashing (should be removed in production)
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>The hash of the provided password</returns>
        public string HashPasswordForTesting(string password)
        {
            return HashPassword(password);
        }
    }
}

