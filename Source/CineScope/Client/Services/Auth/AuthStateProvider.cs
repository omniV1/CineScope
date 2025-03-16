using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using CineScope.Shared.Auth;
using CineScope.Shared.DTOs;
using System.Text.Json;
using Blazored.LocalStorage;
using System.IdentityModel.Tokens.Jwt;

namespace CineScope.Client.Services.Auth
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
        /// Allows updating the authentication state externally, primarily for refreshes.
        /// </summary>
        /// <param name="authState">The new authentication state</param>
        public void UpdateAuthenticationState(Task<AuthenticationState> authState)
        {
            NotifyAuthenticationStateChanged(authState);
        }

        /// <summary>
        /// Gets the current authentication state of the user.
        /// </summary>
        /// <returns>The current authentication state</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                // If no token is found, user is not authenticated
                if (string.IsNullOrWhiteSpace(token))
                {
                    Console.WriteLine("No auth token found, returning anonymous state");
                    return _anonymous;
                }

                // Set the auth token in the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Get user data from local storage
                var userJson = await _localStorage.GetItemAsync<string>("user");
                if (string.IsNullOrEmpty(userJson))
                {
                    Console.WriteLine("No user data found, returning anonymous state");
                    return _anonymous;
                }

                var user = JsonSerializer.Deserialize<UserDto>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (user == null)
                {
                    Console.WriteLine("Failed to deserialize user data, returning anonymous state");
                    return _anonymous;
                }

                Console.WriteLine($"Found authenticated user: {user.Username} (ID: {user.Id})");

                // Create claims for the authenticated user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("ProfilePictureUrl", user.ProfilePictureUrl ?? "")
                };

                // Add role claims
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var identity = new ClaimsIdentity(claims, "jwt");

                // Return the authenticated state
                var state = new AuthenticationState(new ClaimsPrincipal(identity));
                Console.WriteLine("Returning authenticated state");
                return state;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAuthenticationStateAsync: {ex.Message}");
                return _anonymous;
            }
        }

        /// <summary>
        /// Notifies the system that a user has been authenticated.
        /// Updates the authentication state and stores the token.
        /// </summary>
        /// <param name="token">The authentication token</param>
        /// <param name="user">The authenticated user</param>
        public async Task NotifyUserAuthentication(string token, UserDto user)
        {
            try
            {
                Console.WriteLine($"Notifying user authentication: {user.Username} (ID: {user.Id})");

                // Store token and user data in local storage
                await _localStorage.SetItemAsync("authToken", token);
                await _localStorage.SetItemAsync("user", JsonSerializer.Serialize(user));

                // Set the auth token in the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Decode the token to extract claims
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Create claims for the authenticated user
                var claims = new List<Claim>();

                // Add all claims from the token
                foreach (var claim in jwtToken.Claims)
                {
                    claims.Add(new Claim(claim.Type, claim.Value));
                    Console.WriteLine($"Added claim from token: {claim.Type} = {claim.Value}");
                }

                // Ensure we have the key claims
                if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier) && !claims.Any(c => c.Type == "sub"))
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                    Console.WriteLine("Added missing NameIdentifier claim");
                }

                if (!claims.Any(c => c.Type == ClaimTypes.Name))
                {
                    claims.Add(new Claim(ClaimTypes.Name, user.Username));
                    Console.WriteLine("Added missing Name claim");
                }

                if (!claims.Any(c => c.Type == ClaimTypes.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, user.Email));
                    Console.WriteLine("Added missing Email claim");
                }

                // Add ProfilePictureUrl claim if missing
                if (!claims.Any(c => c.Type == "ProfilePictureUrl"))
                {
                    claims.Add(new Claim("ProfilePictureUrl", user.ProfilePictureUrl ?? ""));
                    Console.WriteLine("Added missing ProfilePictureUrl claim");
                }

                // Make sure roles are included
                var roleClaims = claims.Where(c => c.Type == ClaimTypes.Role).ToList();
                if (roleClaims.Count == 0 && user.Roles != null && user.Roles.Any())
                {
                    foreach (var role in user.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                        Console.WriteLine($"Added missing role claim: {role}");
                    }
                }

                var identity = new ClaimsIdentity(claims, "jwt");

                // Notify subscribers about the authentication state change
                var authState = new AuthenticationState(new ClaimsPrincipal(identity));
                NotifyAuthenticationStateChanged(Task.FromResult(authState));

                Console.WriteLine("Authentication state updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in NotifyUserAuthentication: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Notifies the system that a user has logged out.
        /// Clears the authentication state and token.
        /// </summary>
        public async Task NotifyUserLogout()
        {
            try
            {
                Console.WriteLine("Notifying user logout");

                // Remove token and user data from local storage
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("user");

                // Remove the auth token from the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = null;

                // Notify subscribers about the authentication state change
                NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));

                Console.WriteLine("User logged out successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in NotifyUserLogout: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the current user data.
        /// </summary>
        /// <returns>The current user data or null if not authenticated</returns>
        public async Task<UserDto?> GetCurrentUserAsync()
        {
            try
            {
                var userJson = await _localStorage.GetItemAsync<string>("user");
                if (string.IsNullOrEmpty(userJson))
                {
                    Console.WriteLine("No user data in local storage");
                    return null;
                }

                var user = JsonSerializer.Deserialize<UserDto>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (user != null)
                {
                    Console.WriteLine($"Retrieved current user: {user.Username} (ID: {user.Id})");
                }
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCurrentUserAsync: {ex.Message}");
                return null;
            }
        }
    }
}