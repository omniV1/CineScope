// These are the basic tools our app needs to work
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CineScope.Client;
using MudBlazor;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using CineScope.Client.Services.Auth;
using CineScope.Client.Services;

// This is where our app starts running
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Tell the app where to start - look for the App component and put it in the HTML element with id="app"
builder.RootComponents.Add<App>("#app");

// Add support for special browser features like the page title
builder.RootComponents.Add<HeadOutlet>("head::after");

// Set up how our app will talk to the server
// First, we create a special handler that will add authentication information to our requests
builder.Services.AddScoped<AuthenticationHeaderHandler>();

// Then we set up the main way our app talks to the server (HttpClient)
builder.Services.AddScoped(sp =>
{
    // Get the service that helps us store data in the browser
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    
    // Create our authentication handler
    var handler = new AuthenticationHeaderHandler(localStorage);
    
    // Create the main communication tool (HttpClient) with our handler
    var httpClient = new HttpClient(handler)
    {
        // Tell it where our server is located
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
    return httpClient;
});

// Load our app's settings from a configuration file
builder.Configuration.AddJsonFile("appsettings.json", optional: true);

// Add storage service so we can save data in the browser
builder.Services.AddBlazoredLocalStorage();

// Add all the services our app needs
builder.Services.AddScoped<MovieCacheService>();  // For storing movie data temporarily
builder.Services.AddScoped<AnthropicService>();   // For AI-powered features
builder.Services.AddScoped<StateContainer>();     // For sharing data between components

// Set up user authentication
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<AuthStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RecaptchaService>();
builder.Services.AddAuthorizationCore();

// Add MudBlazor services (this gives us pretty UI components)
builder.Services.AddMudServices(config =>
{
    // Configure how notifications appear
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
});

// Start the application!
await builder.Build().RunAsync();

// This class helps add authentication information to our requests to the server
public class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    // Constructor - sets up the handler
    public AuthenticationHeaderHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
        InnerHandler = new HttpClientHandler();
    }

    // This runs every time we make a request to the server
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Get the authentication token from storage
        var token = await _localStorage.GetItemAsync<string>("authToken");
        
        // If we have a token, add it to the request
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Send the request to the server
        return await base.SendAsync(request, cancellationToken);
    }
}
