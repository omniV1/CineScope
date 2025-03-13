namespace CineScope.Client.Shared.Profile
{
    /// <summary>
    /// Represents a profile update request.
    /// Used to transfer profile update data from client to server.
    /// </summary>
    public class UpdateProfileRequest
    {
        /// <summary>
        /// The updated username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The updated email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The URL of the selected profile picture.
        /// </summary>
        public string ProfilePictureUrl { get; set; } = string.Empty;

        /// <summary>
        /// The current password for verification.
        /// Required when changing the password.
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// The new password to set.
        /// Optional - only provided when changing password.
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }
}
