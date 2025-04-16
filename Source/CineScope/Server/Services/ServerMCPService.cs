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
using System.Linq;

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
        private readonly int _mcpServerPort = 5000; // Default port for MCP server

        public ServerMCPService(ILogger<ServerMCPService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("Anthropic");
            
            // Configure the HTTP client for Anthropic API
            _httpClient.BaseAddress = new Uri("https://api.anthropic.com/");
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Set a timeout of 30 seconds
            _httpClient.DefaultRequestHeaders.ConnectionClose = false; // Use persistent connections
            
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
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            }
            else
            {
                _logger.LogWarning("Anthropic API key not found in configuration or environment variables");
            }

            // Set the path to the MCP server
            _mcpServerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CineScope.MCPServer.dll");
            _logger.LogInformation($"MCP server path: {_mcpServerPath}");
        }

        private void KillExistingProcesses()
        {
            try
            {
                // Find and kill any existing dotnet processes that might be using our port
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C netstat -ano | findstr :{_mcpServerPort}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        _logger.LogWarning("Could not start process to check ports");
                        return;
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Extract PIDs from the output
                    var lines = output.Split('\n');
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 4)
                        {
                            if (int.TryParse(parts[4], out int pid))
                            {
                                try
                                {
                                    var processToKill = Process.GetProcessById(pid);
                                    if (processToKill.ProcessName.ToLower().Contains("dotnet"))
                                    {
                                        processToKill.Kill();
                                        _logger.LogInformation($"Killed process {pid} that was using port {_mcpServerPort}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Error killing process {pid}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up existing processes");
            }
        }

        /// <summary>
        /// Public method to start the MCP server
        /// </summary>
        public Task<bool> StartMCPServer()
        {
            return StartMCPServerAsync();
        }

        /// <summary>
        /// Starts the MCP server process if it's not already running
        /// </summary>
        private async Task<bool> StartMCPServerAsync()
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
                    
                    // Give it a moment to initialize
                    await Task.Delay(2000);
                    _isServerRunning = true;
                    _logger.LogInformation("MCP server started successfully.");
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

        private async Task WaitForServerReadyAsync()
        {
            using var httpClient = new HttpClient();
            int maxAttempts = 30;
            int currentAttempt = 0;

            while (currentAttempt < maxAttempts)
            {
                try
                {
                    var response = await httpClient.GetAsync($"http://localhost:{_mcpServerPort}/health");
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("MCP server is ready");
                        return;
                    }
                }
                catch
                {
                    // Ignore exceptions during startup checks
                }

                await Task.Delay(1000);
                currentAttempt++;
            }

            throw new TimeoutException("MCP server failed to start within the expected timeframe");
        }

        /// <summary>
        /// Stops the MCP server process if it's running
        /// </summary>
        public void StopMCPServer()
        {
            try
            {
                if (_mcpServerProcess != null)
                {
                    if (!_mcpServerProcess.HasExited)
                    {
                        _mcpServerProcess.Kill(entireProcessTree: true);
                        _mcpServerProcess.WaitForExit(5000); // Wait up to 5 seconds for graceful shutdown
                    }
                    _mcpServerProcess.Dispose();
                    _mcpServerProcess = null;
                }
                _isServerRunning = false;
                _logger.LogInformation("MCP server stopped.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping MCP server.");
            }
        }

        private string FilterResponse(string response)
        {
            // First pass: Remove any lines containing technical patterns
            var lines = response.Split('\n');
            var filteredLines = lines.Where(line =>
            {
                var trimmedLine = line.Trim().ToLowerInvariant();
                
                // Skip empty lines and technical content
                return !string.IsNullOrWhiteSpace(trimmedLine) &&
                       !ContainsTechnicalPatterns(trimmedLine);
            });
            
            // Join and clean up the text
            var result = string.Join("\n", filteredLines)
                .Replace("\n\n\n", "\n\n")
                .Trim();

            // Additional cleaning
            result = RemoveSearchPhrases(result);
            result = RemoveTechnicalArtifacts(result);

            return result;
        }

        private bool ContainsTechnicalPatterns(string line)
        {
            var patterns = new[]
            {
                "getmovieby", "searchmoviesby", "id:", "objectid",
                "database", "query", "let me", "searching",
                "looking up", "consulting", "checking",
                "*", "()", "[]", "{}", ">>>",
                "title:", "rating:", "summary:", "year:",
                "based on data", "according to", "shows that",
                "results show", "found in", "retrieved",
                "fetching", "loading", "processing"
            };

            return patterns.Any(pattern => line.ToLowerInvariant().Contains(pattern));
        }

        private string RemoveTechnicalArtifacts(string response)
        {
            // Remove any remaining technical artifacts
            response = System.Text.RegularExpressions.Regex.Replace(response, @"\*.*?\*", "");
            response = System.Text.RegularExpressions.Regex.Replace(response, @"\(id:.*?\)", "");
            response = System.Text.RegularExpressions.Regex.Replace(response, @"[<>{}()\[\]`]", "");
            response = System.Text.RegularExpressions.Regex.Replace(response, @"\s*\.\s*\.", ".");
            response = System.Text.RegularExpressions.Regex.Replace(response, @"\s+", " ");
            
            // Fix movie years that might have been affected
            response = System.Text.RegularExpressions.Regex.Replace(
                response, 
                @"(\d{4})",
                "($1)"
            );

            // Clean up spacing around parentheses
            response = System.Text.RegularExpressions.Regex.Replace(response, @"\s+\)", ")");
            response = System.Text.RegularExpressions.Regex.Replace(response, @"\(\s+", "(");
            
            return response.Trim();
        }

        /// <summary>
        /// Gets a response from Claude with MCP capabilities
        /// </summary>
        public async Task<string> GetResponseWithMCP(string userMessage, string? systemPrompt = null)
        {
            try
            {
                // Ensure MCP server is running
                bool mcpRunning = await StartMCPServerAsync();
                if (!mcpRunning)
                {
                    return "Error: Unable to start MCP server. Please try again.";
                }

                // Construct a more specific message that enforces formatting
                var processedMessage = $@"Format your response about {userMessage} following these EXACT rules:

1. Use numbered entries (1., 2., 3.)
2. Put movie titles in this format: Movie Title (YYYY)
3. Indent all paragraphs with exactly 3 spaces
4. Add TWO blank lines between each movie
5. Include TWO paragraphs per movie:
   - First paragraph: Plot and significance
   - Second paragraph: Achievements and impact
6. Keep paragraphs separated by ONE blank line

Example format:

1. The Movie Title (1994)
   First paragraph about the plot and significance of the movie. Make this 2-3 sentences long and focus on the story and its importance.

   Second paragraph about achievements and impact. Keep this to 1-2 sentences about awards or influence.


2. Another Movie (2000)
   First paragraph goes here with plot details and significance. Remember to maintain the exact spacing and formatting throughout.

   Second paragraph with achievements and impact. Keep the structure consistent.";

                // Get API key - check again in case it was not available at initialization
                if (!_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
                {
                    var apiKey = _configuration["AnthropicSettings:ApiKey"] 
                               ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
                    
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        _logger.LogWarning("No Anthropic API key provided. Service will not function properly.");
                        return "Error: Anthropic API key is not configured.";
                    }
                    
                    _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                }

                // Set required headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _configuration["AnthropicSettings:ApiKey"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY"));

                var messages = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        ["role"] = "user",
                        ["content"] = processedMessage
                    }
                };

                var requestBody = new Dictionary<string, object>
                {
                    ["model"] = "claude-3-sonnet-20240229",
                    ["max_tokens"] = 1024,
                    ["messages"] = messages,
                    ["temperature"] = 0.7
                };

                if (!string.IsNullOrEmpty(systemPrompt))
                {
                    requestBody["system"] = systemPrompt;
                }

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

                _logger.LogInformation("Sending request to Claude API with formatting instructions");
                var response = await _httpClient.PostAsync("v1/messages", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API Error: {response.StatusCode}, {errorContent}");
                    return $"Error calling Claude API: {response.StatusCode}";
                }

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
                        var fullResponse = textElement.GetString() ?? "Error: Empty response from Claude";
                        _logger.LogInformation("Raw Claude Response:\n{RawResponse}", fullResponse); // Log raw response
                        
                        // Apply simplified filtering and cleanup
                        var cleanedResponse = PostProcessResponse(fullResponse);
                        
                        _logger.LogInformation("Processed Response:\n{ProcessedResponse}", cleanedResponse); // Log processed response
                        return cleanedResponse;
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

        private string PostProcessResponse(string response)
        {
            // Normalize line endings
            response = response.Replace("\r\n", "\n").Replace("\r", "\n");

            // Basic cleanup: remove leading/trailing whitespace and collapse excessive newlines
            response = response.Trim();
            response = System.Text.RegularExpressions.Regex.Replace(response, @"\n{3,}", "\n\n"); // Collapse 3+ newlines to 2

            // Minimal filtering of technical artifacts - less aggressive than before
            response = System.Text.RegularExpressions.Regex.Replace(response, @"(?i)(let me search|searching for|looking up|querying).*?:\s*", "", System.Text.RegularExpressions.RegexOptions.Multiline);
            response = System.Text.RegularExpressions.Regex.Replace(response, @"(?i)(getmovieby|searchmoviesby|id:|objectid).*?\n", "", System.Text.RegularExpressions.RegexOptions.Multiline);
            response = System.Text.RegularExpressions.Regex.Replace(response, @"[*`<>\[\]{}]", ""); // Remove markdown/technical symbols - Fixed pattern

            return response.Trim();
        }

        private string RemoveSearchPhrases(string response)
        {
            // Minimal version or bypass
            return response;
        }

        /// <summary>
        /// Gets movie recommendations using Claude and MCP
        /// </summary>
        public async Task<string> GetMovieRecommendation(string userPreferences)
        {
            var systemPrompt = @"
You are a friendly movie expert having a natural conversation. Respond as if you're talking to a friend about movies.

FORMATTING REQUIREMENTS:
1. ALWAYS format movie entries EXACTLY like this:

1. [Movie Title] (Year)
   [First paragraph about the movie - 2-3 sentences about plot and significance]

   [Second paragraph about achievements/impact - 1-2 sentences]


2. [Movie Title] (Year)
   [First paragraph about the movie - 2-3 sentences about plot and significance]

   [Second paragraph about achievements/impact - 1-2 sentences]


3. [Movie Title] (Year)
   [First paragraph about the movie - 2-3 sentences about plot and significance]

   [Second paragraph about achievements/impact - 1-2 sentences]

CRITICAL FORMATTING RULES:
- Add EXACTLY TWO blank lines between each movie entry
- Indent all description paragraphs with 3 spaces
- Keep paragraphs separated by ONE blank line within each movie entry
- Format years in parentheses: (YYYY)
- Number each entry: 1., 2., 3.
- NEVER merge movies into a single paragraph
- NEVER use technical language or database terms
- NEVER mention searching or looking up information
- NEVER include IDs, ratings, or technical details

EXAMPLE OF CORRECT FORMATTING:

1. The Godfather (1972)
   Francis Ford Coppola's epic crime drama follows the Corleone family's rise in the criminal underworld. Marlon Brando delivers an unforgettable performance as Don Vito Corleone, while Al Pacino shines as his reluctant heir Michael.

   Winner of three Academy Awards including Best Picture, this masterpiece defined the gangster genre and is consistently ranked among the greatest films ever made.


2. Pulp Fiction (1994)
   Quentin Tarantino's groundbreaking crime film weaves together multiple storylines in non-linear fashion. The film revolutionized independent cinema with its sharp dialogue, eclectic soundtrack, and memorable characters.

   The movie earned Tarantino and Roger Avary an Oscar for Best Original Screenplay and revitalized John Travolta's career.";

            return await GetResponseWithMCP(userPreferences, systemPrompt);
        }

        private async Task<HttpResponseMessage> SendRequestWithRetryAsync(HttpRequestMessage request, int maxRetries = 3)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return await _httpClient.SendAsync(request);
                }
                catch (Exception ex) when (retryCount < maxRetries)
                {
                    retryCount++;
                    _logger.LogWarning(ex, $"Request failed. Retrying {retryCount}/{maxRetries}...");
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount))); // Exponential backoff
                }
            }
        }
    }
} 