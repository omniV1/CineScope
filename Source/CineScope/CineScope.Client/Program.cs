using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CineScope.Client;
using System.Net.Http.Json;
using CineScope.Client.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register client-side services for API access
builder.Services.AddScoped<CineScope.Client.Services.MovieClientService>();
// Add other client services as needed

await builder.Build().RunAsync();
