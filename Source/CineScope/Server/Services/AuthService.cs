using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Shared.Auth;
using CineScope.Shared.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace CineScope.Server.Services
{
    /// <summary>
    /// Service responsible for authentication-related operations.
    /// Handles user login, registration, and token generation.
    /// </summary>
    public class AuthService : IAuthService
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
        /// Application configuration for JWT settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Maximum number of failed login attempts before account lockout.
        /// </summary>
        private const int MAX_FAILED_ATTEMPTS = 3;

        /// <summary>
        /// Initializes a new instance of the AuthService.
        /// </summary>
        /// <param name="mongoDbService">Injected MongoDB service</param>
        /// <param name="settings">Injected MongoDB settings</param>
        /// <param name="configuration">Injected application configuration</param>
        public AuthService(IMongoDbService mongoDbService, IOptions<MongoDbSettings> settings, IConfiguration configuration)
        {
            _mongoDbService = mongoDbService;
            _settings = settings.Value;
            _configuration = configuration;
        }

        /// <summary>
        /// Authenticates a user based on login credentials.
        /// Handles account lockout after multiple failed attempts.
        /// </summary>
        /// <param name="loginRequest">The login credentials</param>
        /// <returns>Authentication result with token if successful</returns>
        public async Task<AuthResponse> LoginAsync(LoginRequest loginRequest)
        {
            // Get the users collection
            var collection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);

            // Find the user by username
            var user = await collection.Find(u => u.Username == loginRequest.Username).FirstOrDefaultAsync();

            // If user not found, return authentication failure
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            // Check if the account is locked
            if (user.IsLocked)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "This account is locked due to multiple failed login attempts. Please contact support."
                };
            }

            // Verify the password
            bool passwordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);

            // If password is invalid, increment failed attempts and potentially lock account
            if (!passwordValid)
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;

                // Check if account should be locked
                if (user.FailedLoginAttempts >= MAX_FAILED_ATTEMPTS)
                {
                    user.IsLocked = true;

                    // Update the user record with lock status
                    var lockUpdate = Builders<User>.Update
                        .Set(u => u.FailedLoginAttempts, user.FailedLoginAttempts)
                        .Set(u => u.IsLocked, true);

                    await collection.UpdateOneAsync(u => u.Id == user.Id, lockUpdate);

                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Your account has been locked due to multiple failed login attempts. Please contact support."
                    };
                }
                else
                {
                    // Update failed login attempts count
                    var failedUpdate = Builders<User>.Update
                        .Set(u => u.FailedLoginAttempts, user.FailedLoginAttempts);

                    await collection.UpdateOneAsync(u => u.Id == user.Id, failedUpdate);

                    return new AuthResponse
                    {
                        Success = false,
                        Message = $"Invalid username or password. Attempts remaining: {MAX_FAILED_ATTEMPTS - user.FailedLoginAttempts}"
                    };
                }
            }

            // Password is valid, reset failed attempts and update last login
            var successUpdate = Builders<User>.Update
                .Set(u => u.FailedLoginAttempts, 0)
                .Set(u => u.LastLogin, DateTime.UtcNow);

            await collection.UpdateOneAsync(u => u.Id == user.Id, successUpdate);

            // Generate JWT token for the authenticated user
            var token = GenerateJwtToken(user);

            // Map user to DTO for response
            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl, 
                Roles = user.Roles
            };

            // Return successful authentication response
            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                User = userDto
            };
        }

        /// <summary>
        /// Registers a new user with the provided information.
        /// Verifies that username and email are not already in use.
        /// </summary>
        /// <param name="registerRequest">The registration information</param>
        /// <returns>Registration result with token if successful</returns>
        public async Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            // Get the users collection
            var collection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);

            // Check if username already exists
            var existingUsername = await collection.Find(u => u.Username == registerRequest.Username).FirstOrDefaultAsync();
            if (existingUsername != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Username already exists"
                };
            }

            // Check if email already exists
            var existingEmail = await collection.Find(u => u.Email == registerRequest.Email).FirstOrDefaultAsync();
            if (existingEmail != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email already registered"
                };
            }

            // Create new user object
            var newUser = new User
            {
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                Roles = new List<string> { "User" }, // Default role
                CreatedAt = DateTime.UtcNow,
                FailedLoginAttempts = 0,
                IsLocked = false
            };

            // Insert the new user into the database
            await collection.InsertOneAsync(newUser);

            // Generate JWT token for the new user
            var token = GenerateJwtToken(newUser);

            // Map user to DTO for response
            var userDto = new UserDto
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                ProfilePictureUrl = newUser.ProfilePictureUrl, 
                Roles = newUser.Roles
            };

            // Return successful registration response
            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                User = userDto
            };
        }

        /// <summary>
        /// Generates a JWT token for the authenticated user.
        /// </summary>
        /// <param name="user">The authenticated user</param>
        /// <returns>JWT token string</returns>
        private string GenerateJwtToken(User user)
        {
            // Get JWT configuration values
            var jwtSecret = _configuration["JwtSettings:Secret"];
            var jwtIssuer = _configuration["JwtSettings:Issuer"];
            var jwtAudience = _configuration["JwtSettings:Audience"];
            var jwtExpiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"]);

            // Create security key using the secret
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Ensure we have a valid profile picture URL
            string profilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl)
                ? user.ProfilePictureUrl
                : "/profile-pictures/default.svg";

            // Ensure path starts with /
            if (!profilePictureUrl.StartsWith("/") && !profilePictureUrl.StartsWith("http"))
            {
                profilePictureUrl = "/" + profilePictureUrl;
            }

            // Create claims for the token
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("ProfilePictureUrl", profilePictureUrl), // Make sure this claim has the exact name "ProfilePictureUrl"
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            // Add role claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpiryMinutes),
                signingCredentials: creds
            );

            // Return the serialized token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}