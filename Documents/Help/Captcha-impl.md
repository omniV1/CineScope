# Complete Integration Guide for reCAPTCHA in CineScope

This step-by-step guide will help you implement Google's reCAPTCHA v2 in your CineScope application to protect both your registration and review submission forms from spam and abuse.

## Step 1: Configure Application Settings

First, add reCAPTCHA configuration to your application settings:

1. Open `Server/appsettings.json`
2. Add the following configuration:

```json
"Recaptcha": {
  "SiteKey": "6LczsgUrAAAAPC7YT5QIINbueIGE6GVjommFIBn",
  "SecretKey": "6LczsgUrAAAADzw4g_EXfChkMOopFqjZDC_JhV4"
}
```

## Step 2: Add reCAPTCHA Script to Index.html

1. Open `Client/wwwroot/index.html`
2. Add the reCAPTCHA script in the `<head>` section:

```html
<!-- Google reCAPTCHA -->
<script src="https://www.google.com/recaptcha/api.js" async defer></script>
```

## Step 3: Create Helper JavaScript Functions

1. Create a new JavaScript file at `Client/wwwroot/js/recaptcha.js`
2. Add the following functions:

```javascript
window.recaptchaFunctions = {
    getResponse: function() {
        return grecaptcha ? grecaptcha.getResponse() : '';
    },
    
    reset: function() {
        if (typeof grecaptcha !== 'undefined') {
            grecaptcha.reset();
        }
    }
};
```

3. Reference this script in `index.html` just before the closing `</body>` tag:

```html
<script src="js/recaptcha.js"></script>
```

## Step 4: Create RecaptchaService on the Server

1. Create a new file at `Server/Services/RecaptchaService.cs`:

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
        private readonly string _secretKey;
        private readonly ILogger<RecaptchaService> _logger;
        private readonly bool _isEnabled;

        public RecaptchaService(HttpClient httpClient, IConfiguration configuration, ILogger<RecaptchaService> logger)
        {
            _httpClient = httpClient;
            _secretKey = configuration["Recaptcha:SecretKey"];
            _logger = logger;
            _isEnabled = !string.IsNullOrEmpty(_secretKey);
            
            if (!_isEnabled)
            {
                _logger.LogWarning("reCAPTCHA is not configured. Verification will be bypassed.");
            }
        }

        public async Task<bool> VerifyAsync(string recaptchaResponse)
        {
            if (!_isEnabled)
            {
                _logger.LogWarning("reCAPTCHA verification bypassed due to missing configuration.");
                return true; // Skip verification if not configured
            }
            
            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                _logger.LogWarning("Empty reCAPTCHA response received.");
                return false;
            }

            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", _secretKey),
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

## Step 5: Register RecaptchaService in Program.cs

1. Open `Server/Program.cs`
2. Add the RecaptchaService to the services collection:

```csharp
// Add this with your other service registrations
builder.Services.AddHttpClient<RecaptchaService>();
builder.Services.AddScoped<RecaptchaService>();
```

## Step 6: Create Request Models for reCAPTCHA

1. Create a new file at `Shared/Models/RecaptchaRequests.cs`:

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

## Step 7: Update AuthController

1. Open `Server/Controllers/AuthController.cs`
2. Add the RecaptchaService to the constructor:

```csharp
private readonly RecaptchaService _recaptchaService;

public AuthController(
    IAuthService authService,
    IMongoDbService mongoDbService,
    IOptions<MongoDbSettings> options,
    IConfiguration configuration,
    RecaptchaService recaptchaService)
{
    _authService = authService;
    _mongoDbService = mongoDbService;
    _settings = options.Value;
    _configuration = configuration;
    _recaptchaService = recaptchaService;
}
```

3. Add a new endpoint for registration with reCAPTCHA:

```csharp
/// <summary>
/// POST: api/Auth/register-with-captcha
/// Handles user registration requests with reCAPTCHA verification.
/// </summary>
/// <param name="request">Registration information with captcha response</param>
/// <returns>Registration result with token if successful</returns>
[HttpPost("register-with-captcha")]
public async Task<ActionResult<AuthResponse>> RegisterWithCaptcha([FromBody] RegisterWithCaptchaRequest request)
{
    // Validate the model state
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // Verify reCAPTCHA
    var isValidCaptcha = await _recaptchaService.VerifyAsync(request.RecaptchaResponse);
    if (!isValidCaptcha)
    {
        return BadRequest(new AuthResponse { 
            Success = false, 
            Message = "reCAPTCHA verification failed. Please try again." 
        });
    }

    // Validate that passwords match
    if (request.RegisterRequest.Password != request.RegisterRequest.ConfirmPassword)
    {
        ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
        return BadRequest(ModelState);
    }

    // Attempt to register the user
    var result = await _authService.RegisterAsync(request.RegisterRequest);

    // Return appropriate response based on result
    if (result.Success)
    {
        return Ok(result);
    }
    else
    {
        // Return 400 Bad Request for registration failures
        return BadRequest(result);
    }
}
```

## Step 8: Update ReviewController

1. Open `Server/Controllers/ReviewController.cs`
2. Add the RecaptchaService to the constructor:

```csharp
private readonly RecaptchaService _recaptchaService;

public ReviewController(
    ReviewService reviewService, 
    ContentFilterService contentFilterService, 
    UserService userService,
    RecaptchaService recaptchaService)
{
    _reviewService = reviewService;
    _contentFilterService = contentFilterService;
    _userService = userService;
    _recaptchaService = recaptchaService;
}
```

3. Add a new endpoint for creating reviews with reCAPTCHA:

```csharp
/// <summary>
/// POST: api/Review/with-captcha
/// Creates a new review after content validation and reCAPTCHA verification.
/// </summary>
/// <param name="request">The review data with captcha response</param>
/// <returns>The created review with assigned ID</returns>
[HttpPost("with-captcha")]
[Authorize] // Require authentication
public async Task<ActionResult<ReviewDto>> CreateReviewWithCaptcha([FromBody] ReviewWithCaptchaRequest request)
{
    try
    {
        // Verify reCAPTCHA
        var isValidCaptcha = await _recaptchaService.VerifyAsync(request.RecaptchaResponse);
        if (!isValidCaptcha)
        {
            return BadRequest(new { 
                Message = "reCAPTCHA verification failed. Please try again." 
            });
        }

        // Get user identity from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                         User.FindFirst("sub");

        if (userIdClaim == null)
        {
            return Unauthorized(new { Message = "User identity could not be determined" });
        }

        // Force the userId to match the authenticated user
        request.Review.UserId = userIdClaim.Value;

        // Validate content against banned words
        var contentValidation = await _contentFilterService.ValidateContentAsync(request.Review.Text);

        // If content is not approved, return bad request
        if (!contentValidation.IsApproved)
        {
            return BadRequest(new
            {
                Message = "Review contains inappropriate content",
                ViolationWords = contentValidation.ViolationWords
            });
        }

        // Ensure movie ID is valid
        if (!ObjectId.TryParse(request.Review.MovieId, out _))
        {
            return BadRequest("Invalid movie ID format");
        }

        // Ensure user ID is valid
        if (!ObjectId.TryParse(request.Review.UserId, out _))
        {
            return BadRequest("Invalid user ID format");
        }

        // Map DTO to model
        var review = new Review
        {
            MovieId = request.Review.MovieId,
            UserId = userIdClaim.Value, // Use the authenticated user's ID
            Rating = request.Review.Rating,
            Text = request.Review.Text,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsApproved = true,
            FlaggedWords = Array.Empty<string>()
        };

        // Create the review in the database
        var createdReview = await _reviewService.CreateReviewAsync(review);

        // Update the movie's average rating
        await _reviewService.UpdateMovieAverageRatingAsync(review.MovieId);

        // Map back to DTO and return
        var createdDto = MapToDto(createdReview);
        createdDto.Username = request.Review.Username; // Preserve username from request

        return CreatedAtAction(nameof(GetReviewById), new { id = createdReview.Id }, createdDto);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception in CreateReviewWithCaptcha: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return StatusCode(500, new { Error = $"Failed to create review: {ex.Message}" });
    }
}
```

## Step 9: Update Client AuthService

1. Open `Client/Services/Auth/AuthService.cs`
2. Add a method to handle registration with reCAPTCHA:

```csharp
/// <summary>
/// Registers a new user with reCAPTCHA verification.
/// </summary>
/// <param name="registerRequest">The registration information</param>
/// <param name="recaptchaResponse">The reCAPTCHA response token</param>
/// <returns>Registration result</returns>
public async Task<AuthResponse> RegisterWithCaptchaAsync(RegisterRequest registerRequest, string recaptchaResponse)
{
    try
    {
        Console.WriteLine($"Attempting registration with reCAPTCHA for user: {registerRequest.Username}");

        var request = new RegisterWithCaptchaRequest
        {
            RegisterRequest = registerRequest,
            RecaptchaResponse = recaptchaResponse
        };

        // Send registration request to the API
        var response = await _httpClient.PostAsJsonAsync("api/Auth/register-with-captcha", request);

        Console.WriteLine($"Registration with captcha response status: {response.StatusCode}");

        // Parse the response
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // If registration was successful, store the token and notify the auth state provider
        if (result.Success)
        {
            Console.WriteLine("Registration successful, updating authentication state");
            await _authStateProvider.NotifyUserAuthentication(result.Token, result.User);
        }
        else
        {
            Console.WriteLine($"Registration failed: {result.Message}");
        }

        return result;
    }
    catch (Exception ex)
    {
        // Return error response
        Console.WriteLine($"Exception in RegisterWithCaptcha: {ex.Message}");
        return new AuthResponse
        {
            Success = false,
            Message = $"An error occurred during registration: {ex.Message}"
        };
    }
}
```

## Step 10: Update Register Component

1. Open `Client/Pages/Auth/Register.razor`
2. Add the necessary using statements and injected services:

```csharp
@using CineScope.Shared.Models
@inject IJSRuntime JSRuntime
```

3. Add the reCAPTCHA HTML element in the form, right before the registration button:

```html
<div class="my-4">
    <div class="g-recaptcha" data-sitekey="6LczsgUrAAAAPC7YT5QIINbueIGE6GVjommFIBn"></div>
</div>
```

4. Update the `HandleRegister` method to include reCAPTCHA verification:

```csharp
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
        var recaptchaResponse = await JSRuntime.InvokeAsync<string>(
            "recaptchaFunctions.getResponse");

        if (string.IsNullOrEmpty(recaptchaResponse))
        {
            errorMessage = "Please complete the reCAPTCHA verification.";
            Snackbar.Add(errorMessage, Severity.Warning);
            return;
        }

        // Use the AuthService to register the user with captcha
        var result = await AuthService.RegisterWithCaptchaAsync(registerRequest, recaptchaResponse);

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
            
            // Reset the captcha
            await JSRuntime.InvokeVoidAsync("recaptchaFunctions.reset");
        }
    }
    catch (Exception ex)
    {
        // Handle exceptions
        errorMessage = "An error occurred during registration. Please try again.";
        Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        
        // Reset the captcha
        await JSRuntime.InvokeVoidAsync("recaptchaFunctions.reset");
    }
    finally
    {
        // Reset loading state
        isLoading = false;
    }
}
```

## Step 11: Create a Review Service on the Client

1. Create a new service file at `Client/Services/ReviewService.cs`:

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CineScope.Client.Services.Auth;
using CineScope.Shared.DTOs;
using CineScope.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace CineScope.Client.Services
{
    public class ReviewService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;
        private readonly NavigationManager _navigationManager;

        public ReviewService(HttpClient httpClient, AuthService authService, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _authService = authService;
            _navigationManager = navigationManager;
        }

        public async Task<ReviewDto> SubmitReviewWithCaptchaAsync(ReviewDto review, string recaptchaResponse)
        {
            try
            {
                // Ensure the user is authenticated
                await _authService.EnsureAuthHeaderAsync();

                // Create request model
                var request = new ReviewWithCaptchaRequest
                {
                    Review = review,
                    RecaptchaResponse = recaptchaResponse
                };

                // Submit the review
                var response = await _httpClient.PostAsJsonAsync("api/Review/with-captcha", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ReviewDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to submit review: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting review: {ex.Message}");
                throw;
            }
        }
    }
}
```

2. Register this service in `Program.cs` in the client project:

```csharp
// In Client/Program.cs, add with your other service registrations
builder.Services.AddScoped<ReviewService>();
```

## Step 12: Update the CreateReview Component

1. Open `Client/Components/Reviews/CreateReview.razor`
2. Add the necessary using statements and injected services:

```csharp
@using CineScope.Client.Services
@using CineScope.Shared.Models
@inject ReviewService ReviewService
@inject IJSRuntime JSRuntime
```

3. Add the reCAPTCHA HTML element in the form, before the form actions:

```html
<div class="my-4">
    <div class="g-recaptcha" data-sitekey="6LczsgUrAAAAPC7YT5QIINbueIGE6GVjommFIBn"></div>
</div>
```

4. Update the `SubmitReview` method to include reCAPTCHA verification:

```csharp
private async Task SubmitReview()
{
    // Validate the form
    await form.Validate();

    Console.WriteLine($"After validation - success={success}, errors={string.Join(", ", errors)}");

    if (!success)
    {
        Console.WriteLine("Form validation failed - review not submitted");
        return;
    }

    // Check if user has selected a rating
    if (ratingValue == 0)
    {
        errorMessage = "Please select a rating";
        Console.WriteLine("Rating validation failed - no stars selected");
        return;
    }

    try
    {
        // Set loading state
        isLoading = true;
        errorMessage = string.Empty;
        Console.WriteLine("Starting review submission process");

        // Get reCAPTCHA response
        var recaptchaResponse = await JSRuntime.InvokeAsync<string>(
            "recaptchaFunctions.getResponse");

        if (string.IsNullOrEmpty(recaptchaResponse))
        {
            errorMessage = "Please complete the reCAPTCHA verification.";
            Snackbar.Add(errorMessage, Severity.Warning);
            return;
        }

        // First, validate content against banned word list
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

        // Set the rating from the stars component
        review.Rating = (double)ratingValue;

        // Set current date/time
        review.CreatedAt = DateTime.Now;

        Console.WriteLine($"Submitting review: MovieId={review.MovieId}, UserId={review.UserId}, Rating={review.Rating}");

        // Submit the review with captcha
        var createdReview = await ReviewService.SubmitReviewWithCaptchaAsync(review, recaptchaResponse);

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
    catch (Exception ex)
    {
        // Handle exceptions
        errorMessage = "An error occurred. Please try again.";
        Console.WriteLine($"Review submission exception: {ex.Message}");
        Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        
        // Reset the captcha
        await JSRuntime.InvokeVoidAsync("recaptchaFunctions.reset");
    }
    finally
    {
        // Reset loading state
        isLoading = false;
    }
}
```

5. Update the `Reset` method to also reset the reCAPTCHA:

```csharp
private async Task Reset()
{
    review.Rating = 0;
    ratingValue = 0;
    review.Text = string.Empty;
    errorMessage = string.Empty;
    contentWarning = null;
    
    // Reset the captcha
    await JSRuntime.InvokeVoidAsync("recaptchaFunctions.reset");
    
    StateHasChanged();
}
```

## Step 13: Add CSS Styling for reCAPTCHA

1. Create a new CSS file at `Client/wwwroot/css/recaptcha.css`:

```css
/* Responsive reCAPTCHA */
.g-recaptcha {
    transform-origin: left top;
    margin-bottom: 1rem;
}

@media screen and (max-width: 480px) {
    .g-recaptcha {
        transform: scale(0.85);
        margin-bottom: 0.5rem;
    }
}

/* Dark theme adjustments for reCAPTCHA */
.grecaptcha-badge {
    filter: invert(15%);
}

/* Center recaptcha for better alignment */
.recaptcha-container {
    display: flex;
    justify-content: center;
}

@media (max-width: 320px) {
    .g-recaptcha {
        transform: scale(0.77);
    }
}
```

2. Add this CSS to your `index.html`:

```html
<link href="css/recaptcha.css" rel="stylesheet" />
```

## Step 14: Test Your Implementation

1. Run your application
2. Try to register a new user without completing the reCAPTCHA - you should see an error
3. Register a user with completing the reCAPTCHA - it should work
4. Try to submit a review without completing the reCAPTCHA - you should see an error
5. Submit a review with completing the reCAPTCHA - it should work

## Step 15: Add Error Handling and Logging (Optional Enhancement)

To make your implementation more robust, add additional error handling and logging:

1. Add error logging to the client-side components:

```csharp
// In both components, add this to catch blocks
Console.Error.WriteLine($"reCAPTCHA error: {ex.Message}");
```

2. Add additional verification in the server-side code:

```csharp
// In the server controllers, add additional checks
if (!ModelState.IsValid || request?.RecaptchaResponse == null)
{
    _logger.LogWarning("Invalid request model or missing reCAPTCHA response");
    return BadRequest(ModelState);
}
```

## Final Step: Production Configuration

Before going to production:

1. Register your domain with Google reCAPTCHA console
2. Get production keys
3. Update your configuration in `appsettings.Production.json`
4. Consider implementing reCAPTCHA v3 for a more seamless user experience

Congratulations! You now have a fully implemented reCAPTCHA system protecting your CineScope application from spam and abuse.
