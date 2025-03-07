using Microsoft.AspNetCore.Mvc;
using CineScope.Shared.Models;
using CineScope.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CineScope.Controllers
{
    /// <summary>
    /// Controller responsible for user authentication, registration, and account management
    /// Provides endpoints for login, registration, and administrative functions
    /// </summary>
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// Constructor for UserController
        /// </summary>
        /// <param name="userService">Service for user-related operations</param>
        /// <param name="logger">Logger for recording events and errors</param>
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Handles user login attempts, managing authentication and account security
        /// </summary>
        /// <param name="model">Login credentials including username and password</param>
        /// <returns>Success status and user information on successful login</returns>
        // POST: api/users/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                // Log the login attempt (without recording the password)
                _logger.LogInformation($"Login attempt for username: {model.Username}");

                // Attempt to authenticate the user with provided credentials
                bool isAuthenticated = await _userService.AuthenticateUserAsync(model.Username, model.Password);

                if (isAuthenticated)
                {
                    // Fetch user details to return in the response
                    var user = await _userService.GetUserByUsernameAsync(model.Username);
                    _logger.LogInformation($"Login successful for: {model.Username}");

                    // Return user information while excluding sensitive data
                    return Ok(new
                    {
                        success = true,
                        user = new
                        {
                            id = user.Id.ToString(),
                            username = user.username,
                            email = user.Email,
                            roles = user.Roles
                        }
                    });
                }

                // Check if the account is locked due to previous failed attempts
                bool isLocked = await _userService.IsAccountLockedAsync(model.Username);
                if (isLocked)
                {
                    _logger.LogWarning($"Account locked: {model.Username}");
                    return BadRequest(new { success = false, message = "Your account has been locked due to too many failed login attempts" });
                }

                // Record this failed login attempt for security monitoring
                await _userService.RecordFailedLoginAttemptAsync(model.Username);
                _logger.LogWarning($"Failed login attempt for: {model.Username}");

                // Return generic error to prevent username enumeration attacks
                return BadRequest(new { success = false, message = "Invalid username or password" });
            }
            catch (Exception ex)
            {
                // Log detailed error but return generic message to client
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Handles new user registration, including validation of username and email uniqueness
        /// </summary>
        /// <param name="model">Registration information including username, email and password</param>
        /// <returns>Success status and created user information</returns>
        // POST: api/users/register
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                _logger.LogInformation($"Registration attempt for username: {model.Username}, email: {model.Email}");

                // Check if username already exists to maintain uniqueness
                var existingUser = await _userService.GetUserByUsernameAsync(model.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Registration failed - username already exists: {model.Username}");
                    return BadRequest(new { success = false, message = "Username already exists" });
                }

                // Check if email already exists to maintain uniqueness
                existingUser = await _userService.GetUserByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Registration failed - email already exists: {model.Email}");
                    return BadRequest(new { success = false, message = "Email already exists" });
                }

                // Create new user with default settings
                var newUser = new UserModel
                {
                    username = model.Username,
                    Email = model.Email,
                    PasswordHash = model.Password, // Note: UserService will hash this before storing
                    Roles = new List<string> { "User" }, // Default role for new users
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = null, // No login yet
                    IsLocked = false,
                    FailedLoginAttempts = 0
                };

                // Save the user to the database
                var createdUser = await _userService.CreateUserAsync(newUser);
                _logger.LogInformation($"Registration successful for: {model.Username}");

                // Return success with limited user info to avoid exposing sensitive data
                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        id = createdUser.Id.ToString(),
                        username = createdUser.username,
                        email = createdUser.Email
                    }
                });
            }
            catch (Exception ex)
            {
                // Log detailed error but return generic message to client
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { success = false, message = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Diagnostic endpoint to verify database connectivity and view sample user data
        /// Note: This should be secured or disabled in production environments
        /// </summary>
        /// <returns>Connection status and sample user data for verification</returns>
        // GET: api/users/check-db-connection
        [HttpGet("check-db-connection")]
        public async Task<ActionResult> CheckDbConnection()
        {
            try
            {
                _logger.LogInformation("Database connection check requested");

                // Attempt to retrieve users to verify database connectivity
                var users = await _userService.GetAllUsersAsync();

                // Create a sanitized list of sample users for diagnostics
                // Only showing the first 5 users with partial password hashes
                var sampleUsers = users.Take(5).Select(u => new {
                    id = u.Id.ToString(),
                    username = u.username,
                    email = u.Email,
                    passwordHash = u.PasswordHash.Substring(0, 10) + "...", // Only show part of the hash for security
                    isLocked = u.IsLocked,
                    failedLoginAttempts = u.FailedLoginAttempts
                }).ToList();

                _logger.LogInformation($"Database connection successful. Found {users.Count} users.");

                // Return diagnostic information
                return Ok(new
                {
                    success = true,
                    message = "Database connection successful",
                    userCount = users.Count,
                    sampleUsers = sampleUsers
                });
            }
            catch (Exception ex)
            {
                // Log and return detailed error for diagnostics
                _logger.LogError(ex, "Error checking database connection");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Database connection failed: {ex.Message}",
                    details = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Unlocks a user account that has been locked due to multiple failed login attempts
        /// </summary>
        /// <param name="username">Username of the account to unlock</param>
        /// <returns>Status of the unlock operation</returns>
        // POST: api/users/unlock-account?username=xxx
        [HttpPost("unlock-account")]
        public async Task<ActionResult> UnlockAccount([FromQuery] string username)
        {
            try
            {
                _logger.LogInformation($"Account unlock requested for: {username}");

                // Reset the login attempts counter, effectively unlocking the account
                await _userService.ResetFailedLoginAttemptsAsync(username);

                // Verify the account was actually unlocked by retrieving updated user info
                var user = await _userService.GetUserByUsernameAsync(username);

                // Handle case where username doesn't exist
                if (user == null)
                {
                    _logger.LogWarning($"User not found for unlock: {username}");
                    return NotFound(new { success = false, message = $"User '{username}' not found" });
                }

                _logger.LogInformation($"Account unlocked successfully: {username}");

                // Return status with current account lock state
                return Ok(new
                {
                    success = true,
                    message = $"Account {username} has been unlocked",
                    user = new
                    {
                        username = user.username,
                        isLocked = user.IsLocked,
                        failedLoginAttempts = user.FailedLoginAttempts
                    }
                });
            }
            catch (Exception ex)
            {
                // Log detailed error
                _logger.LogError(ex, $"Error unlocking account {username}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Failed to unlock account: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Diagnostic tool to generate password hashes for testing
        /// Warning: This endpoint should be secured or disabled in production
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <returns>The hashed version of the input password and common password hashes</returns>
        // GET: api/users/hash-password?password=xxx
        [HttpGet("hash-password")]
        public ActionResult HashPassword([FromQuery] string password)
        {
            try
            {
                _logger.LogInformation("Password hash generation requested");

                // Generate hash for the provided password
                var hashedPassword = _userService.HashPasswordForTesting(password);

                // Generate hashes for common passwords to help with debugging
                var commonPasswords = new Dictionary<string, string>
                {
                    { "admin", "admin" },
                    { "admin123", "admin123" },
                    { "password", "password" },
                    { "123456", "123456" },
                    { "qwerty", "qwerty" }
                };

                // Create a dictionary of common password hashes for comparison
                var commonHashes = new Dictionary<string, string>();
                foreach (var pwd in commonPasswords)
                {
                    commonHashes.Add(pwd.Key, _userService.HashPasswordForTesting(pwd.Value));
                }

                // Return all hash information
                return Ok(new
                {
                    success = true,
                    password = password,
                    hashedPassword = hashedPassword,
                    commonHashes = commonHashes
                });
            }
            catch (Exception ex)
            {
                // Log detailed error
                _logger.LogError(ex, "Error generating password hash");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error generating hash: {ex.Message}"
                });
            }
        }
    }
}
