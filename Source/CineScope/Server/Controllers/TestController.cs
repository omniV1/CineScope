using CineScope.Server.Data;
using CineScope.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CineScope.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;
        private readonly MongoDbSettings _settings;

        public TestController(MongoDbService mongoDbService, IOptions<MongoDbSettings> options)
        {
            _mongoDbService = mongoDbService;
            _settings = options.Value;
        }

        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Test MongoDB connection by getting Users collection
                var collection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);
                var count = await collection.CountDocumentsAsync(Builders<User>.Filter.Empty);

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
                return StatusCode(500, new { Error = $"MongoDB connection failed: {ex.Message}" });
            }
        }
    }
}