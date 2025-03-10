using CineScope.Server.Interfaces;
using CineScope.Server.Services;
using CineScope.Server.Data;
using CineScope.Server;
// Show the ASCII art intro at application startup
ConsoleIntro.ShowIntro();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

/// <summary>
/// Configure MVC controllers and Razor Pages for the application.
/// </summary>
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

/// <summary>
/// Configure MongoDB connection settings from appsettings.json.
/// Bind the MongoDbSettings section to the MongoDbSettings class.
/// </summary>
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(nameof(MongoDbSettings)));

/// <summary>
/// Register MongoDB service as a singleton to maintain the connection
/// throughout the application's lifetime.
/// </summary>
builder.Services.AddSingleton<IMongoDbService, MongoDbService>();

/// Register services as scoped services.
/// Scoped services are created once per HTTP request.
/// </summary>
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<ContentFilterService>();
builder.Services.AddScoped<UserService>();

/// <summary>
/// Build the application from the configured services.
/// </summary>
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    /// <summary>
    /// Enable WebAssembly debugging support for Blazor applications
    /// when running in development mode.
    /// </summary>
    app.UseWebAssemblyDebugging();
}
else
{
    /// <summary>
    /// Configure error handling for production environment.
    /// Redirects to /Error page when exceptions occur.
    /// </summary>
    app.UseExceptionHandler("/Error");

    /// <summary>
    /// Enable HTTP Strict Transport Security (HSTS) for enhanced security.
    /// Forces browsers to use HTTPS for all requests to this domain.
    /// </summary>
    app.UseHsts();
}

/// <summary>
/// Redirect HTTP requests to HTTPS for secure communication.
/// </summary>
app.UseHttpsRedirection();

/// <summary>
/// Enable serving Blazor WebAssembly files to the client.
/// </summary>
app.UseBlazorFrameworkFiles();

/// <summary>
/// Enable serving static files (CSS, JavaScript, images).
/// </summary>
app.UseStaticFiles();

/// <summary>
/// Configure routing middleware for the application.
/// </summary>
app.UseRouting();

/// <summary>
/// Map Razor Pages routes for server-side pages.
/// </summary>
app.MapRazorPages();

/// <summary>
/// Map controller routes for API endpoints.
/// </summary>
app.MapControllers();

/// <summary>
/// Configure fallback route to serve index.html for any unmatched routes.
/// This is essential for client-side routing in Blazor WebAssembly.
/// </summary>
app.MapFallbackToFile("index.html");

/// <summary>
/// Start the application.
/// </summary>
app.Run();