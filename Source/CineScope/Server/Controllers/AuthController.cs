using System.Threading.Tasks;
using CineScope.Server.Interfaces;
using CineScope.Shared.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CineScope.Server.Controllers
{
    /// <summary>
    /// API controller for authentication operations.
    /// Provides endpoints for user login and registration.
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
        /// Initializes a new instance of the AuthController.
        /// </summary>
        /// <param name="authService">Injected authentication service</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
    }
}