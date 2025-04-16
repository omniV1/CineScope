using System;
using System.Collections.Generic;

namespace CineScope.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for user information.
    /// Used to transfer user data between the server and client.
    /// Contains only non-sensitive user information for security.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// User's chosen username for login and display purposes.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// URL to the user's profile picture.
        /// If empty or null, a default profile picture will be shown.
        /// </summary>
        public string ProfilePictureUrl { get; set; } = string.Empty;

        /// <summary>
        /// Collection of roles assigned to this user (e.g., "User", "Admin").
        /// Used for authorization and permission checking.
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();
    }
}