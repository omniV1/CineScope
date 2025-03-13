using CineScope.Server.Data;
using CineScope.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CineScope.Server.Controllers
{
    /// <summary>
    /// API controller for testing database connectivity.
    /// Used primarily for development and troubleshooting.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Reference to the MongoDB service for database operations.
        /// </summary>
        private readonly MongoDbService _mongoDbService;

        /// <summary>
        /// MongoDB settings from configuration.
        /// </summary>
        private readonly MongoDbSettings _settings;

        /// <summary>
        /// Initializes a new instance of the TestController.
        /// </summary>
        /// <param name="mongoDbService">Injected MongoDB service</param>
        /// <param name="options">Injected MongoDB settings</param>
        public TestController(MongoDbService mongoDbService, IOptions<MongoDbSettings> options)
        {
            _mongoDbService = mongoDbService;
            _settings = options.Value;
        }

        /// <summary>
        /// GET: api/Test
        /// Tests the MongoDB connection and returns collection statistics.
        /// </summary>
        /// <returns>Connection status and collection counts</returns>
        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Attempt to access the Users collection to verify database connectivity
                var collection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);

                // Count documents in the collection to verify read access
                var count = await collection.CountDocumentsAsync(Builders<User>.Filter.Empty);

                // Return success message with collection statistics
                return Ok(new
                {
                    Message = "MongoDB connection successful",
                    CollectionsCounts = new
                    {
                        Users = count,
                        Movies = count,
                        BannedWords = count,
                        Reviews = count
                    }
                });
            }
            catch (Exception ex)
            {
                // Return error details if connection fails
                return StatusCode(500, new { Error = $"MongoDB connection failed: {ex.Message}" });
            }
        }
    }
}