using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using CineScope.Shared.Auth;
using CineScope.Shared.DTOs;
using System.Text.Json;
using Blazored.LocalStorage;

namespace CineScope.Client.Services
{
    /// <summary>
    /// Provides authentication state management for the application.
    /// Handles token storage, user authentication and manages the current user context.
    /// </summary>
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationState _anonymous;

        /// <summary>
        /// Initializes a new instance of the AuthStateProvider.
        /// </summary>
        /// <param name="httpClient">HTTP client for API communication</param>
        /// <param name="localStorage">Local storage service for token persistence</param>
        public AuthStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        /// <summary>
        /// Gets the current authentication state of the user.
        /// </summary>
        /// <returns>The current authentication state</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            // If no token is found, user is not authenticated
            if (string.IsNullOrWhiteSpace(token))
                return _anonymous;

            // Set the auth token in the authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Get user data from local storage
            var userJson = await _localStorage.GetItemAsync<string>("user");
            if (string.IsNullOrEmpty(userJson))
                return _anonymous;

            var user = JsonSerializer.Deserialize<UserDto>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (user == null)
                return _anonymous;

            // Create claims for the authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Add role claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "jwt");

            // Return the authenticated state
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        /// <summary>
        /// Notifies the system that a user has been authenticated.
        /// Updates the authentication state and stores the token.
        /// </summary>
        /// <param name="token">The authentication token</param>
        /// <param name="user">The authenticated user</param>
        public async Task NotifyUserAuthentication(string token, UserDto user)
        {
            // Store token and user data in local storage
            await _localStorage.SetItemAsync("authToken", token);
            await _localStorage.SetItemAsync("user", JsonSerializer.Serialize(user));

            // Set the auth token in the authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create claims for the authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Add role claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "jwt");

            // Notify subscribers about the authentication state change
            var authState = new AuthenticationState(new ClaimsPrincipal(identity));
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        /// <summary>
        /// Notifies the system that a user has logged out.
        /// Clears the authentication state and token.
        /// </summary>
        public async Task NotifyUserLogout()
        {
            // Remove token and user data from local storage
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("user");

            // Remove the auth token from the authorization header
            _httpClient.DefaultRequestHeaders.Authorization = null;

            // Notify subscribers about the authentication state change
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }

        /// <summary>
        /// Gets the current user data.
        /// </summary>
        /// <returns>The current user data or null if not authenticated</returns>
        public async Task<UserDto?> GetCurrentUserAsync()
        {
            var userJson = await _localStorage.GetItemAsync<string>("user");
            if (string.IsNullOrEmpty(userJson))
                return null;

            return JsonSerializer.Deserialize<UserDto>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}