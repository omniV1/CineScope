# Implementing reCAPTCHA with Azure Key Vault for Production

This guide will show you how to implement Google reCAPTCHA in your CineScope application with proper Azure Key Vault integration for production secrets management.

## Step 1: Set Up Azure Key Vault

1. If you haven't already created a Key Vault in Azure:
   
```bash
# Create a Key Vault (run in Azure CLI)
az keyvault create --name CineScopeKeyVault --resource-group YourResourceGroup --location YourRegion
```

2. Add your reCAPTCHA secrets to the Key Vault:

```bash
# Add the reCAPTCHA site key
az keyvault secret set --vault-name CineScopeKeyVault --name "Recaptcha--SiteKey" --value "6LczsgUrAAAAPC7YT5QIINbueIGE6GVjommFIBn"

# Add the reCAPTCHA secret key
az keyvault secret set --vault-name CineScopeKeyVault --name "Recaptcha--SecretKey" --value "6LczsgUrAAAADzw4g_EXfChkMOopFqjZDC_JhV4"
```

## Step 2: Update Server Configuration for Azure Key Vault

1. Add the required NuGet packages to your Server project:

```bash
dotnet add Server/Server.csproj package Azure.Identity
dotnet add Server/Server.csproj package Azure.Security.KeyVault.Secrets
dotnet add Server/Server.csproj package Microsoft.Extensions.Configuration.AzureKeyVault
```

2. Update `Program.cs` in your Server project to use Azure Key Vault:

```csharp
// In Server/Program.cs, modify the configuration setup

// Configure MongoDB serialization settings for better compatibility
ConfigureMongoDb();

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault Configuration when in production
if (builder.Environment.IsProduction())
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    var keyVaultUri = $"https://{keyVaultName}.vault.azure.net/";
    
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
        
    builder.Logging.LogInformation("Azure Key Vault configured for production");
}
// Use local development secrets in Development
else if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Rest of your Program.cs remains the same
```

3. Update your `appsettings.json` to include placeholder for Key Vault name:

```json
{
  "KeyVaultName": "CineScopeKeyVault",
  "Recaptcha": {
    "SiteKey": "", 
    "SecretKey": ""
  },
  // Other settings...
}
```

## Step 3: Create Server-Side RecaptchaService

Create a new file at `Server/Services/RecaptchaService.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CineScope.Server.Services
{
    public class RecaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecaptchaService> _logger;
        private readonly bool _isEnabled;

        public RecaptchaService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            ILogger<RecaptchaService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            // Check if recaptcha is properly configured
            var secretKey = _configuration["Recaptcha:SecretKey"];
            _isEnabled = !string.IsNullOrEmpty(secretKey);
            
            if (!_isEnabled)
            {
                _logger.LogWarning("reCAPTCHA is not configured. Verification will be bypassed.");
            }
            else
            {
                _logger.LogInformation("reCAPTCHA service initialized");
            }
        }

        public async Task<bool> VerifyAsync(string recaptchaResponse)
        {
            // Skip verification if not configured (but log warning)
            if (!_isEnabled)
            {
                _logger.LogWarning("reCAPTCHA verification bypassed because it's not configured");
                return true;
            }
            
            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                _logger.LogWarning("Empty reCAPTCHA response received");
                return false;
            }

            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", _configuration["Recaptcha:SecretKey"]),
                    new KeyValuePair<string, string>("response", recaptchaResponse)
                });

                var response = await _httpClient.PostAsync(
                    "https://www.google.com/recaptcha/api/siteverify", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("reCAPTCHA verification failed with status code: {StatusCode}", 
                        response.StatusCode);
                    return false;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<RecaptchaResponse>(responseJson, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Success != true)
                {
                    _logger.LogWarning("reCAPTCHA verification failed. Error codes: {ErrorCodes}", 
                        string.Join(", ", result?.ErrorCodes ?? Array.Empty<string>()));
                }

                return result?.Success ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during reCAPTCHA verification");
                return false;
            }
        }

        private class RecaptchaResponse
        {
            public bool Success { get; set; }
            public DateTime? ChallengeTs { get; set; }
            public string Hostname { get; set; }
            public string[] ErrorCodes { get; set; }
        }
    }
}
```

## Step 4: Register RecaptchaService in Program.cs

In `Server/Program.cs`, add the RecaptchaService to the dependency injection container:

```csharp
// Register RecaptchaService
builder.Services.AddHttpClient<RecaptchaService>();
builder.Services.AddScoped<RecaptchaService>();
```

## Step 5: Create Request Models for reCAPTCHA

Create a new file at `Shared/Models/RecaptchaRequests.cs`:

```csharp
using CineScope.Shared.Auth;
using CineScope.Shared.DTOs;

namespace CineScope.Shared.Models
{
    public class CaptchaRequest
    {
        public string RecaptchaResponse { get; set; }
    }

    public class RegisterWithCaptchaRequest : CaptchaRequest
    {
        public RegisterRequest RegisterRequest { get; set; }
    }

    public class ReviewWithCaptchaRequest : CaptchaRequest
    {
        public ReviewDto Review { get; set; }
    }
}
```

## Step 6: Create Client-Side RecaptchaConfig Service

Create a new file at `Client/Services/RecaptchaConfigService.cs`:

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace CineScope.Client.Services
{
    public class RecaptchaConfigService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private string _siteKey;
        private bool _isInitialized = false;

        public RecaptchaConfigService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }
        
        public async Task<string> GetSiteKeyAsync()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }
            
            return _siteKey;
        }
        
        public async Task InitializeAsync()
        {
            try
            {
                // Get site key from API
                var response = await _httpClient.GetAsync("api/config/recaptcha-site-key");
                
                if (response.IsSuccessStatusCode)
                {
                    _siteKey = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // Fallback to a default for development only
                    _siteKey = "6LczsgUrAAAAPC7YT5QIINbueIGE6GVjommFIBn";
                    Console.WriteLine("Could not retrieve reCAPTCHA site key from API, using development fallback");
                }
                
                _isInitialized = true;
                
                // Initialize reCAPTCHA in the page
                await _jsRuntime.InvokeVoidAsync("eval", @"
                    if (typeof grecaptcha === 'undefined') {
                        var script = document.createElement('script');
                        script.src = 'https://www.google.com/recaptcha/api.js';
                        script.async = true;
                        script.defer = true;
                        document.head.appendChild(script);
                    }
                ");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error initializing reCAPTCHA: {ex.Message}");
                _siteKey = "6LczsgUrAAAAPC7YT5QIINbueIGE6GVjommFIBn"; // Fallback
                _isInitialized = true;
            }
        }
        
        public async Task<string> GetResponseTokenAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("eval", "grecaptcha.getResponse()");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting reCAPTCHA response: {ex.Message}");
                return string.Empty;
            }
        }
        
        public async Task ResetAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("eval", "grecaptcha.reset()");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error resetting reCAPTCHA: {ex.Message}");
            }
        }
    }
}
```

Register this service in `Client/Program.cs`:

```csharp
builder.Services.AddScoped<RecaptchaConfigService>();
```

## Step 7: Create ConfigController on the Server

Create a new file at `Server/Controllers/ConfigController.cs`:

```csharp
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CineScope.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(IConfiguration configuration, ILogger<ConfigController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("recaptcha-site-key")]
        public IActionResult GetRecaptchaSiteKey()
        {
            try
            {
                var siteKey = _configuration["Recaptcha:SiteKey"];
                
                if (string.IsNullOrEmpty(siteKey))
                {
                    _logger.LogWarning("reCAPTCHA site key not found in configuration");
                    return NotFound("reCAPTCHA site key not configured");
                }
                
                return Ok(siteKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reCAPTCHA site key");
                return StatusCode(500, "Error retrieving configuration");
            }
        }
    }
}
```

## Step 8: Create RecaptchaComponent

Create a new file at `Client/Components/Shared/Recaptcha.razor`:

```csharp
@using CineScope.Client.Services
@inject RecaptchaConfigService RecaptchaConfigService
@inject IJSRuntime JSRuntime

<div class="recaptcha-container my-3">
    <div class="g-recaptcha @Class" data-sitekey="@SiteKey"></div>
</div>

@code {
    [Parameter] public string Class { get; set; } = "";
    [Parameter] public EventCallback<string> OnVerified { get; set; }
    
    private string SiteKey { get; set; } = "";
    
    protected override async Task OnInitializedAsync()
    {
        SiteKey = await RecaptchaConfigService.GetSiteKeyAsync();
    }
    
    public async Task<string> GetResponseAsync()
    {
        return await RecaptchaConfigService.GetResponseTokenAsync();
    }
    
    public async Task ResetAsync()
    {
        await RecaptchaConfigService.ResetAsync();
    }
}
```

## Step 9: Update AuthService and ReviewService for Client

Update both `AuthService.cs` and create a new `ReviewService.cs` to handle the reCAPTCHA validation:

For `ReviewService.cs`:

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CineScope.Shared.DTOs;
using CineScope.Shared.Models;

namespace CineScope.Client.Services
{
    public class ReviewService
    {
        private readonly HttpClient _httpClient;
        
        public ReviewService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<ReviewDto> SubmitReviewWithCaptchaAsync(ReviewDto review, string recaptchaResponse)
        {
            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                throw new ArgumentException("reCAPTCHA response is required");
            }
            
            var request = new ReviewWithCaptchaRequest
            {
                Review = review,
                RecaptchaResponse = recaptchaResponse
            };
            
            var response = await _httpClient.PostAsJsonAsync("api/Review/with-captcha", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to submit review: {response.StatusCode} - {error}");
            }
            
            return await response.Content.ReadFromJsonAsync<ReviewDto>();
        }
    }
}
```

## Step 10: Update the Register Component

Modify `Client/Pages/Auth/Register.razor`:

```razor
@page "/register"
@using CineScope.Client.Services
@using CineScope.Client.Components.Shared
@using CineScope.Shared.Auth
@using CineScope.Shared.Models
@inject AuthService AuthService
@inject RecaptchaConfigService RecaptchaConfigService
@inject NavigationManager NavigationManager
@inject ISnackbar Snackbar

<PageTitle>CineScope - Register</PageTitle>

<MudGrid Justify="Justify.Center" Class="mt-8">
    <MudItem xs="12" sm="8" md="6" lg="4">
        <MudCard Elevation="3" Class="pa-4">
            <MudForm @ref="form" @bind-IsValid="@success" @bind-Errors="@errors">
                <MudCardContent>
                    <MudText Typo="Typo.h4" Align="Align.Center" GutterBottom="true">Create Account</MudText>
                    <MudText Typo="Typo.subtitle1" Align="Align.Center" Class="mb-4">Join the CineScope community</MudText>

                    <!-- Form Fields -->
                    <!-- ... existing fields ... -->

                    <!-- reCAPTCHA component -->
                    <Recaptcha @ref="recaptchaComponent" />

                    @if (!string.IsNullOrEmpty(errorMessage))
                    {
                        <MudAlert Severity="Severity.Error" Class="mt-4" Dense="true">@errorMessage</MudAlert>
                    }
                </MudCardContent>

                <MudCardActions Class="d-flex justify-center flex-column gap-4">
                    <MudButton Variant="Variant.Filled" Color="Color.Primary" Size="Size.Large"
                               FullWidth="true" Disabled="@(!success || isLoading)"
                               OnClick="HandleRegister">
                        @if (isLoading)
                        {
                            <MudProgressCircular Class="ms-n1" Size="Size.Small" Indeterminate="true" />
                            <MudText Class="ms-2">Processing</MudText>
                        }
                        else
                        {
                            <MudText>Register</MudText>
                        }
                    </MudButton>

                    <MudLink Href="/login">Already have an account? Login here</MudLink>
                </MudCardActions>
            </MudForm>
        </MudCard>
    </MudItem>
</MudGrid>

@code {
    private MudForm form;
    private bool success;
    private string[] errors = { };
    private string errorMessage = string.Empty;
    private bool isLoading = false;
    private Recaptcha recaptchaComponent;

    private RegisterRequest registerRequest = new RegisterRequest();

    // Validation methods remain the same...

    private async Task HandleRegister()
    {
        // Validate the form
        await form.Validate();

        if (!success)
        {
            return;
        }

        try
        {
            // Set loading state
            isLoading = true;
            errorMessage = string.Empty;

            // Get reCAPTCHA response
            var recaptchaResponse = await recaptchaComponent.GetResponseAsync();
            
            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                errorMessage = "Please complete the reCAPTCHA verification";
                Snackbar.Add(errorMessage, Severity.Warning);
                return;
            }

            // Create request with captcha
            var request = new RegisterWithCaptchaRequest
            {
                RegisterRequest = registerRequest,
                RecaptchaResponse = recaptchaResponse
            };

            // Use the AuthService to register the user
            var response = await Http.PostAsJsonAsync("api/Auth/register-with-captcha", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                
                if (result.Success)
                {
                    // Show success message
                    Snackbar.Add("Registration successful! Welcome to CineScope.", Severity.Success);

                    // Redirect to home page
                    NavigationManager.NavigateTo("/");
                }
                else
                {
                    // Handle error response
                    errorMessage = result.Message ?? "Registration failed. Please try again.";
                    Snackbar.Add(errorMessage, Severity.Error);
                    
                    // Reset captcha
                    await recaptchaComponent.ResetAsync();
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                errorMessage = $"Registration failed: {error}";
                Snackbar.Add(errorMessage, Severity.Error);
                
                // Reset captcha
                await recaptchaComponent.ResetAsync();
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            errorMessage = "An error occurred during registration. Please try again.";
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
            
            // Reset captcha
            if (recaptchaComponent != null)
            {
                await recaptchaComponent.ResetAsync();
            }
        }
        finally
        {
            // Reset loading state
            isLoading = false;
        }
    }
}
```

## Step 11: Update the CreateReview Component

Modify `Client/Components/Reviews/CreateReview.razor`:

```razor
@using System.Net.Http.Json
@using CineScope.Client.Services
@using CineScope.Client.Components.Shared
@using CineScope.Client.Services.Auth
@using CineScope.Shared.DTOs
@using CineScope.Shared.Models
@inject HttpClient Http
@inject ISnackbar Snackbar
@inject AuthService AuthService
@inject ReviewService ReviewService

<MudCard Elevation="3" Class="create-review-form mb-4">
    <!-- Card header and content remain the same -->

    <!-- Add reCAPTCHA before the action buttons -->
    <MudCardContent>
        <!-- Existing content -->
        
        <!-- Add reCAPTCHA -->
        <Recaptcha @ref="recaptchaComponent" Class="my-4" />

        <!-- Existing warnings and alerts -->
    </MudCardContent>

    <MudCardActions Class="pb-4 px-4">
        <MudButton Variant="Variant.Text" Color="Color.Secondary" OnClick="@Reset">Reset</MudButton>
        <MudSpacer />
        <MudButton Variant="Variant.Filled" Color="Color.Primary"
                   Disabled="@(isLoading || ratingValue == 0)" OnClick="@SubmitReview">
            @if (isLoading)
            {
                <MudProgressCircular Class="ms-n1" Size="Size.Small" Indeterminate="true" />
                <MudText Class="ms-2">Submitting</MudText>
            }
            else
            {
                <MudText>Submit Review</MudText>
            }
        </MudButton>
    </MudCardActions>
</MudCard>

@code {
    // Existing parameters and fields
    
    private Recaptcha recaptchaComponent;

    // SubmitReview method updated to use reCAPTCHA
    private async Task SubmitReview()
    {
        // Validate the form
        await form.Validate();

        if (!success)
        {
            return;
        }

        // Check if user has selected a rating
        if (ratingValue == 0)
        {
            errorMessage = "Please select a rating";
            return;
        }

        try
        {
            // Set loading state
            isLoading = true;
            errorMessage = string.Empty;

            // Get reCAPTCHA response
            var recaptchaResponse = await recaptchaComponent.GetResponseAsync();
            
            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                errorMessage = "Please complete the reCAPTCHA verification";
                Snackbar.Add(errorMessage, Severity.Warning);
                return;
            }

            // Validate content
            if (!await ValidateContent())
            {
                errorMessage = "Review contains inappropriate content. Please revise.";
                Snackbar.Add(errorMessage, Severity.Warning);
                return;
            }

            // Ensure we have a user ID
            if (string.IsNullOrEmpty(review.UserId))
            {
                var user = await AuthService.GetCurrentUser();
                if (user == null)
                {
                    errorMessage = "You must be logged in to submit a review.";
                    Snackbar.Add(errorMessage, Severity.Error);
                    return;
                }
                review.UserId = user.Id;
                review.Username = user.Username;
            }

            // Set the rating and date
            review.Rating = (double)ratingValue;
            review.CreatedAt = DateTime.Now;

            try
            {
                // Submit review with captcha
                var request = new ReviewWithCaptchaRequest
                {
                    Review = review,
                    RecaptchaResponse = recaptchaResponse
                };
                
                var response = await Http.PostAsJsonAsync("api/Review/with-captcha", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var createdReview = await response.Content.ReadFromJsonAsync<ReviewDto>();
                    
                    // Show success message
                    Snackbar.Add("Review submitted successfully!", Severity.Success);
                    
                    // Notify parent component
                    if (createdReview != null)
                    {
                        await OnReviewSubmitted.InvokeAsync(createdReview);
                    }
                    
                    // Reset the form
                    Reset();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    errorMessage = $"Failed to submit review: {error}";
                    Snackbar.Add(errorMessage, Severity.Error);
                    
                    // Reset captcha
                    await recaptchaComponent.ResetAsync();
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error submitting review: {ex.Message}";
                Snackbar.Add(errorMessage, Severity.Error);
                
                // Reset captcha
                await recaptchaComponent.ResetAsync();
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            errorMessage = "An error occurred. Please try again.";
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            // Reset loading state
            isLoading = false;
        }
    }

    // Reset method updated to reset reCAPTCHA
    private async Task Reset()
    {
        review.Rating = 0;
        ratingValue = 0;
        review.Text = string.Empty;
        errorMessage = string.Empty;
        contentWarning = null;
        
        // Reset captcha
        if (recaptchaComponent != null)
        {
            await recaptchaComponent.ResetAsync();
        }
        
        StateHasChanged();
    }
}
```

## Step 12: Configure Azure App Service for Key Vault Access

If you're hosting in Azure App Service, ensure it has access to your Key Vault:

1. In Azure Portal, go to your App Service
2. Navigate to Settings > Identity
3. Set System assigned to On and save
4. Note the Object ID that appears
5. Go to your Key Vault
6. Navigate to Access Policies
7. Add Access Policy:
   - Select "Get, List" under Secret permissions
   - Select the principal (your App Service's managed identity)
   - Save the policy

## Step 13: Update App Service Configuration

1. In Azure Portal, go to your App Service
2. Navigate to Settings > Configuration
3. Add these application settings:

   - `ASPNETCORE_ENVIRONMENT`: `Production`
   - `KeyVaultName`: `CineScopeKeyVault` (your Key Vault name)

## Step 14: Update Azure Pipeline

If you're using Azure DevOps for deployment, update your pipeline YAML:

```yaml
# Add to your build or release pipeline
steps:
- task: AzureAppServiceSettings@1
  displayName: 'Configure App Service Settings'
  inputs:
    azureSubscription: '$(AzureSubscription)'
    appName: '$(WebAppName)'
    resourceGroupName: '$(ResourceGroupName)'
    appSettings: |
      [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production",
          "slotSetting": false
        },
        {
          "name": "KeyVaultName",
          "value": "CineScopeKeyVault",
          "slotSetting": false
        }
      ]
```

## Step 15: Test Your Implementation

1. Deploy your changes to Azure
2. Test registration with reCAPTCHA
3. Test review submission with reCAPTCHA
4. Verify in logs that your app is using the Key Vault secrets

## Final Check: Security Best Practices

1. **Ensure your Azure resources have appropriate access controls**
2. **Avoid committing secrets to source control**
3. **Use a managed identity for your App Service whenever possible**
4. **Keep your Key Vault access restricted to only necessary services**
5. **Use conditional access policies in reCAPTCHA for better security**
6. **Add appropriate logging for security events**

This implementation provides a secure way to deploy reCAPTCHA with Azure Key Vault in your CineScope application. The server-side validation verifies the reCAPTCHA response with Google's servers, while the client-side component retrieves the site key securely from the server.
