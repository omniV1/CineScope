namespace CineScope.Server.Data
{
    /// <summary>
    /// Configuration settings for connecting to MongoDB.
    /// Values are populated from appsettings.json configuration.
    /// </summary>
    public class MongoDbSettings
    {
        /// <summary>
        /// MongoDB connection string containing server address, credentials, and connection options.
        /// Format: mongodb://[username:password@]host[:port][/database][?options]
        /// For Atlas: mongodb+srv://username:password@cluster.mongodb.net/database
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Name of the MongoDB database to use for the CineScope application.
        /// All collections will be stored within this database.
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the collection storing user account information.
        /// Maps to the User model class.
        /// </summary>
        public string UsersCollectionName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the collection storing movie information.
        /// Maps to the Movie model class.
        /// </summary>
        public string MoviesCollectionName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the collection storing user reviews.
        /// Maps to the Review model class.
        /// </summary>
        public string ReviewsCollectionName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the collection storing banned words for content filtering.
        /// Maps to the BannedWord model class.
        /// </summary>
        public string BannedWordsCollectionName { get; set; } = string.Empty;
    }
}