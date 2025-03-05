using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CineScope.Client;
using CineScope.Client.ClientServices;
using CineScope.Client.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient - make it use explicit headers
builder.Services.AddScoped(sp => {
    var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    httpClient.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    return httpClient;
});

// Register client-side services
builder.Services.AddScoped<MovieClientService>();
builder.Services.AddScoped<UserClientService>();
builder.Services.AddScoped<ReviewClientService>();

await builder.Build().RunAsync();