using CineScope.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CineScope.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MCPController : ControllerBase
    {
        private readonly ServerMCPService _mcpService;
        private readonly ILogger<MCPController> _logger;

        public MCPController(ServerMCPService mcpService, ILogger<MCPController> logger)
        {
            _mcpService = mcpService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint to get movie recommendations using MCP
        /// </summary>
        [HttpPost("recommendation")]
        public async Task<IActionResult> GetMovieRecommendation([FromBody] MovieQueryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest("Query cannot be empty");
            }

            try
            {
                _logger.LogInformation($"Received MCP movie recommendation request: {request.Query}");
                var response = await _mcpService.GetMovieRecommendation(request.Query);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MCP movie recommendation request");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Test endpoint to verify MCP server is working
        /// </summary>
        [HttpGet("test")]
        public async Task<IActionResult> TestMCP()
        {
            try
            {
                bool serverStarted = await _mcpService.StartMCPServer();
                if (serverStarted)
                {
                    return Ok(new { message = "MCP server started successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to start MCP server");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing MCP server");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Request model for movie queries
    /// </summary>
    public class MovieQueryRequest
    {
        public string Query { get; set; } = string.Empty;
    }
} 