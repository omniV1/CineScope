using System.ComponentModel.DataAnnotations;

namespace CineScope.Shared.Auth
{
    /// <summary>
    /// Represents a user login request.
    /// Contains the data required for authentication.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// The username for authentication.
        /// Required field with validation error message.
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The password for authentication.
        /// Required field with validation error message.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Option to maintain user session for extended periods.
        /// Controls whether authentication token has longer expiration.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}