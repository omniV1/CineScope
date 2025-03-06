using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/clean")]
public class CleanupController : ControllerBase
{
    private readonly MongoDBSettings _settings;

    public CleanupController(MongoDBSettings settings)
    {
        _settings = settings;
    }

    [HttpGet]
    public async Task<IActionResult> CleanDuplicateFields()
    {
        try
        {
            var client = new MongoClient(_settings.ConnectionString);
            var database = client.GetDatabase(_settings.DatabaseName);
            var collection = database.GetCollection<BsonDocument>("Users");

            // Find documents with uppercase fields
            var filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.Exists("Username"),
                Builders<BsonDocument>.Filter.Exists("Email")
            );

            var users = await collection.Find(filter).ToListAsync();

            foreach (var user in users)
            {
                var updates = new List<UpdateDefinition<BsonDocument>>();

                // Remove uppercase fields
                if (user.Contains("Username"))
                {
                    updates.Add(Builders<BsonDocument>.Update.Unset("Username"));
                }

                if (user.Contains("Email"))
                {
                    updates.Add(Builders<BsonDocument>.Update.Unset("Email"));
                }

                if (updates.Count > 0)
                {
                    var combinedUpdate = Builders<BsonDocument>.Update.Combine(updates);
                    await collection.UpdateOneAsync(
                        Builders<BsonDocument>.Filter.Eq("_id", user["_id"]),
                        combinedUpdate
                    );
                }
            }

            // Also drop any problematic indexes
            try
            {
                await collection.Indexes.DropOneAsync("Username_1");
                await collection.Indexes.DropOneAsync("Email_1");
            }
            catch { /* Ignore if indexes don't exist */ }

            return Ok(new { message = "Cleaned up duplicate fields and removed problematic indexes" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }
}