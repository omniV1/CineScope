using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace CineScope.Client.Services;

public class RecaptchaService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;

    public RecaptchaService(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets the reCAPTCHA response for verification
    /// </summary>
    /// <returns>The verification token from reCAPTCHA</returns>
    public async Task<string> ExecuteRecaptchaAsync(string action)
    {
        try
        {
            var response = await _jsRuntime.InvokeAsync<string>("executeRecaptcha");
            if (string.IsNullOrEmpty(response))
            {
                throw new Exception("No reCAPTCHA response received");
            }
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"reCAPTCHA error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Verifies the reCAPTCHA token with the server
    /// </summary>
    /// <param name="token">The token from reCAPTCHA</param>
    /// <param name="action">The action being verified (for logging purposes)</param>
    /// <returns>True if verification succeeds</returns>
    public async Task<bool> VerifyTokenAsync(string token, string action)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("reCAPTCHA token is empty");
                return false;
            }

            Console.WriteLine($"Verifying reCAPTCHA token for action: {action}");
            var response = await _httpClient.PostAsJsonAsync("api/recaptcha/verify", new { token, action });
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"reCAPTCHA verification failed: {error}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"reCAPTCHA verification error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Resets the reCAPTCHA widget
    /// </summary>
    public async Task ResetAsync()
    {
        await _jsRuntime.InvokeVoidAsync("resetRecaptcha");
    }
} 