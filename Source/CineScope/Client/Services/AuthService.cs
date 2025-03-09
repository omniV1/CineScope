using System.Net.Http.Json;
using CineScope.Shared.Auth;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace CineScope.Client.Services
{
    /// <summary>
    /// Service for handling authentication-related operations.
    /// </summary>
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthStateProvider _authStateProvider;

        /// <summary>
        /// Initializes a new instance of the AuthService.
        /// </summary>
        /// <param name="httpClient">HTTP client for API communication</param>
        /// <param name="authStateProvider">Authentication state provider</param>
        public AuthService(HttpClient httpClient, AuthStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
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

                // If login was successful, notify the auth state provider
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

                // If registration was successful, notify the auth state provider
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
        public async Task<UserDto?> GetCurrentUser()
        {
            return await _authStateProvider.GetCurrentUserAsync();
        }

        /// <summary>
        /// Checks if the user is authenticated.
        /// </summary>
        /// <returns>True if the user is authenticated, false otherwise</returns>
        public async Task<bool> IsAuthenticated()
        {
            var authState = await ((AuthenticationStateProvider)_authStateProvider).GetAuthenticationStateAsync();
            return authState.User.Identity.IsAuthenticated;
        }
    }
}