using MongoDB.Driver;
using Microsoft.Extensions.Options;
using CineScope.Server.Interfaces;

namespace CineScope.Server.Data
{
    /// <summary>
    /// Service responsible for providing access to MongoDB collections.
    /// Implements the IMongoDbService interface for dependency injection.
    /// </summary>
    public class MongoDbService : IMongoDbService
    {
        /// <summary>
        /// Reference to the MongoDB database instance.
        /// Initialized in the constructor from configuration settings.
        /// </summary>
        private readonly IMongoDatabase _database;

        /// <summary>
        /// Initializes a new instance of the MongoDbService.
        /// Sets up the MongoDB client and database connection using injected configuration.
        /// </summary>
        /// <param name="options">MongoDB configuration options from appsettings.json</param>
        public MongoDbService(IOptions<MongoDbSettings> options)
        {
            // Extract settings from the injected options
            var settings = options.Value;

            // Create a new MongoDB client using the connection string
            var client = new MongoClient(settings.ConnectionString);

            // Get a reference to the specific database for the application
            _database = client.GetDatabase(settings.DatabaseName);
        }

        /// <summary>
        /// Gets a MongoDB collection of the specified type by name.
        /// This method provides typed access to database collections.
        /// </summary>
        /// <typeparam name="T">The model type that maps to documents in this collection</typeparam>
        /// <param name="collectionName">The name of the collection in MongoDB</param>
        /// <returns>A typed IMongoCollection instance for performing operations</returns>
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}