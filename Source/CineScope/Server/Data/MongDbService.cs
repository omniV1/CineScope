using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace CineScope.Server.Data
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IOptions<MongoDbSettings> options)
        {
            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}