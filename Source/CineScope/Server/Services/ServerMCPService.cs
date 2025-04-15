using System;
using System.Diagnostics;
using System.Text.Json;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CineScope.Server.Services
{
    public class ServerMCPService
    {
        private readonly ILogger<ServerMCPService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private Process? _mcpServerProcess;
        private readonly string _mcpServerPath;
        private bool _isServerRunning = false;

        public ServerMCPService(ILogger<ServerMCPService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("Anthropic");
            
            // Configure the HTTP client for Anthropic API
            _httpClient.BaseAddress = new Uri("https://api.anthropic.com/");
            
            // Get API key from configuration
            var apiKey = _configuration["AnthropicSettings:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                // Try to get from environment variable as fallback
                apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            }

            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2024-01-31");
            }
            else
            {
                _logger.LogWarning("Anthropic API key not found in configuration or environment variables");
            }

            // Set the path to the MCP server
            _mcpServerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CineScope.MCPServer.dll");
            _logger.LogInformation($"MCP server path: {_mcpServerPath}");
        }

        /// <summary>
        /// Starts the MCP server process if it's not already running
        /// </summary>
        public async Task<bool> StartMCPServer()
        {
            if (_isServerRunning && _mcpServerProcess != null && !_mcpServerProcess.HasExited)
            {
                _logger.LogInformation("MCP server is already running.");
                return true;
            }

            try
            {
                _logger.LogInformation($"Starting MCP server from {_mcpServerPath}");
                
                // Start the MCP server process
                _mcpServerProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = _mcpServerPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = true
                    }
                };
                
                _mcpServerProcess.OutputDataReceived += (sender, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _logger.LogInformation($"MCP Server: {e.Data}");
                    }
                };
                
                _mcpServerProcess.ErrorDataReceived += (sender, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _logger.LogError($"MCP Server Error: {e.Data}");
                    }
                };
                
                bool started = _mcpServerProcess.Start();
                if (started)
                {
                    _mcpServerProcess.BeginOutputReadLine();
                    _mcpServerProcess.BeginErrorReadLine();
                    
                    _logger.LogInformation("MCP server started successfully.");
                    
                    // Give it a moment to initialize
                    await Task.Delay(2000);
                    _isServerRunning = true;
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to start MCP server.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting MCP server.");
                return false;
            }
        }

        /// <summary>
        /// Stops the MCP server process if it's running
        /// </summary>
        public void StopMCPServer()
        {
            if (_mcpServerProcess != null && !_mcpServerProcess.HasExited)
            {
                try
                {
                    _mcpServerProcess.Kill();
                    _mcpServerProcess.Dispose();
                    _mcpServerProcess = null;
                    _isServerRunning = false;
                    _logger.LogInformation("MCP server stopped.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping MCP server.");
                }
            }
        }

        /// <summary>
        /// Gets a response from Claude with MCP capabilities
        /// </summary>
        public async Task<string> GetResponseWithMCP(string userMessage, string? systemPrompt = null)
        {
            try
            {
                // Ensure MCP server is running
                bool mcpRunning = await StartMCPServer();
                if (!mcpRunning)
                {
                    return "Error: Unable to start MCP server. Please try again.";
                }

                // Get API key - check again in case it was not available at initialization
                if (!_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
                {
                    var apiKey = _configuration["AnthropicSettings:ApiKey"] 
                               ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
                    
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        _logger.LogWarning("No Anthropic API key provided. Service will not function properly.");
                        return "Error: Anthropic API key is not configured. Please set the API key using .NET User Secrets (in development) or environment variables (in production).";
                    }
                    
                    _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                }

                // Set required headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2024-01-31");
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _configuration["AnthropicSettings:ApiKey"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY"));

                var messages = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        ["role"] = "user",
                        ["content"] = userMessage
                    }
                };

                var requestBody = new Dictionary<string, object>
                {
                    ["model"] = "claude-3-sonnet-20240229",
                    ["max_tokens"] = 1024,
                    ["messages"] = messages
                };

                if (!string.IsNullOrEmpty(systemPrompt))
                {
                    requestBody["system"] = systemPrompt;
                }

                // Convert the request body to JSON with proper settings
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
                
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody, jsonOptions),
                    Encoding.UTF8,
                    "application/json"
                );

                // Log the request body for debugging
                _logger.LogInformation($"Request body: {await jsonContent.ReadAsStringAsync()}");

                // Make the API call to the messages endpoint
                var response = await _httpClient.PostAsync("v1/messages", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API Error: {response.StatusCode}, {errorContent}");
                    return $"Error calling Claude API: {response.StatusCode}";
                }

                // Parse the response
                var responseJson = await response.Content.ReadFromJsonAsync<JsonDocument>();
                if (responseJson == null)
                {
                    return "Error: Invalid response from Claude API";
                }

                var root = responseJson.RootElement;
                if (root.TryGetProperty("content", out var contentArray) &&
                    contentArray.GetArrayLength() > 0)
                {
                    var firstContent = contentArray[0];
                    if (firstContent.TryGetProperty("text", out var textElement))
                    {
                        return textElement.GetString() ?? "Error: Empty response from Claude";
                    }
                }

                return "Error: Unable to parse Claude's response content";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Claude API with MCP");
                return $"An error occurred: {ex.Message}";
            }
        }

        /// <summary>
        /// Gets movie recommendations using Claude and MCP
        /// </summary>
        public async Task<string> GetMovieRecommendation(string userPreferences)
        {
            var systemPrompt = @"
You are a movie recommendation expert with access to a comprehensive movie database through function calls.

You have access to the following tools:
- SearchMoviesByTitle(title): Search for movies by title
- GetMovieById(id): Get detailed information about a movie by its ID

Always use these tools to provide accurate movie information rather than relying on your general knowledge.
Present your recommendations in a friendly, conversational way.
";
            return await GetResponseWithMCP(userPreferences, systemPrompt);
        }
    }
} 