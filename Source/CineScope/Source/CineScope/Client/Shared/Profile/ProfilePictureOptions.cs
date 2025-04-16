namespace CineScope.Client.Shared.Profile
{
    /// <summary>
    /// Provides a collection of predefined SVG profile pictures.
    /// </summary>
    public static class ProfilePictureOptions
    {
        /// <summary>
        /// Base URL for profile pictures, either hosted on GitHub via jsDelivr or local to the application.
        /// For GitHub hosting, replace with your repository details.
        /// </summary>
        private const string BaseUrl = "/profile-pictures/";

        /// <summary>
        /// Gets the URLs to all available SVG profile pictures.
        /// </summary>
        public static readonly string[] Options = new string[]
        {
            BaseUrl + "avatar1.svg",
            BaseUrl + "avatar2.svg",
            BaseUrl + "avatar3.svg",
            BaseUrl + "avatar4.svg",
            BaseUrl + "avatar5.svg",
            BaseUrl + "avatar6.svg",
            BaseUrl + "avatar7.svg",
            BaseUrl + "avatar8.svg",
        };

        /// <summary>
        /// Gets the default profile picture URL for users who haven't selected one.
        /// </summary>
        public static readonly string DefaultProfilePicture = BaseUrl + "default.svg";
    }
}