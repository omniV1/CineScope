using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CineScope.Client;
using CineScope.Client.ClientServices;


var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Register client services
builder.Services.AddScoped<MovieClientService>();
builder.Services.AddScoped<UserClientService>();
builder.Services.AddScoped<ReviewClientService>();

await builder.Build().RunAsync();
