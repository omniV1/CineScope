using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace CineScope.Server.Models
{
    /// <summary>
    /// Represents a user in the CineScope application.
    /// This model maps directly to documents in the Users collection in MongoDB.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user.
        /// The BsonId attribute marks this as the document's primary key in MongoDB.
        /// BsonRepresentation converts between .NET string and MongoDB ObjectId.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// User's chosen username for login and display purposes.
        /// Must be unique across the system.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's email address for account recovery and notifications.
        /// Must be unique across the system.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Secure hashed representation of user's password.
        /// Raw passwords should never be stored in the database.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Collection of roles assigned to this user (e.g., "User", "Admin").
        /// Used for authorization and permission checking.
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Timestamp indicating when the user account was created.
        /// Automatically set to UTC time when a new user is registered.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp indicating when the user last logged in.
        /// Nullable to indicate a user who has never logged in.
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// Flag indicating if the account is currently locked.
        /// Set to true after multiple failed login attempts or by admin action.
        /// </summary>
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// Counter tracking consecutive failed login attempts.
        /// Used for security to implement account lockout after multiple failures.
        /// </summary>
        public int FailedLoginAttempts { get; set; } = 0;

        /// <summary>
        /// URL to the user's profile picture.
        /// This can be an absolute URL to an external service or a relative path to a locally stored image.
        /// If empty or null, a default profile picture will be shown.
        /// </summary>
        public string ProfilePictureUrl { get; set; } = string.Empty;
    }
}