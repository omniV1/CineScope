using MongoDB.Driver;

namespace CineScope.Server.Interfaces
{
    public interface IMongoDbService
    {
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}