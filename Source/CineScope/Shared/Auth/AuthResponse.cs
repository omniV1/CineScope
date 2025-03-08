using CineScope.Shared.DTOs;

namespace CineScope.Shared.Auth
{
    /// <summary>
    /// Represents the response from authentication operations.
    /// Contains authentication result and user information.
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Indicates whether the authentication was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// A message providing details about the authentication result.
        /// Contains error information if authentication failed.
        /// </summary>
        public string Message { get; set; } = string.Empty;
         
        /// <summary>
        /// JWT token for authenticated session if authentication succeeded.
        /// Null if authentication failed.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// User data for the authenticated user if authentication succeeded.
        /// Null if authentication failed.
        /// </summary>
        public UserDto User { get; set; } = new UserDto();
    }
}