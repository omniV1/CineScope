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

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient with auth header handler
builder.Services.AddScoped<AuthenticationHeaderHandler>();
builder.Services.AddScoped(sp =>
{
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var handler = new AuthenticationHeaderHandler(localStorage);
    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
    return httpClient;
});

// Add Blazored LocalStorage for JWT token storage
builder.Services.AddBlazoredLocalStorage();

// Add the client-side movie cache service
builder.Services.AddScoped<ClientMovieCacheService>();

// Register the poster cache service
builder.Services.AddScoped<CineScope.Client.Services.MoviePosterCacheService>();

// Add Authentication Services
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<AuthStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddAuthorizationCore();

// Add MudBlazor services with configuration
builder.Services.AddMudServices(config => {
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    // Explicitly set theme colors here as a backup 
    var theme = new MudTheme()
    {
        PaletteDark = new PaletteDark()
        {
            Black = "#0f0f0f",
            White = "#ffffff",
            Primary = "#E50914", // Updated to logo red
            Secondary = "#f5f5f1", // Off-white for contrast
            Success = "#3bef9e",
            Error = "#ff3f5b",
            Warning = "#ffb527",
            Info = "#2196f3",
            Background = "#0f0f0f",
            BackgroundGrey = "#1a1a1a",
            Surface = "#1a1a1a",
            AppbarBackground = "#0f0f0f",
            AppbarText = "#ffffff",
            DrawerBackground = "#1a1a1a",
            DrawerText = "#ffffff",
            TextPrimary = "#ffffff",
            TextSecondary = "#b3b3b3",
            ActionDefault = "#ffffff",
            ActionDisabled = "#636363",
            ActionDisabledBackground = "#2c2c2c",
            LinesDefault = "#2c2c2c",
            LinesInputs = "#4a4a4a",
            TableLines = "#2c2c2c",
            TableStriped = "#2c2c2c"
        },
        Typography = new Typography()
        {
            Default = new Default()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "1rem",
                FontWeight = 400,
                LineHeight = 1.5,
                LetterSpacing = ".00938em"
            },
            H1 = new H1()
            {
                FontSize = "2.5rem",
                FontWeight = 700,
                LineHeight = 1.167,
                LetterSpacing = "-.01562em"
            },
            H2 = new H2()
            {
                FontSize = "2rem",
                FontWeight = 700,
                LineHeight = 1.2,
                LetterSpacing = "-.00833em"
            }
        }
    };
});

await builder.Build().RunAsync();

// Token-attaching HTTP handler
public class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthenticationHeaderHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Try to get the token from local storage
        var token = await _localStorage.GetItemAsync<string>("authToken");

        // If token exists, add it to the Authorization header
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Console.WriteLine($"Added auth token to request: {request.RequestUri}");
        }
        else
        {
            Console.WriteLine($"No auth token available for request: {request.RequestUri}");
        }

        // Pass the request to the inner handler
        return await base.SendAsync(request, cancellationToken);
    }
}
