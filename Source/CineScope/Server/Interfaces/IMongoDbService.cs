using MongoDB.Driver;

namespace CineScope.Server.Interfaces
{
    /// <summary>
    /// Interface defining operations for MongoDB data access.
    /// Enables dependency injection and mocking for unit tests.
    /// </summary>
    public interface IMongoDbService
    {
        /// <summary>
        /// Gets a MongoDB collection of the specified type by name.
        /// </summary>
        /// <typeparam name="T">The model type that maps to documents in this collection</typeparam>
        /// <param name="collectionName">The name of the collection in MongoDB</param>
        /// <returns>A typed IMongoCollection instance for performing operations</returns>
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}