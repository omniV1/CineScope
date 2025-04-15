using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace CineScope.Client.Services.Auth
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationState _anonymous;

        public AuthStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public void UpdateAuthenticationState(Task<AuthenticationState> authState)
        {
            NotifyAuthenticationStateChanged(authState);
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return _anonymous;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var userJson = await _localStorage.GetItemAsync<string>("user");
                if (string.IsNullOrEmpty(userJson))
                {
                    return _anonymous;
                }

                var user = JsonSerializer.Deserialize<UserDto>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (user == null)
                {
                    return _anonymous;
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("ProfilePictureUrl", user.ProfilePictureUrl ?? "")
                };

                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var identity = new ClaimsIdentity(claims, "jwt");
                var state = new AuthenticationState(new ClaimsPrincipal(identity));
                return state;
            }
            catch (Exception)
            {
                return _anonymous;
            }
        }

        public async Task NotifyUserAuthentication(string token, UserDto user)
        {
            await _localStorage.SetItemAsync("authToken", token);
            await _localStorage.SetItemAsync("user", JsonSerializer.Serialize(user));

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var claims = new List<Claim>();

            foreach (var claim in jwtToken.Claims)
            {
                claims.Add(new Claim(claim.Type, claim.Value));
            }

            if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier) && !claims.Any(c => c.Type == "sub"))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            }

            if (!claims.Any(c => c.Type == ClaimTypes.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.Username));
            }

            if (!claims.Any(c => c.Type == ClaimTypes.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            if (!claims.Any(c => c.Type == "ProfilePictureUrl"))
            {
                claims.Add(new Claim("ProfilePictureUrl", user.ProfilePictureUrl ?? ""));
            }

            var roleClaims = claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            if (roleClaims.Count == 0 && user.Roles != null && user.Roles.Any())
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var authState = new AuthenticationState(new ClaimsPrincipal(identity));
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public async Task NotifyUserLogout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("user");

            _httpClient.DefaultRequestHeaders.Authorization = null;

            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }

        public async Task<UserDto?> GetCurrentUserAsync()
        {
            var userJson = await _localStorage.GetItemAsync<string>("user");
            if (string.IsNullOrEmpty(userJson))
            {
                return null;
            }

            var user = JsonSerializer.Deserialize<UserDto>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return user;
        }

        public async Task<Dictionary<string, string>> DecodeJwtTokenAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrEmpty(token))
            {
                return new Dictionary<string, string> { { "Error", "No token found" } };
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
            return claims;
        }
    }
}
