using System.Net.Http.Json;
using CineScope.Shared.Models;

namespace CineScope.Client.ClientServices
{
    public class UserClientService
    {
        private readonly HttpClient _httpClient;

        public UserClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserModel?> GetUserAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<UserModel>($"api/user/{id}");
        }

        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            var request = new
            {
                Username = username,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("api/user/login", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>();
            }

            return null;
        }

        public async Task<RegisterResponse?> RegisterAsync(string username, string email, string password)
        {
            var request = new
            {
                Username = username,
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("api/user/register", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<RegisterResponse>();
            }

            return null;
        }
    }

    public class LoginResponse
    {
        public string Message { get; set; } = string.Empty;
        public UserModel? User { get; set; }
    }

    public class RegisterResponse
    {
        public UserModel? User { get; set; }
    }
}