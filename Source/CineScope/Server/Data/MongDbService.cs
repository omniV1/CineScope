using MongoDB.Driver;
using Microsoft.Extensions.Options;
using CineScope.Server.Interfaces;

namespace CineScope.Server.Data
{
    public class MongoDbService : IMongoDbService  // Make sure this explicitly implements IMongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IOptions<MongoDbSettings> options)
        {
            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        // Explicitly implement the interface method
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}