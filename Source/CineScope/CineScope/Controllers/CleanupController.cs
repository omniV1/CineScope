using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller responsible for database cleanup operations
/// </summary>
[ApiController]
[Route("api/clean")]
public class CleanupController : ControllerBase
{
    private readonly MongoDBSettings _settings;

    /// <summary>
    /// Constructor for CleanupController
    /// </summary>
    /// <param name="settings">MongoDB configuration settings</param>
    public CleanupController(MongoDBSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Endpoint to clean up duplicate fields in the Users collection
    /// Removes uppercase field names that might have been created erroneously
    /// </summary>
    /// <returns>Result of the cleanup operation</returns>
    [HttpGet]
    public async Task<IActionResult> CleanDuplicateFields()
    {
        try
        {
            // Initialize MongoDB connection
            var client = new MongoClient(_settings.ConnectionString);
            var database = client.GetDatabase(_settings.DatabaseName);
            var collection = database.GetCollection<BsonDocument>("Users");

            // Create a filter to find documents with uppercase field names
            // that should be lowercase according to our schema
            var filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.Exists("Username"),
                Builders<BsonDocument>.Filter.Exists("Email")
            );

            // Get all matching users
            var users = await collection.Find(filter).ToListAsync();

            foreach (var user in users)
            {
                var updates = new List<UpdateDefinition<BsonDocument>>();

                // Check for "Username" field (uppercase) and mark for removal
                // The correct field name is likely "username" (lowercase)
                if (user.Contains("Username"))
                {
                    updates.Add(Builders<BsonDocument>.Update.Unset("Username"));
                }

                // Check for "Email" field (uppercase) and mark for removal
                // The correct field name is likely "email" (lowercase)
                if (user.Contains("Email"))
                {
                    updates.Add(Builders<BsonDocument>.Update.Unset("Email"));
                }

                // If we have fields to update, execute the update
                if (updates.Count > 0)
                {
                    var combinedUpdate = Builders<BsonDocument>.Update.Combine(updates);
                    await collection.UpdateOneAsync(
                        Builders<BsonDocument>.Filter.Eq("_id", user["_id"]),
                        combinedUpdate
                    );
                }
            }

            // Drop any problematic indexes that might have been created on these fields
            try
            {
                await collection.Indexes.DropOneAsync("Username_1");
                await collection.Indexes.DropOneAsync("Email_1");
            }
            catch
            {
                // Ignore exceptions if indexes don't exist
                // This is a cleanup operation, so we continue even if some steps fail
            }

            return Ok(new { message = "Cleaned up duplicate fields and removed problematic indexes" });
        }
        catch (Exception ex)
        {
            // Return detailed error information to help with debugging
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }
}
