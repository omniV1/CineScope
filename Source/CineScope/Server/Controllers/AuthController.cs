using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Shared.Auth;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace CineScope.Server.Controllers
{
    /// <summary>
    /// API controller for authentication operations.
    /// Provides endpoints for user login, registration, and token refresh.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Reference to the authentication service for business logic.
        /// </summary>
        private readonly IAuthService _authService;

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
        /// Initializes a new instance of the AuthController.
        /// </summary>
        /// <param name="authService">Injected authentication service</param>
        /// <param name="mongoDbService">Injected MongoDB service for token refresh</param>
        /// <param name="options">Injected MongoDB settings</param>
        /// <param name="configuration">Injected application configuration</param>
        public AuthController(
            IAuthService authService,
            IMongoDbService mongoDbService,
            IOptions<MongoDbSettings> options,
            IConfiguration configuration)
        {
            _authService = authService;
            _mongoDbService = mongoDbService;
            _settings = options.Value;
            _configuration = configuration;
        }

        /// <summary>
        /// POST: api/Auth/login
        /// Handles user login requests.
        /// </summary>
        /// <param name="loginRequest">The login credentials</param>
        /// <returns>Authentication result with token if successful</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest loginRequest)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Attempt to authenticate the user
            var result = await _authService.LoginAsync(loginRequest);

            // Return appropriate response based on result
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                // Return 401 Unauthorized for failed login attempts
                return Unauthorized(result);
            }
        }

        /// <summary>
        /// POST: api/Auth/register
        /// Handles user registration requests.
        /// </summary>
        /// <param name="registerRequest">The registration information</param>
        /// <returns>Registration result with token if successful</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest registerRequest)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that passwords match
            if (registerRequest.Password != registerRequest.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
                return BadRequest(ModelState);
            }

            // Attempt to register the user
            var result = await _authService.RegisterAsync(registerRequest);

            // Return appropriate response based on result
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                // Return 400 Bad Request for registration failures
                return BadRequest(result);
            }
        }

        /// <summary>
        /// POST: api/Auth/refresh
        /// Refreshes the authentication token for the current user.
        /// </summary>
        /// <returns>Authentication result with new token if successful</returns>
        /// <summary>
        /// POST: api/Auth/refresh
        /// Refreshes the authentication token for the current user.
        /// </summary>
        /// <returns>Authentication result with new token if successful</returns>
        [HttpPost("refresh")]
        [Authorize] // Require authentication
        public async Task<ActionResult<AuthResponse>> RefreshToken()
        {
            try
            {
                // Extract user ID from claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new AuthResponse { Success = false, Message = "Invalid token" });

                // Get the user from database
                var collection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);
                var user = await collection.Find(u => u.Id == userId).FirstOrDefaultAsync();

                if (user == null)
                    return Unauthorized(new AuthResponse { Success = false, Message = "User not found" });

                // Generate a new token with fresh user data
                var token = GenerateJwtToken(user);

                // Return the new token with updated user info
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Roles = user.Roles
                };

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Token = token,
                    User = userDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = $"Error refreshing token: {ex.Message}"
                });
            }
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

            // Create claims for the token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
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