public class MongoDBIndexService
{
    private readonly IMongoDatabase _database;
    private readonly MongoDBSettings _settings;

    public MongoDBIndexService(MongoDBSettings settings)
    {
        _settings = settings;
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public async Task CreateIndexesAsync()
    {
        // First, handle duplicate null values in the Users collection
        await CleanupNullFieldValues<UserModel>(
            _settings.UsersCollectionName,
            new[] { "Username", "Email" });

        // Then create indexes with sparse option
        var usersCollection = _database.GetCollection<UserModel>(_settings.UsersCollectionName);

        // Create Username index - sparse means it ignores null values
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserModel>(
                Builders<UserModel>.IndexKeys.Ascending(user => user.username),
                new CreateIndexOptions
                {
                    Unique = true,
                    Sparse = true  // Skip null values 
                }));

        // Create Email index - sparse means it ignores null values
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserModel>(
                Builders<UserModel>.IndexKeys.Ascending(user => user.Email),
                new CreateIndexOptions
                {
                    Unique = true,
                    Sparse = true  // Skip null values 
                }));

        // Create LastLogin index (doesn't need to be unique)
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserModel>(
                Builders<UserModel>.IndexKeys.Descending(user => user.LastLogin)));

        // Continue with other collection indexes...
        // (other collections index creation code remains the same)
    }

    private async Task CleanupNullFieldValues<T>(string collectionName, string[] fieldNames)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);

        foreach (var fieldName in fieldNames)
        {
            Console.WriteLine($"Checking for null {fieldName} values in {collectionName}");

            // Find docs with null field value
            var filter = Builders<BsonDocument>.Filter.Eq(fieldName, BsonNull.Value);
            var nullFieldDocs = await collection.Find(filter).ToListAsync();

            if (nullFieldDocs.Count > 1)
            {
                Console.WriteLine($"Found {nullFieldDocs.Count} documents with null {fieldName} - fixing all but first one");

                // Keep the first one, update all others with generated values
                for (int i = 1; i < nullFieldDocs.Count; i++)
                {
                    var docId = nullFieldDocs[i]["_id"];
                    var updateFilter = Builders<BsonDocument>.Filter.Eq("_id", docId);
                    var update = Builders<BsonDocument>.Update
                        .Set(fieldName, $"auto_{fieldName.ToLower()}_{Guid.NewGuid().ToString("N").Substring(0, 8)}");

                    await collection.UpdateOneAsync(updateFilter, update);
                    Console.WriteLine($"Fixed document {docId} with generated {fieldName}");
                }
            }
            else
            {
                Console.WriteLine($"No duplicate null {fieldName} values found");
            }
        }
    }
}