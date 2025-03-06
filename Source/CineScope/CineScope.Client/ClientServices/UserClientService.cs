using System.Net.Http.Json;
using CineScope.Shared.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CineScope.Client.ClientServices
{
    public class UserClientService
    {
        private readonly HttpClient _httpClient;

        public UserClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            Console.WriteLine($"UserClientService initialized with BaseAddress: {_httpClient.BaseAddress}");
        }

        public string GetBaseAddress()
        {
            return _httpClient.BaseAddress?.ToString() ?? "No base address set";
        }

        public async Task<UserModel?> GetUserAsync(string id)
        {
            try
            {
                Console.WriteLine($"GetUserAsync: Calling api/users/{id}");
                return await _httpClient.GetFromJsonAsync<UserModel>($"api/users/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserAsync Error: {ex.Message}");
                return null;
            }
        }

        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            try
            {
                Console.WriteLine($"LoginAsync: Calling api/users/login with username: {username}");
                
                // Create the login model
                var loginModel = new LoginModel
                {
                    Username = username,
                    Password = password
                };

                Console.WriteLine($"Sending login request to: {_httpClient.BaseAddress}api/users/login");
                
                // Send the POST request
                var response = await _httpClient.PostAsJsonAsync("api/users/login", loginModel);
                
                Console.WriteLine($"Login response status: {response.StatusCode}");

                // Read the response content as string for debugging
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login response content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        // Use case-insensitive property name matching to handle different casing
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        // Parse the response content manually for more control
                        using (JsonDocument document = JsonDocument.Parse(responseContent))
                        {
                            JsonElement root = document.RootElement;
                            
                            var loginResponse = new LoginResponse();
                            
                            if (root.TryGetProperty("success", out JsonElement successElement))
                            {
                                loginResponse.Success = successElement.GetBoolean();
                                
                                if (loginResponse.Success && root.TryGetProperty("user", out JsonElement userElement))
                                {
                                    var userInfo = new UserInfo
                                    {
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
                                    loginResponse.Message = messageElement.GetString() ?? "Unknown error";
                                }
                            }
                            else
                            {
                                // Try to deserialize using the default method as a fallback
                                var deserializedResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, options);
                                if (deserializedResponse != null)
                                {
                                    loginResponse = deserializedResponse;
                                }
                                else
                                {
                                    loginResponse.Success = true; // If the response was successful but format is unexpected
                                    loginResponse.Message = "Login successful but response format was unexpected";
                                }
                            }
                            
                            Console.WriteLine($"Login success: {loginResponse.Success}, Message: {loginResponse.Message}");
                            return loginResponse;
                        }
                    }
                    catch (JsonException ex)
                    {
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
                Console.WriteLine($"HTTP request error during login: {ex.Message}");
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error during login: {ex.Message}");
                return new LoginResponse
                {
                    Success = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }

        public async Task<RegisterResponse?> RegisterAsync(string username, string email, string password)
        {
            try
            {
                // Create the request model 
                var registerModel = new
                {
                    Username = username,
                    Email = email,
                    Password = password
                };

                // Log full request URL for debugging
                Console.WriteLine($"RegisterAsync: Full request URL: {_httpClient.BaseAddress}api/users/register");
                Console.WriteLine($"RegisterAsync: Sending request with username: {username}, email: {email}");

                // Use direct JSON serialization for more control
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(registerModel),
                    System.Text.Encoding.UTF8,
                    "application/json");

                // Make the HTTP request
                var response = await _httpClient.PostAsync("api/users/register", jsonContent);
                Console.WriteLine($"RegisterAsync: Received status code {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"RegisterAsync: Raw response content: {content}");

                // Create a response object to return
                var registerResponse = new RegisterResponse();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        using (JsonDocument document = JsonDocument.Parse(content))
                        {
                            JsonElement root = document.RootElement;
                            
                            if (root.TryGetProperty("success", out JsonElement successElement))
                            {
                                registerResponse.Success = successElement.GetBoolean();
                                
                                if (registerResponse.Success && root.TryGetProperty("user", out JsonElement userElement))
                                {
                                    var userInfo = new UserInfo
                                    {
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
                                    registerResponse.Message = messageElement.GetString() ?? "Registration failed";
                                }
                                else
                                {
                                    registerResponse.Message = "Registration failed with unknown error";
                                }
                            }
                            else if (root.TryGetProperty("message", out JsonElement messageElement))
                            {
                                registerResponse.Success = false;
                                registerResponse.Message = messageElement.GetString() ?? "Registration failed";
                            }
                            else
                            {
                                registerResponse.Success = true;
                                registerResponse.Message = "Registration successful but response format was unexpected";
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"RegisterAsync: Error parsing JSON response: {ex.Message}");
                        registerResponse.Success = true;
                        registerResponse.Message = "Registration successful but could not parse response data";
                    }
                }
                else
                {
                    registerResponse.Success = false;
                    
                    try
                    {
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
                        registerResponse.Message = $"Registration failed: {response.StatusCode} - {response.ReasonPhrase}";
                    }
                }
                
                return registerResponse;
            }
            catch (Exception ex)
            {
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

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }
    
    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}