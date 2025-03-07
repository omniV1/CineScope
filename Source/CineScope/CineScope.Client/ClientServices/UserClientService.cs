using System.Net.Http.Json;
using CineScope.Shared.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CineScope.Client.ClientServices
{
    /// <summary>
    /// Client service for user authentication and account management in Blazor WebAssembly
    /// Handles communication with user-related API endpoints
    /// </summary>
    public class UserClientService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Constructor for UserClientService
        /// </summary>
        /// <param name="httpClient">Configured HttpClient for API communication</param>
        public UserClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            Console.WriteLine($"UserClientService initialized with BaseAddress: {_httpClient.BaseAddress}");
        }

        /// <summary>
        /// Returns the base address of the configured HttpClient
        /// Useful for diagnostic purposes
        /// </summary>
        /// <returns>The base URL for API requests</returns>
        public string GetBaseAddress()
        {
            return _httpClient.BaseAddress?.ToString() ?? "No base address set";
        }

        /// <summary>
        /// Retrieves user information by ID
        /// </summary>
        /// <param name="id">The ID of the user to retrieve</param>
        /// <returns>User data if found, null otherwise</returns>
        public async Task<UserModel?> GetUserAsync(string id)
        {
            try
            {
                Console.WriteLine($"GetUserAsync: Calling api/users/{id}");
                return await _httpClient.GetFromJsonAsync<UserModel>($"api/users/{id}");
            }
            catch (Exception ex)
            {
                // Log error and return null to indicate failure
                Console.WriteLine($"GetUserAsync Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Authenticates a user with username and password
        /// </summary>
        /// <param name="username">Username for login</param>
        /// <param name="password">Password for login</param>
        /// <returns>Login response containing success status and user information if successful</returns>
        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            try
            {
                Console.WriteLine($"LoginAsync: Calling api/users/login with username: {username}");

                // Create the login model with credentials
                var loginModel = new LoginModel
                {
                    Username = username,
                    Password = password
                };

                Console.WriteLine($"Sending login request to: {_httpClient.BaseAddress}api/users/login");

                // Send the POST request with credentials
                var response = await _httpClient.PostAsJsonAsync("api/users/login", loginModel);

                Console.WriteLine($"Login response status: {response.StatusCode}");

                // Read the response content as string for debugging and parsing
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login response content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        // Use case-insensitive property matching to handle API response variations
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        // Parse the response content manually for more control over the structure
                        using (JsonDocument document = JsonDocument.Parse(responseContent))
                        {
                            JsonElement root = document.RootElement;

                            var loginResponse = new LoginResponse();

                            // Check for success property in the response
                            if (root.TryGetProperty("success", out JsonElement successElement))
                            {
                                loginResponse.Success = successElement.GetBoolean();

                                // If successful, extract user information
                                if (loginResponse.Success && root.TryGetProperty("user", out JsonElement userElement))
                                {
                                    var userInfo = new UserInfo
                                    {
                                        // Extract all user fields with null checking
                                        Id = userElement.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? string.Empty : string.Empty,
                                        Username = userElement.TryGetProperty("username", out var usernameElement) ? usernameElement.GetString() ?? string.Empty : string.Empty,
                                        Email = userElement.TryGetProperty("email", out var emailElement) ? emailElement.GetString() ?? string.Empty : string.Empty,
                                        Roles = userElement.TryGetProperty("roles", out var rolesElement) && rolesElement.ValueKind == JsonValueKind.Array
                                            ? JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText(), options) ?? new List<string>()
                                            : new List<string>()
                                    };

                                    loginResponse.User = userInfo;
                                    loginResponse.Message = "Login successful";
                                }
                                else if (root.TryGetProperty("message", out JsonElement messageElement))
                                {
                                    // Extract error message if present
                                    loginResponse.Message = messageElement.GetString() ?? "Unknown error";
                                }
                            }
                            else
                            {
                                // Fallback deserialization if the response format doesn't match expectations
                                var deserializedResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, options);
                                if (deserializedResponse != null)
                                {
                                    loginResponse = deserializedResponse;
                                }
                                else
                                {
                                    // If all else fails, assume success but note the unexpected format
                                    loginResponse.Success = true;
                                    loginResponse.Message = "Login successful but response format was unexpected";
                                }
                            }

                            Console.WriteLine($"Login success: {loginResponse.Success}, Message: {loginResponse.Message}");
                            return loginResponse;
                        }
                    }
                    catch (JsonException ex)
                    {
                        // Handle JSON parsing errors
                        Console.WriteLine($"JSON deserialization error: {ex.Message}");
                        return new LoginResponse
                        {
                            Success = false,
                            Message = $"Error processing server response: {ex.Message}"
                        };
                    }
                }
                else
                {
                    // Handle unsuccessful HTTP response
                    Console.WriteLine($"Login failed with status code: {response.StatusCode}");
                    return new LoginResponse
                    {
                        Success = false,
                        Message = $"Login failed with status code: {response.StatusCode}. {responseContent}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle network/HTTP errors
                Console.WriteLine($"HTTP request error during login: {ex.Message}");
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                Console.WriteLine($"Unexpected error during login: {ex.Message}");
                return new LoginResponse
                {
                    Success = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="username">Username for new account</param>
        /// <param name="email">Email address for new account</param>
        /// <param name="password">Password for new account</param>
        /// <returns>Registration result with success status and user information if successful</returns>
        public async Task<RegisterResponse?> RegisterAsync(string username, string email, string password)
        {
            try
            {
                // Create the registration request model
                var registerModel = new
                {
                    Username = username,
                    Email = email,
                    Password = password
                };

                // Log request details for debugging
                Console.WriteLine($"RegisterAsync: Full request URL: {_httpClient.BaseAddress}api/users/register");
                Console.WriteLine($"RegisterAsync: Sending request with username: {username}, email: {email}");

                // Use manual JSON serialization for more control over the request
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(registerModel),
                    System.Text.Encoding.UTF8,
                    "application/json");

                // Send the HTTP request
                var response = await _httpClient.PostAsync("api/users/register", jsonContent);
                Console.WriteLine($"RegisterAsync: Received status code {response.StatusCode}");

                // Read the response content for processing
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"RegisterAsync: Raw response content: {content}");

                // Create response object to return to the caller
                var registerResponse = new RegisterResponse();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        // Parse the response JSON
                        using (JsonDocument document = JsonDocument.Parse(content))
                        {
                            JsonElement root = document.RootElement;

                            // Check for success flag in response
                            if (root.TryGetProperty("success", out JsonElement successElement))
                            {
                                registerResponse.Success = successElement.GetBoolean();

                                // If registration successful, extract user information
                                if (registerResponse.Success && root.TryGetProperty("user", out JsonElement userElement))
                                {
                                    var userInfo = new UserInfo
                                    {
                                        // Extract all user fields with null checking
                                        Id = userElement.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? string.Empty : string.Empty,
                                        Username = userElement.TryGetProperty("username", out var usernameElement) ? usernameElement.GetString() ?? string.Empty : string.Empty,
                                        Email = userElement.TryGetProperty("email", out var emailElement) ? emailElement.GetString() ?? string.Empty : string.Empty,
                                        Roles = userElement.TryGetProperty("roles", out var rolesElement) && rolesElement.ValueKind == JsonValueKind.Array
                                            ? JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText()) ?? new List<string>()
                                            : new List<string>()
                                    };

                                    registerResponse.User = userInfo;
                                    registerResponse.Message = "Registration successful";
                                }
                                else if (root.TryGetProperty("message", out JsonElement messageElement))
                                {
                                    // Extract error message if present
                                    registerResponse.Message = messageElement.GetString() ?? "Registration failed";
                                }
                                else
                                {
                                    registerResponse.Message = "Registration failed with unknown error";
                                }
                            }
                            else if (root.TryGetProperty("message", out JsonElement messageElement))
                            {
                                // Alternative response format handling
                                registerResponse.Success = false;
                                registerResponse.Message = messageElement.GetString() ?? "Registration failed";
                            }
                            else
                            {
                                // If all else fails, assume success but note the unexpected format
                                registerResponse.Success = true;
                                registerResponse.Message = "Registration successful but response format was unexpected";
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        // Handle JSON parsing errors
                        Console.WriteLine($"RegisterAsync: Error parsing JSON response: {ex.Message}");
                        registerResponse.Success = true;
                        registerResponse.Message = "Registration successful but could not parse response data";
                    }
                }
                else
                {
                    // Handle unsuccessful HTTP response
                    registerResponse.Success = false;

                    try
                    {
                        // Try to extract error message from response
                        using (JsonDocument document = JsonDocument.Parse(content))
                        {
                            JsonElement root = document.RootElement;
                            registerResponse.Message = root.TryGetProperty("message", out JsonElement messageElement) ?
                                                  messageElement.GetString() ?? $"Registration failed: {response.StatusCode}" :
                                                  $"Registration failed: {response.StatusCode}";
                        }
                    }
                    catch
                    {
                        // If parsing fails, use HTTP status information
                        registerResponse.Message = $"Registration failed: {response.StatusCode} - {response.ReasonPhrase}";
                    }
                }

                return registerResponse;
            }
            catch (Exception ex)
            {
                // Log and handle any unexpected errors
                Console.WriteLine($"RegisterAsync Exception: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return new RegisterResponse
                {
                    Success = false,
                    Message = $"Registration error: {ex.Message}"
                };
            }
        }
    }

    /// <summary>
    /// Response model for login operations
    /// </summary>
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// Response model for registration operations
    /// </summary>
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// Simplified user information model for client-side user data
    /// </summary>
    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}

