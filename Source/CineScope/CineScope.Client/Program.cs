using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CineScope.Client;
using CineScope.Client.ClientServices;
using CineScope.Client.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient with dynamic base address
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
);

// Register client services
builder.Services.AddScoped<MovieClientService>();
builder.Services.AddScoped<UserClientService>();
builder.Services.AddScoped<ReviewClientService>();

await builder.Build().RunAsync();