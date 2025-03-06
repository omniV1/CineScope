using Microsoft.AspNetCore.Mvc;
using CineScope.Shared.Models;
using CineScope.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CineScope.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                _logger.LogInformation($"Login attempt for username: {model.Username}");
                bool isAuthenticated = await _userService.AuthenticateUserAsync(model.Username, model.Password);

                if (isAuthenticated)
                {
                    var user = await _userService.GetUserByUsernameAsync(model.Username);
                    _logger.LogInformation($"Login successful for: {model.Username}");
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

                // Check if account is locked
                bool isLocked = await _userService.IsAccountLockedAsync(model.Username);
                if (isLocked)
                {
                    _logger.LogWarning($"Account locked: {model.Username}");
                    return BadRequest(new { success = false, message = "Your account has been locked due to too many failed login attempts" });
                }

                // Record failed attempt
                await _userService.RecordFailedLoginAttemptAsync(model.Username);
                _logger.LogWarning($"Failed login attempt for: {model.Username}");

                return BadRequest(new { success = false, message = "Invalid username or password" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                _logger.LogInformation($"Registration attempt for username: {model.Username}, email: {model.Email}");
                
                // Check if username already exists
                var existingUser = await _userService.GetUserByUsernameAsync(model.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Registration failed - username already exists: {model.Username}");
                    return BadRequest(new { success = false, message = "Username already exists" });
                }

                // Check if email already exists
                existingUser = await _userService.GetUserByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Registration failed - email already exists: {model.Email}");
                    return BadRequest(new { success = false, message = "Email already exists" });
                }

                // Create new user
                var newUser = new UserModel
                {
                    username = model.Username,
                    Email = model.Email,
                    PasswordHash = model.Password, // UserService will hash this
                    Roles = new List<string> { "User" },
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = null,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                };

                var createdUser = await _userService.CreateUserAsync(newUser);
                _logger.LogInformation($"Registration successful for: {model.Username}");

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
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { success = false, message = "An error occurred during registration" });
            }
        }

        // GET: api/users/check-db-connection
        [HttpGet("check-db-connection")]
        public async Task<ActionResult> CheckDbConnection()
        {
            try
            {
                _logger.LogInformation("Database connection check requested");
                
                // Try to get all users to verify DB connection
                var users = await _userService.GetAllUsersAsync();
                
                // Return count and first few usernames for verification
                var sampleUsers = users.Take(5).Select(u => new { 
                    id = u.Id.ToString(),
                    username = u.username, 
                    email = u.Email,
                    passwordHash = u.PasswordHash.Substring(0, 10) + "...", // Show part of the hash for debugging
                    isLocked = u.IsLocked,
                    failedLoginAttempts = u.FailedLoginAttempts
                }).ToList();
                
                _logger.LogInformation($"Database connection successful. Found {users.Count} users.");
                
                return Ok(new { 
                    success = true, 
                    message = "Database connection successful",
                    userCount = users.Count,
                    sampleUsers = sampleUsers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database connection");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Database connection failed: {ex.Message}",
                    details = ex.StackTrace
                });
            }
        }

        // POST: api/users/unlock-account?username=xxx
        [HttpPost("unlock-account")]
        public async Task<ActionResult> UnlockAccount([FromQuery] string username)
        {
            try
            {
                _logger.LogInformation($"Account unlock requested for: {username}");
                
                await _userService.ResetFailedLoginAttemptsAsync(username);
                
                // Verify the account was unlocked
                var user = await _userService.GetUserByUsernameAsync(username);
                
                if (user == null)
                {
                    _logger.LogWarning($"User not found for unlock: {username}");
                    return NotFound(new { success = false, message = $"User '{username}' not found" });
                }
                
                _logger.LogInformation($"Account unlocked successfully: {username}");
                
                return Ok(new { 
                    success = true, 
                    message = $"Account {username} has been unlocked",
                    user = new {
                        username = user.username,
                        isLocked = user.IsLocked,
                        failedLoginAttempts = user.FailedLoginAttempts
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unlocking account {username}");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Failed to unlock account: {ex.Message}"
                });
            }
        }
        
        // GET: api/users/hash-password?password=xxx
        [HttpGet("hash-password")]
        public ActionResult HashPassword([FromQuery] string password)
        {
            try
            {
                _logger.LogInformation("Password hash generation requested");
                
                var hashedPassword = _userService.HashPasswordForTesting(password);
                
                // Check for common admin passwords
                var commonPasswords = new Dictionary<string, string>
                {
                    { "admin", "admin" },
                    { "admin123", "admin123" },
                    { "password", "password" },
                    { "123456", "123456" },
                    { "qwerty", "qwerty" }
                };
                
                var commonHashes = new Dictionary<string, string>();
                foreach (var pwd in commonPasswords)
                {
                    commonHashes.Add(pwd.Key, _userService.HashPasswordForTesting(pwd.Value));
                }
                
                return Ok(new { 
                    success = true,
                    password = password,
                    hashedPassword = hashedPassword,
                    commonHashes = commonHashes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating password hash");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Error generating hash: {ex.Message}"
                });
            }
        }
    }
} 