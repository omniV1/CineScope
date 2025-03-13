// Source/CineScope/Shared/DTOs/UserProfileDto.cs
using System;

namespace CineScope.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for user profile information.
    /// Contains personal user data for profile management.
    /// </summary>
    public class UserProfileDto
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
        /// Timestamp indicating when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp indicating when the user last logged in.
        /// Nullable for users who have never logged in.
        /// </summary>
        public DateTime? LastLogin { get; set; }
    }

    /// <summary>
    /// Data Transfer Object (DTO) for public user information.
    /// Contains only non-sensitive information visible to other users.
    /// </summary>
    public class PublicUserDto
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// User's chosen username for display purposes.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// URL to the user's profile picture.
        /// If empty or null, a default profile picture will be shown.
        /// </summary>
        public string ProfilePictureUrl { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp indicating when the user joined the platform.
        /// </summary>
        public DateTime JoinDate { get; set; }
    }
}