using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CineScope.Client;
using MudBlazor.Services;

/// <summary>
/// Entry point for the Blazor WebAssembly client application.
/// Configures and initializes the client-side services and components.
/// </summary>
var builder = WebAssemblyHostBuilder.CreateDefault(args);

/// <summary>
/// Register the root App component to render in the browser.
/// This is the main entry point for the Blazor UI.
/// </summary>
builder.RootComponents.Add<App>("#app");

/// <summary>
/// Register the HeadOutlet component for managing document head content.
/// Used for dynamic page titles and meta tags.
/// </summary>
builder.RootComponents.Add<HeadOutlet>("head::after");

/// <summary>
/// Register an HttpClient instance for making API requests.
/// Configured to target the base address of the host environment.
/// </summary>
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

/// <summary>
/// Register MudBlazor UI component services.
/// Enables the use of MudBlazor components in the application.
/// </summary>
builder.Services.AddMudServices();

/// <summary>
/// Build and run the WebAssembly application.
/// </summary>
await builder.Build().RunAsync();