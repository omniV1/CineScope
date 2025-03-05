using System;

namespace CineScope.Shared.Models
{
    public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string UsersCollectionName { get; set; } = string.Empty;
        public string MoviesCollectionName { get; set; } = string.Empty;
        public string ReviewsCollectionName { get; set; } = string.Empty;
        public string BannedWordsCollectionName { get; set; } = string.Empty;
    }
}