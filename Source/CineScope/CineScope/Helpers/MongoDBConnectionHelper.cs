using MongoDB.Driver;
using CineScope.Models;  

namespace CineScope.Helpers
{
    public static class MongoDbConnectionHelper
    {
        public static IMongoClient CreateClient(Models.MongoDBSettings settings)  // Fully qualify the type
        {
            var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
            clientSettings.ServerApi = new ServerApi(ServerApiVersion.V1);
            return new MongoClient(clientSettings);
        }
    }
}
