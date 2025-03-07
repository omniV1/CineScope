using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

/// <summary>
/// Service responsible for setting up and maintaining MongoDB indexes
/// Ensures proper database performance and data integrity
/// </summary>
public class MongoDBIndexService
{
    private readonly IMongoDatabase _database;
    private readonly MongoDBSettings _settings;

    /// <summary>
    /// Constructor for MongoDBIndexService
    /// </summary>
    /// <param name="settings">MongoDB connection and database settings</param>
    public MongoDBIndexService(MongoDBSettings settings)
    {
        _settings = settings;

        // Initialize MongoDB client using connection string from settings
        var client = new MongoClient(settings.ConnectionString);

        // Get reference to the database
        _database = client.GetDatabase(settings.DatabaseName);
    }

    /// <summary>
    /// Creates all necessary indexes for the MongoDB collections
    /// Also handles cleanup of problematic null values that could break unique indexes
    /// </summary>
    /// <returns>Task representing the index creation operation</returns>
    public async Task CreateIndexesAsync()
    {
        // First, we need to handle documents with duplicate null values
        // MongoDB treats null as a value, so multiple documents with null in a unique index field would conflict
        await CleanupNullFieldValues<UserModel>(
            _settings.UsersCollectionName,
            new[] { "Username", "Email" });

        // After cleanup, we can create indexes safely
        var usersCollection = _database.GetCollection<UserModel>(_settings.UsersCollectionName);

        // Create Username index
        // - Unique: No two users can have the same username
        // - Sparse: Ignores documents where the username field is null
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserModel>(
                Builders<UserModel>.IndexKeys.Ascending(user => user.username),
                new CreateIndexOptions
                {
                    Unique = true,
                    Sparse = true  // Skip null values when enforcing uniqueness
                }));

        // Create Email index
        // - Unique: No two users can have the same email
        // - Sparse: Ignores documents where the email field is null
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserModel>(
                Builders<UserModel>.IndexKeys.Ascending(user => user.Email),
                new CreateIndexOptions
                {
                    Unique = true,
                    Sparse = true  // Skip null values when enforcing uniqueness
                }));

        // Create LastLogin index for efficient sorting by last login date
        // Descending order to optimize queries that sort by most recent logins
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserModel>(
                Builders<UserModel>.IndexKeys.Descending(user => user.LastLogin)));

        // Additional collection indexes would be created here
        // For movies, reviews, banned words, etc.
    }

    /// <summary>
    /// Cleans up documents with null values in fields that will have unique indexes
    /// This prevents errors when creating unique indexes when multiple documents have null values
    /// </summary>
    /// <typeparam name="T">The model type for the collection</typeparam>
    /// <param name="collectionName">Name of the collection to clean up</param>
    /// <param name="fieldNames">Array of field names to check for null values</param>
    /// <returns>Task representing the cleanup operation</returns>
    private async Task CleanupNullFieldValues<T>(string collectionName, string[] fieldNames)
    {
        // Get a reference to the collection using BsonDocument for flexibility
        var collection = _database.GetCollection<BsonDocument>(collectionName);

        // Process each field that needs null value cleanup
        foreach (var fieldName in fieldNames)
        {
            Console.WriteLine($"Checking for null {fieldName} values in {collectionName}");

            // Find documents where the field has a null value
            var filter = Builders<BsonDocument>.Filter.Eq(fieldName, BsonNull.Value);
            var nullFieldDocs = await collection.Find(filter).ToListAsync();

            // If we have more than one document with null value, we need to fix them
            if (nullFieldDocs.Count > 1)
            {
                Console.WriteLine($"Found {nullFieldDocs.Count} documents with null {fieldName} - fixing all but first one");

                // Keep the first document with null as is, update all others with unique generated values
                // This approach preserves one null document which is fine with sparse indexes
                for (int i = 1; i < nullFieldDocs.Count; i++)
                {
                    var docId = nullFieldDocs[i]["_id"];
                    var updateFilter = Builders<BsonDocument>.Filter.Eq("_id", docId);

                    // Generate a unique value for the field using a UUID
                    var update = Builders<BsonDocument>.Update
                        .Set(fieldName, $"auto_{fieldName.ToLower()}_{Guid.NewGuid().ToString("N").Substring(0, 8)}");

                    // Update the document
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

