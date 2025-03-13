using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using CineScope.Shared.Auth;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CineScope.Client.Services.Auth
{
    /// <summary>
    /// Service responsible for client-side authentication operations.
    /// Handles login, registration, token management, and automatic token refresh.
    /// </summary>
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigationManager;

        // Constants for token management
        private const int TOKEN_REFRESH_THRESHOLD_MINUTES = 5; // Refresh token when less than 5 minutes until expiry
        private const string TOKEN_STORAGE_KEY = "authToken";
        private const string USER_STORAGE_KEY = "user";

        /// <summary>
        /// Initializes a new instance of the AuthService.
        /// </summary>
        /// <param name="httpClient">HTTP client for API communication</param>
        /// <param name="authStateProvider">Authentication state provider</param>
        /// <param name="localStorage">Local storage service for token persistence</param>
        /// <param name="navigationManager">Navigation manager for redirects</param>
        public AuthService(
            HttpClient httpClient,
            AuthStateProvider authStateProvider,
            ILocalStorageService localStorage,
            NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
            _navigationManager = navigationManager;
        }

        /// <summary>
        /// Authenticates a user based on login credentials.
        /// </summary>
        /// <param name="loginRequest">The login credentials</param>
        /// <returns>Authentication result</returns>
        public async Task<AuthResponse> Login(LoginRequest loginRequest)
        {
            try
            {
                // Send login request to the API
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginRequest);

                // Parse the response
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                // If login was successful, store the token and notify the auth state provider
                if (result.Success)
                {
                    await _authStateProvider.NotifyUserAuthentication(result.Token, result.User);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Return error response
                return new AuthResponse
                {
                    Success = false,
                    Message = $"An error occurred during login: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Registers a new user with the provided information.
        /// </summary>
        /// <param name="registerRequest">The registration information</param>
        /// <returns>Registration result</returns>
        public async Task<AuthResponse> Register(RegisterRequest registerRequest)
        {
            try
            {
                // Send registration request to the API
                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", registerRequest);

                // Parse the response
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                // If registration was successful, store the token and notify the auth state provider
                if (result.Success)
                {
                    await _authStateProvider.NotifyUserAuthentication(result.Token, result.User);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Return error response
                return new AuthResponse
                {
                    Success = false,
                    Message = $"An error occurred during registration: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        public async Task Logout()
        {
            // No need to call the server for logout in a JWT-based auth system
            // Simply remove the token and update the auth state
            await _authStateProvider.NotifyUserLogout();
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns>Current user or null if not authenticated</returns>
        public async Task<UserDto> GetCurrentUser()
        {
            return await _authStateProvider.GetCurrentUserAsync();
        }

        /// <summary>
        /// Checks if the user is authenticated.
        /// </summary>
        /// <returns>True if the user is authenticated, false otherwise</returns>
        public async Task<bool> IsAuthenticated()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.Identity.IsAuthenticated;
        }

        /// <summary>
        /// Ensures the authentication header is set on the HttpClient.
        /// If the token is close to expiration, it attempts to refresh it first.
        /// Call this before making API requests that require authentication.
        /// </summary>
        public async Task EnsureAuthHeaderAsync()
        {
            // First, try to refresh the token if needed
            await RefreshTokenIfNeededAsync();

            // Then set the authorization header with the current token
            var token = await _localStorage.GetItemAsync<string>(TOKEN_STORAGE_KEY);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine("Auth header set with current token");
            }
            else
            {
                Console.WriteLine("Warning: No auth token available for header");
                // Remove auth header if no token exists
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        /// <summary>
        /// Checks the current token's expiration and refreshes it if needed.
        /// </summary>
        /// <returns>True if the token is valid (either refreshed or not needing refresh), false otherwise</returns>
        public async Task<bool> RefreshTokenIfNeededAsync()
        {
            try
            {
                // Get the current token from local storage
                var token = await _localStorage.GetItemAsync<string>(TOKEN_STORAGE_KEY);
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("No token to refresh");
                    return false;
                }

                // Decode the token to check expiration time
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Calculate time until expiration
                var expiryTime = jwtToken.ValidTo.ToUniversalTime();
                var timeUntilExpiry = expiryTime - DateTime.UtcNow;

                Console.WriteLine($"Token expires in {timeUntilExpiry.TotalMinutes:F1} minutes");

                // If token expires soon, refresh it
                if (timeUntilExpiry.TotalMinutes < TOKEN_REFRESH_THRESHOLD_MINUTES)
                {
                    Console.WriteLine("Token expiring soon, attempting refresh");
                    return await RefreshTokenAsync();
                }

                // Token is still valid and not close to expiration
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking token expiration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to refresh the authentication token.
        /// </summary>
        /// <returns>True if refresh was successful, false otherwise</returns>
        private async Task<bool> RefreshTokenAsync()
        {
            try
            {
                // Get the current token
                var currentToken = await _localStorage.GetItemAsync<string>(TOKEN_STORAGE_KEY);
                if (string.IsNullOrEmpty(currentToken))
                    return false;

                // Temporarily set the authorization header with the current token for the refresh request
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", currentToken);

                // Send refresh request to the API
                // NOTE: You need to implement this endpoint on the server side!
                var response = await _httpClient.PostAsync("api/Auth/refresh", null);

                if (response.IsSuccessStatusCode)
                {
                    // Parse the refreshed token response
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                    if (result.Success)
                    {
                        // Update token in storage and auth state
                        await _authStateProvider.NotifyUserAuthentication(result.Token, result.User);
                        Console.WriteLine("Token refreshed successfully");
                        return true;
                    }
                }

                // If we get here, refresh failed
                Console.WriteLine($"Token refresh failed: {response.StatusCode}");

                // If unauthorized (401), the refresh token is likely invalid
                // Redirect to login page if that's the case
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HandleFailedAuthentication();
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing token: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Handles HTTP request execution with automatic token refresh on 401 responses.
        /// Use this method instead of direct HttpClient calls for protected API endpoints.
        /// </summary>
        /// <typeparam name="T">The expected response type</typeparam>
        /// <param name="requestFunc">Function that executes the HTTP request</param>
        /// <returns>The API response object of type T</returns>
        public async Task<T> ExecuteWithAuthenticationAsync<T>(Func<Task<T>> requestFunc)
        {
            try
            {
                // Ensure authentication header is up to date before the request
                await EnsureAuthHeaderAsync();

                // Execute the request
                return await requestFunc();
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("401"))
            {
                // Got a 401 Unauthorized response
                Console.WriteLine("Received 401 Unauthorized, attempting token refresh");

                // Try to refresh the token
                if (await RefreshTokenAsync())
                {
                    // If refresh succeeded, retry the request
                    Console.WriteLine("Token refreshed, retrying request");
                    return await requestFunc();
                }

                // If refresh failed, handle failed authentication
                await HandleFailedAuthentication();
                throw; // Rethrow to let the caller handle it
            }
        }

        /// <summary>
        /// Handles failed authentication by logging the user out and redirecting to the login page.
        /// </summary>
        private async Task HandleFailedAuthentication()
        {
            // Log out the user
            await Logout();

            // Redirect to login page with return URL
            var returnUrl = _navigationManager.Uri;
            _navigationManager.NavigateTo($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        /// <summary>
        /// Refreshes the authentication token after a profile update to reflect changes in the UI.
        /// </summary>
        /// <returns>True if token refresh was successful</returns>
        public async Task<bool> RefreshUserStateAsync()
        {
            try
            {
                // Get the current token
                var currentToken = await _localStorage.GetItemAsync<string>("authToken");
                if (string.IsNullOrEmpty(currentToken))
                    return false;

                // Call the API endpoint to refresh the token with updated user information
                var response = await _httpClient.PostAsync("api/Auth/refresh", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (result.Success)
                    {
                        // Update auth state with the new token and user information
                        await _authStateProvider.NotifyUserAuthentication(result.Token, result.User);
                        Console.WriteLine("Authentication state refreshed successfully");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing user state: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates the user information in the authentication state without requiring a full login
        /// </summary>
        public async Task RefreshUserProfile()
        {
            try
            {
                // Get the current token
                var token = await _localStorage.GetItemAsync<string>("authToken");
                if (string.IsNullOrEmpty(token))
                    return;

                // Get updated user profile from the server
                var response = await _httpClient.GetAsync("api/User/profile");

                if (response.IsSuccessStatusCode)
                {
                    var updatedUser = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                    if (updatedUser != null)
                    {
                        // Convert UserProfileDto to UserDto
                        var userDto = new UserDto
                        {
                            Id = updatedUser.Id,
                            Username = updatedUser.Username,
                            Email = updatedUser.Email,
                            ProfilePictureUrl = updatedUser.ProfilePictureUrl,
                            Roles = await GetUserRoles()
                        };

                        // Update the user in local storage
                        await _localStorage.SetItemAsync("user", JsonSerializer.Serialize(userDto));

                        // Create a new authentication state with updated user info
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var jwtToken = tokenHandler.ReadJwtToken(token);

                        // Create claims list from token
                        var claims = new List<Claim>(jwtToken.Claims);

                        // Update or add the ProfilePictureUrl claim
                        var existingClaim = claims.FirstOrDefault(c => c.Type == "ProfilePictureUrl");
                        if (existingClaim != null)
                        {
                            // Remove the old claim
                            claims.Remove(existingClaim);
                        }

                        // Add the updated profile picture URL
                        claims.Add(new Claim("ProfilePictureUrl", updatedUser.ProfilePictureUrl ?? ""));

                        // Create a new identity and principal
                        var identity = new ClaimsIdentity(claims, "jwt");
                        var principal = new ClaimsPrincipal(identity);

                        // Use the new public method instead of the protected one
                        _authStateProvider.UpdateAuthenticationState(Task.FromResult(new AuthenticationState(principal)));

                        Console.WriteLine("User profile refreshed with updated profile picture");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing user profile: {ex.Message}");
            }
        }

        // Helper method to get user roles from the current authentication state
        private async Task<List<string>> GetUserRoles()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var roles = new List<string>();

            if (authState.User.Identity.IsAuthenticated)
            {
                // Check for role claims
                var roleClaims = authState.User.Claims.Where(c => c.Type == ClaimTypes.Role);
                roles.AddRange(roleClaims.Select(c => c.Value));
            }

            if (roles.Count == 0)
            {
                // Default role if none found
                roles.Add("User");
            }

            return roles;
        }
    }
}