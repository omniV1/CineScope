# The Movie Compatibility Predictor (MCP) Implementation

## Introduction

This document provides a narrative breakdown of the Movie Compatibility Predictor (MCP) implementation in the CineScope application. By intentionally naming this feature "Movie Compatibility Predictor" (MCP), we've created a clever reference to Anthropic's "Model Context Protocol" (MCP) framework that powers our recommendation system. 

As we explore each component, I'll walk through the actual code, explaining its purpose, design decisions, and how the pieces work together to create a sophisticated AI-powered movie recommendation system for users.

## The Architecture Story: How It All Fits Together

The MCP implementation follows a thoughtfully designed multi-tier architecture that separates concerns while maintaining efficient communication between components. At a high level, it looks like this:

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                 │     │                 │     │                 │
│   Client UI     │◄───►│   ASP.NET API   │◄───►│   MCP Server    │
│  (MCPDemo.razor)│     │ (MCPController) │     │  (.NET Process) │
│                 │     │                 │     │                 │
└─────────────────┘     └─────────────────┘     └────────┬────────┘
                                                         │
                                                         ▼
                                                ┌─────────────────┐
                                                │                 │
                                                │  Anthropic API  │
                                                │   (Claude AI)   │
                                                │                 │
                                                └─────────────────┘
```

When a user inputs a question or movie preference in the UI, this sets off a chain of events:

1. The Blazor UI component (`MCPDemo.razor`) captures the user input and sends it to the ASP.NET API endpoint
2. The API controller (`MCPController.cs`) validates the input and passes it to the server service
3. The server service (`ServerMCPService.cs`) ensures the MCP server is running, formats the query, and communicates with Claude's API
4. Claude's API processes the query and returns a response
5. The response travels back through the chain, being cleaned and formatted along the way
6. The UI displays the processed response to the user

Now, let's examine each component in detail, with code examples to illustrate how they work.

## The Server Service: The Heart of the MCP System

The `ServerMCPService.cs` file is where most of the magic happens. This service manages communication with Anthropic's Claude API, handles process management for the MCP server, and processes Claude's responses to ensure they're user-friendly.

### Initialization and Configuration

Let's look at how the service is initialized:

```csharp
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

## The Client Service: Connecting to the Server

The `AnthropicService.cs` file provides a client-side service that manages communication with the server and coordinates MCP operations. Let's examine its implementation:

```csharp
public class AnthropicService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AnthropicService> _logger;
    private readonly IConfiguration _configuration;
    private Process? _mcpServerProcess;
    private bool _isServerRunning;
    private readonly SemaphoreSlim _serverLock = new(1, 1);
    private readonly string _mcpServerEndpoint;

    public AnthropicService(
        HttpClient httpClient,
        ILogger<AnthropicService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _mcpServerEndpoint = _configuration["MCPServer:Endpoint"] ?? "http://localhost:5000";
        _isServerRunning = false;
    }

    public async Task<string> GetMovieRecommendationAsync(string userQuery)
    {
        try
        {
            await EnsureMCPServerRunningAsync();

            var request = new
            {
                Query = userQuery
            };

            var response = await _httpClient.PostAsJsonAsync($"{_mcpServerEndpoint}/api/mcp/query", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<MCPResponse>();
            return result?.Response ?? "I apologize, but I couldn't process your request at this time.";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting movie recommendation: {ex.Message}");
            return "I apologize, but there was an error processing your request. Please try again later.";
        }
    }

    private async Task EnsureMCPServerRunningAsync()
    {
        await _serverLock.WaitAsync();
        try
        {
            if (_isServerRunning && _mcpServerProcess?.HasExited == false)
            {
                return;
            }

            await StartMCPServerAsync();
        }
        finally
        {
            _serverLock.Release();
        }
    }

    private async Task StartMCPServerAsync()
    {
        try
        {
            string mcpServerPath = "../CineScope.MCPServer/bin/Debug/net8.0/CineScope.MCPServer.dll";
            _logger.LogInformation($"Starting MCP server from {mcpServerPath}");

            _mcpServerProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = mcpServerPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
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
            if (!started)
            {
                throw new Exception("Failed to start MCP server process");
            }

            _mcpServerProcess.BeginOutputReadLine();
            _mcpServerProcess.BeginErrorReadLine();

            // Wait for the server to be ready
            await WaitForServerReadyAsync();
            _isServerRunning = true;

            _logger.LogInformation("MCP server started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error starting MCP server: {ex.Message}");
            throw;
        }
    }

    private async Task WaitForServerReadyAsync()
    {
        int maxAttempts = 30;
        int currentAttempt = 0;
        int delayMs = 1000;

        while (currentAttempt < maxAttempts)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_mcpServerEndpoint}/health");
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

            await Task.Delay(delayMs);
            currentAttempt++;
        }

        throw new TimeoutException("MCP server failed to start within the expected timeframe");
    }

    public void Dispose()
    {
        try
        {
            if (_mcpServerProcess != null && !_mcpServerProcess.HasExited)
            {
                _mcpServerProcess.Kill();
                _mcpServerProcess.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error disposing MCP server process: {ex.Message}");
        }
    }

    private class MCPResponse
    {
        public string? Response { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}
```

The `AnthropicService` class provides client-side functionality for communicating with the MCP server. Like other components in the system, it receives its dependencies through constructor injection - an HTTP client for making API requests, a logger for monitoring and debugging, and configuration settings.

The service initializes the MCP server endpoint from configuration, with a default fallback to "http://localhost:5000". It also creates a semaphore (`_serverLock`) to ensure thread safety when starting the server.

The main public method, `GetMovieRecommendationAsync`, handles the process of getting movie recommendations based on user queries. It first ensures the MCP server is running by calling `EnsureMCPServerRunningAsync`, then creates a request object with the user's query.

The method then sends a POST request to the server's API endpoint and processes the response. If successful, it extracts the response content; if not, it provides a user-friendly error message. Exception handling ensures that any errors during this process are logged and translated into appropriate user messages.

The `EnsureMCPServerRunningAsync` method is responsible for checking if the server is running and starting it if necessary. It uses the semaphore to prevent concurrent server starts, which could lead to race conditions. The method checks if the server is already running and hasn't exited unexpectedly; if not, it calls `StartMCPServerAsync` to start the server.

The `StartMCPServerAsync` method handles the process of starting the MCP server. It's similar to the server-side implementation we saw earlier, but with some differences appropriate for the client side. The method creates a new process to run the MCP server DLL, configures output and error handling, and starts the process.

After starting the process, the method calls `WaitForServerReadyAsync` to wait for the server to become ready. If the server can't be started or doesn't become ready in time, the method throws an exception, which is caught and handled by the calling code.

The `WaitForServerReadyAsync` method implements a polling approach to determine when the server is ready, similar to what we saw in the server-side implementation. It repeatedly tries to call the server's health endpoint, with a delay between attempts. If the server responds successfully, the method returns; if not, it continues trying up to a maximum number of attempts before giving up.

Finally, the `Dispose` method ensures proper cleanup of resources when the service is no longer needed. It checks if the server process is still running and, if so, terminates it and releases the associated resources.

## Integration and Communication Flow

Now that we've examined each component individually, let's see how they work together to provide a seamless user experience. When a user interacts with the MCP system, the following sequence of events occurs:

1. The user enters a query in the `MCPDemo.razor` component's text field and clicks the "Get Help" button.

2. The `GetResponseAsync` method in the component is called, which:
   - Updates the UI to show loading indicators
   - Creates a request object with the user's query
   - Sends a POST request to the "api/MCP/recommendation" endpoint

3. The `GetMovieRecommendation` method in `MCPController.cs` receives the request, which:
   - Validates the request
   - Logs the incoming query
   - Calls the `GetMovieRecommendation` method in `ServerMCPService.cs`

4. The `GetMovieRecommendation` method in `ServerMCPService.cs`:
   - Creates a system prompt with detailed formatting instructions
   - Calls `GetResponseWithMCP` to communicate with Claude's API

5. The `GetResponseWithMCP` method:
   - Ensures the MCP server is running
   - Constructs a detailed prompt with the user's query and formatting instructions
   - Sends a request to Claude's API
   - Processes the response to remove technical artifacts
   - Returns the cleaned response

6. The response flows back through the call chain:
   - From `GetResponseWithMCP` to `GetMovieRecommendation` in `ServerMCPService.cs`
   - From there to `GetMovieRecommendation` in `MCPController.cs`
   - Then back to `GetResponseAsync` in `MCPDemo.razor`

7. The `GetResponseAsync` method in the Blazor component:
   - Updates the `Response` property with the received content
   - Clears the loading indicators
   - Triggers a UI update through `StateHasChanged()`

8. The component renders the response in a formatted card, displaying it to the user.

This flow demonstrates the clear separation of concerns in the architecture, with each component handling its specific responsibilities. The UI component manages user interaction, the controller handles API routing and validation, and the service provides the core business logic and external API communication.

## Advanced Topics in the MCP Implementation

### Prompt Engineering Techniques

One of the most fascinating aspects of the MCP implementation is the sophisticated prompt engineering used to guide Claude's responses. Let's examine the techniques in more detail:

1. **Explicit Formatting Instructions**: The system provides detailed, step-by-step instructions for how responses should be formatted, including numbering, indentation, spacing, and structure. This ensures consistent, readable outputs.

2. **Persona Definition**: The system prompt establishes Claude as "a friendly movie expert having a natural conversation," creating a specific tone and persona for interactions.

3. **Examples**: Concrete examples show exactly what the desired response format looks like, making it easier for Claude to understand the expectations.

4. **Negative Guidance**: The prompt includes explicit instructions about what NOT to do, preventing unwanted behaviors like using technical language or mentioning database operations.

5. **Content Structure**: The prompt specifies the content structure for each movie entry, with one paragraph about plot and significance and another about achievements and impact. This ensures comprehensive but concise information.

These techniques combine to create a reliable, consistent user experience despite the inherently variable nature of AI-generated responses.

### Error Handling and Resilience

The MCP implementation includes robust error handling at multiple levels, creating a resilient system that can recover from various failure modes:

1. **UI-Level Error Handling**: The Blazor component catches exceptions during API calls and provides user-friendly error messages without exposing implementation details.

2. **API-Level Error Handling**: The controller validates inputs, catches exceptions, and returns appropriate HTTP status codes, ensuring proper API behavior even when errors occur.

3. **Service-Level Error Handling**: The service includes comprehensive exception handling for process management, API communication, and response processing, with detailed logging for troubleshooting.

4. **Process Management**: The system carefully manages the MCP server process, with health checks, timeout handling, and proper resource cleanup, preventing resource leaks and ensuring availability.

5. **Fallback Behavior**: When errors occur, the system provides fallback responses rather than failing completely, maintaining a better user experience even during partial failures.

This multi-layered approach to error handling creates a system that's resilient to various failure modes, from network issues to API errors to process management problems.

### Performance Considerations

The MCP implementation includes several performance optimizations:

1. **Connection Pooling**: The HTTP client is configured with `ConnectionClose = false`, allowing connection reuse for improved performance.

2. **Process Reuse**: The MCP server process is started once and reused for multiple requests, avoiding the overhead of process creation for each query.

3. **Asynchronous Operations**: All I/O operations use the async/await pattern, preventing thread blocking and improving scalability.

4. **Timeout Management**: Various operations include appropriate timeouts to prevent hanging in case of failures.

5. **Resource Cleanup**: The system ensures proper cleanup of resources when they're no longer needed, preventing memory leaks and resource exhaustion.

These optimizations ensure that the MCP system can handle multiple concurrent users efficiently, with responsive performance even under load.

## Best Practices Demonstrated

The MCP implementation demonstrates several software development best practices:

1. **Separation of Concerns**: Each component has a clear, focused responsibility, from UI interaction to API routing to business logic to external communication.

2. **Dependency Injection**: Components receive their dependencies through constructor injection, promoting loose coupling and testability.

3. **Comprehensive Logging**: The system includes detailed logging at all levels, facilitating monitoring, debugging, and troubleshooting.

4. **Error Handling**: Robust error handling ensures the system can recover from various failure modes without crashing or exposing implementation details.

5. **Resource Management**: Careful management of resources, including processes, connections, and memory, prevents leaks and ensures stability.

6. **Input Validation**: The system validates inputs before processing, rejecting invalid requests early and providing clear error messages.

7. **Code Reuse**: Common functionality is encapsulated in methods that can be reused in different contexts, reducing duplication and improving maintainability.

8. **Asynchronous Programming**: Consistent use of async/await for I/O operations improves performance and scalability.

These practices combine to create a well-structured, maintainable, and reliable system that can evolve over time as requirements change.

## Potential Future Enhancements

The MCP implementation is already sophisticated, but several potential enhancements could further improve it:

1. **Response Caching**: Implementing a caching mechanism for frequently asked queries could improve performance and reduce API costs.

2. **Stream Processing**: Implementing streaming responses could improve perceived performance by showing partial results as they become available.

3. **User Feedback Collection**: Adding a mechanism for users to provide feedback on recommendations could help improve the system over time.

4. **Personalized Recommendations**: Integrating with user profiles and viewing history could enable more personalized recommendations.

5. **Multi-Modal Support**: Extending the system to include images, trailers, or other media could enhance the user experience.

6. **Metrics and Monitoring**: Adding more comprehensive metrics and monitoring could help identify performance issues and optimization opportunities.

7. **A/B Testing**: Implementing an A/B testing framework could help evaluate different prompt structures and system configurations.

These enhancements could build on the solid foundation of the current implementation to create an even more powerful and user-friendly system.

## Frequently Asked Questions

### How does the MCP name reflect the dual meaning in the implementation?

The Movie Compatibility Predictor (MCP) was intentionally named to create a dual meaning with Anthropic's Model Context Protocol (MCP). This clever naming reflects how the system leverages Anthropic's protocol techniques to power movie recommendations, creating a perfect alignment between the feature name and its underlying technology.

### How does the system ensure consistent, user-friendly responses?

The system uses sophisticated prompt engineering techniques, including detailed formatting instructions, persona definition, examples, and negative guidance. These techniques guide Claude's responses to follow a consistent format that's easy for users to read and understand. Additionally, the response processing methods clean up any remaining technical artifacts or formatting issues.

### What happens if the MCP server fails to start?

The system includes robust error handling for server startup failures. If the server can't be started, appropriate error messages are logged, and user-friendly error messages are returned to the client. The UI shows appropriate error states, and the system can recover automatically during subsequent requests.

### How does the system balance between specificity and flexibility in recommendations?

The system strikes a balance through careful prompt engineering. The temperature setting of 0.7 in the Claude API request allows for some creativity while maintaining consistency. The prompt provides specific formatting requirements but leaves room for varied content within that structure. This approach ensures that recommendations are tailored to the user's query while maintaining a consistent, readable format.

### What security measures are implemented in the MCP system?

The system includes several security measures, including API key management (retrieving keys from configuration or environment variables rather than hardcoding them), input validation (rejecting empty or invalid queries), authorization (requiring authentication for the UI component through the `[Authorize]` attribute), and error information hiding (providing generic error messages to users while logging detailed information internally).

### How does the MCP system handle concurrent users?

The system is designed to handle multiple concurrent users efficiently. It uses the async/await pattern for non-blocking I/O operations, maintains a single MCP server process that can handle multiple requests, uses connection pooling for HTTP connections, and implements proper synchronization through semaphores when starting the server. These measures ensure that the system can scale to handle multiple users without performance degradation or resource contention.

## Conclusion

The Movie Compatibility Predictor (MCP) implementation in CineScope demonstrates a sophisticated integration of AI capabilities into a web application. By leveraging Anthropic's Claude API with careful prompt engineering, process management, and response processing, the system provides personalized movie recommendations in a consistent, user-friendly format.

The implementation follows sound architectural principles and software development best practices, creating a system that's not only powerful but also maintainable, scalable, and resilient. The dual meaning of MCP reflects the clever alignment of feature name and underlying technology, creating a cohesive narrative for the system.

As demonstrated throughout this guide, every aspect of the implementation—from the UI component to the API controller to the service layer to the external API communication—has been carefully designed and implemented to create a seamless user experience. The code reflects a deep understanding of both the technical aspects of AI integration and the user experience considerations necessary for a successful implementation.

The MCP system serves as an excellent example of how modern AI capabilities can be integrated into web applications to provide valuable functionality for users while maintaining performance, security, and maintainability.

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
```

This constructor sets up everything the service needs to function properly. First, it takes in dependencies through constructor injection - a logger for tracking operations, configuration for settings, and an HTTP client factory for making API calls.

The HTTP client is configured specifically for communicating with Anthropic's API. We set a 30-second timeout to prevent requests from hanging indefinitely, and we configure persistent connections to improve performance by reusing TCP connections rather than creating new ones for each request.

The API key is retrieved using a fallback approach - first checking application configuration, then environment variables. This flexible approach accommodates different deployment scenarios and security practices. The API key is then added to the default headers along with the required API version.

Finally, the path to the MCP server executable is determined based on the application's base directory. This path will be used later when starting the MCP server process.

### Managing the MCP Server Process

The service includes methods for managing the lifecycle of the MCP server process. Let's look at the method for starting the server:

```csharp
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
```

This method first checks if the server is already running by examining the `_isServerRunning` flag and verifying that the process still exists and hasn't exited. This prevents unnecessary restarts and resource usage.

If the server isn't running, the method creates a new process configured to run the MCP server DLL using the dotnet runtime. The process is configured to run without a visible window and with redirected I/O streams, allowing the service to capture and log its output.

Event handlers are attached to the output and error streams, ensuring that all server output is properly logged for monitoring and debugging. This becomes invaluable when troubleshooting issues with the MCP server.

After starting the process, the method waits for a short period to allow the server to initialize before considering it ready. The server's running state is then updated, and the method returns a success indication.

Comprehensive error handling ensures that problems during server startup don't crash the main application. Instead, errors are logged, and a failure indication is returned.

To ensure the server is fully ready before use, the service includes a method to wait for the server to respond to health checks:

```csharp
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
```

This method implements a polling approach to determine when the server is ready. It repeatedly attempts to call the server's health endpoint, with a one-second delay between attempts. If the server responds successfully, the method returns, indicating the server is ready. If the maximum number of attempts is reached without a successful response, the method throws a timeout exception.

The try-catch block inside the loop is important - during startup, the server might not be listening yet, causing connection exceptions. These exceptions are expected and ignored during the polling phase.

When the server is no longer needed, it's important to properly shut it down:

```csharp
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
```

This method ensures proper cleanup of the server process. If the process is still running, it's terminated along with any child processes it might have started (`entireProcessTree: true`). The method then waits up to 5 seconds for the process to exit gracefully before proceeding.

After the process has exited, the method disposes of the process object to release system resources, nulls the reference to allow garbage collection, and updates the server status flag. Any errors during this process are caught and logged, preventing them from affecting the main application.

### Communicating with Claude's API

The core functionality of the MCP service is communicating with Claude's API to get movie recommendations. Let's look at the method that handles this:

```csharp
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
```

This method ties together many aspects of the MCP system. It starts by ensuring the MCP server is running, returning an error message if the server can't be started.

One of the most fascinating aspects of this implementation is the prompt engineering used to guide Claude's responses. The method constructs a detailed message with specific formatting instructions, including rules for numbering entries, formatting movie titles, indentation, spacing, and content structure. These instructions ensure that Claude's responses are consistently formatted and easy to read.

The method also includes an example of the desired format, showing exactly how movie entries should be structured. This example-based guidance is a powerful technique in prompt engineering, helping the AI model understand exactly what's expected.

Before sending the request, the method performs a final check for the API key and sets up the necessary headers. The request body includes the model to use (claude-3-sonnet-20240229), a token limit to control response length, the messages array containing the user's query, and a temperature setting of 0.7, which balances creativity with consistency.

If a system prompt was provided (e.g., additional instructions or context), it's added to the request body. The request is then serialized to JSON using proper camel-case naming policy for compatibility with the API.

After sending the request to Claude's API, the method checks for a successful response and processes the result. The response is parsed from JSON, and the text content is extracted from the first content item.

Finally, the raw response is logged and then passed through a post-processing method to clean up any artifacts or formatting issues before being returned to the caller.

### Processing Claude's Responses

An important aspect of the MCP implementation is how it processes Claude's responses to ensure they're user-friendly. Let's look at the post-processing method:

```csharp
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
```

This method performs several important cleanup operations on Claude's responses. First, it normalizes line endings to ensure consistent processing regardless of the platform (Windows vs. Unix).

Next, it removes leading and trailing whitespace and collapses excessive newlines to maintain clean, consistent formatting. The regex pattern `\n{3,}` matches three or more consecutive newlines and replaces them with exactly two newlines, which corresponds to the desired format for separating movie entries.

The method then filters out technical artifacts that might appear in Claude's responses. This includes "thinking out loud" phrases like "let me search" or "looking up," as well as database-related terms like "getmovieby" or "id:". These filters help maintain the illusion that the system understands movies directly, rather than revealing the underlying query mechanisms.

Finally, the method removes markdown and technical symbols that might interfere with the presentation of the response, and performs a final trim to remove any remaining leading or trailing whitespace.

The service also includes a method specifically for providing movie recommendations:

```csharp
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
```

This method provides a specialized interface for getting movie recommendations. It defines a detailed system prompt that instructs Claude to respond as a friendly movie expert having a natural conversation. The prompt includes comprehensive formatting requirements and examples, ensuring that the recommendations follow a consistent, user-friendly format.

The system prompt also includes "negative guidance" - explicit instructions about what NOT to do. This includes never merging movies into a single paragraph, never using technical language or database terms, never mentioning searching or looking up information, and never including IDs, ratings, or technical details. This negative guidance helps prevent Claude from breaking character or revealing implementation details.

The method then calls the general `GetResponseWithMCP` method, passing the user's preferences and the system prompt. This reuse of the existing method maintains code efficiency while providing specialized functionality.

## The API Controller: Bridging UI and Services

The `MCPController.cs` file defines the API endpoints that the client UI uses to access MCP functionality. Let's examine its implementation:

```csharp
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
```

The controller is decorated with the `[ApiController]` and `[Route("api/[controller]")]` attributes, which designate it as a controller for handling API requests and define its base route as "api/MCP".

The controller receives its dependencies through constructor injection - the ServerMCPService for processing requests and a logger for monitoring and debugging.

The main endpoint is `GetMovieRecommendation`, which accepts a POST request to "api/MCP/recommendation" with a JSON body containing a query. The method first validates that the query isn't empty or whitespace, returning a 400 Bad Request response if it is.

If the query is valid, the method logs the request and calls the MCP service's `GetMovieRecommendation` method, passing the query. The response is then wrapped in an anonymous object and returned as a 200 OK response.

Comprehensive error handling ensures that exceptions are caught, logged, and translated into appropriate HTTP responses for the client. This prevents internal errors from leaking implementation details to the client while providing useful error information.

The controller also includes a test endpoint that can be used to verify that the MCP server can be started successfully. This is valuable for diagnostic and monitoring purposes.

Finally, the controller defines a simple request model, `MovieQueryRequest`, which encapsulates the query string for movie recommendations. This model uses auto-property initialization (`= string.Empty`) to avoid null references.

## The User Interface: Bringing MCP to Life

The `MCPDemo.razor` file defines the Blazor component that provides the user interface for interacting with the MCP system. Let's examine its implementation:

```csharp
@page "/mcp"
@attribute [Authorize]
@using Microsoft.AspNetCore.Components.Authorization
@using CineScope.Client.Services
@using CineScope.Shared.DTOs
@inject AnthropicService AnthropicService
@inject ISnackbar Snackbar
@using System.Net.Http.Json
@inject HttpClient Http
@inject ILogger<MCPDemo> Logger

<MudContainer MaxWidth="MaxWidth.Large" Class="mt-6">
    <MudText Typo="Typo.h3" Align="Align.Left" GutterBottom="true">Help & FAQ</MudText>
    <MudText Typo="Typo.subtitle1" Align="Align.Left" Class="mb-8">Get personalized help with all your movie-related questions</MudText>

    <MudGrid>
        <MudItem xs="12" md="8">
            <MudPaper Elevation="3" Class="pa-4 mb-4">
                <MudTextField @bind-Value="UserInput" Label="Ask me anything about movies..." Variant="Variant.Outlined" 
                            FullWidth="true" Immediate="true" Lines="3" />
                <MudButton Variant="Variant.Filled" Color="Color.Primary" FullWidth="true" 
                        OnClick="GetResponseAsync" Class="mt-4" Disabled="@IsLoading">
                    @if (IsLoading)
                    {
                        <MudProgressCircular Class="ms-n1" Size="Size.Small" Indeterminate="true" />
                        <MudText Class="ms-2">Thinking...</MudText>
                    }
                    else
                    {
                        <MudIcon Icon="@Icons.Material.Filled.QuestionAnswer" Class="mr-2" />
                        <MudText>Get Help</MudText>
                    }
                </MudButton>
            </MudPaper>

            @if (IsServerStarting)
            {
                <MudAlert Severity="Severity.Info" Class="mb-4">
                    <MudText>Preparing to help you, please wait...</MudText>
                </MudAlert>
            }

            @if (!string.IsNullOrEmpty(Response))
            {
                <MudPaper Elevation="3" Class="pa-4 mb-4">
                    <MudText Typo="Typo.h5" Class="mb-4">Answer:</MudText>
                    <MudText Style="white-space: pre-line; font-family: var(--mud-typography-default-family);">@Response</MudText>
                </MudPaper>
            }
        </MudItem>

        <MudItem xs="12" md="4">
            <MudCard>
                <MudCardHeader>
                    <CardHeaderContent>
                        <MudText Typo="Typo.h6">Common Questions</MudText>
                    </CardHeaderContent>
                </MudCardHeader>
                <MudCardContent>
                    <MudList Clickable="true">
                        <MudListItem OnClick="@(() => SetQuestion("What are the top 3 highest rated movies?"))">
                            <MudText>What are the top rated movies?</MudText>
                        </MudListItem>
                        <MudListItem OnClick="@(() => SetQuestion("Can you recommend some good drama movies?"))">
                            <MudText>What drama movies would you recommend?</MudText>
                        </MudListItem>
                        <MudListItem OnClick="@(() => SetQuestion("Tell me about The Godfather"))">
                            <MudText>Tell me about The Godfather</MudText>
                        </MudListItem>
                        <MudListItem OnClick="@(() => SetQuestion("What fantasy movies are available?"))">
                            <MudText>What fantasy movies are available?</MudText>
                        </MudListItem>
                        <MudListItem OnClick="@(() => SetQuestion("Which movies are similar to The Dark Knight?"))">
                            <MudText>Find movies similar to The Dark Knight</MudText>
                        </MudListItem>
                    </MudList>
                </MudCardContent>
            </MudCard>

            <MudExpansionPanels Class="mt-4">
                <MudExpansionPanel Text="How does this work?">
                    <MudText>
                        <p>
                            This help system uses Claude, an advanced AI, to assist you with movie-related questions. 
                            It has direct access to our movie database, ensuring you get accurate and up-to-date information.
                        </p>
                        <p>
                            You can ask about:
                        </p>
                        <MudList Dense="true">
                            <MudListItem Icon="@Icons.Material.Filled.Search">Finding specific movies</MudListItem>
                            <MudListItem Icon="@Icons.Material.Filled.Recommend">Getting recommendations</MudListItem>
                            <MudListItem Icon="@Icons.Material.Filled.Category">Exploring genres</MudListItem>
                            <MudListItem Icon="@Icons.Material.Filled.Star">Finding top-rated films</MudListItem>
                        </MudList>
                    </MudText>
                </MudExpansionPanel>
            </MudExpansionPanels>
        </MudItem>
    </MudGrid>
</MudContainer>

@code {
    private string UserInput { get; set; } = "";
    private string Response { get; set; } = "";
    private bool IsLoading { get; set; } = false;
    private bool IsServerStarting { get; set; } = false;

    /// <summary>
    /// Sets a sample question in the input field
    /// </summary>
    private void SetQuestion(string question)
    {
        UserInput = question;
        StateHasChanged();
    }

    /// <summary>
    /// Gets a response from Claude using MCP via server API
    /// </summary>
    private async Task GetResponseAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput))
            return;

        IsLoading = true;
        IsServerStarting = true;
        Response = "";
        StateHasChanged();

        try
        {
            var request = new { Query = UserInput };
            var response = await Http.PostAsJsonAsync("api/MCP/recommendation", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<MCPResponse>();
                if (result != null)
                {
                    Response = result.Response;
                }
                else
                {
                    Response = "I apologize, but I couldn't process your request at the moment. Please try again.";
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Logger.LogError($"Error from MCP API: {response.StatusCode}, {errorContent}");
                Response = "I apologize, but I'm having trouble accessing the movie database right now. Please try again in a moment.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting response from MCP API");
            Response = "I apologize, but something went wrong. Please try again later.";
        }
        finally
        {
            IsLoading = false;
            IsServerStarting = false;
            StateHasChanged();
        }
    }

    private class MCPResponse
    {
        public string Response { get; set; } = string.Empty;
    }
}
