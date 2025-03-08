using System.ComponentModel.DataAnnotations;

namespace CineScope.Shared.Auth
{
    /// <summary>
    /// Represents a user registration request.
    /// Contains the data required to create a new user account.
    /// Includes data validation attributes for form validation.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// The desired username for the new account.
        /// Must be between 3 and 50 characters.
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The email address for the new account.
        /// Must be a valid email format.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The password for the new account.
        /// Must be at least 6 characters.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation of the password.
        /// Must match the Password property.
        /// </summary>
        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}