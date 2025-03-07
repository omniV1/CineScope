// Program.cs - Main entry point for the CineScope application
// This file configures and initializes the ASP.NET Core web application

using CineScope.Client.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.StaticFiles;

// Create the WebApplication builder - the entry point for configuring the application
var builder = WebApplication.CreateBuilder(args);

// Add Blazor services to the container
// This configures the application to use both Blazor Server and WebAssembly rendering modes
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()    // Enable Blazor Server components for server-side rendering
    .AddInteractiveWebAssemblyComponents(); // Enable Blazor WebAssembly components for client-side rendering

// Add MVC Controllers with custom JSON serialization options
// - Preserves property names as-is (no camelCase conversion)
// - Enables case-insensitive property matching for more flexible API usage
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Add support for Razor Pages (used for error handling and some static views)
builder.Services.AddRazorPages();

// Configure Cross-Origin Resource Sharing (CORS) policy
// This policy allows API access from different origins - important for separated frontend/backend development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure MongoDB conventions to make document mapping more flexible
// The IgnoreExtraElements setting allows the app to work even when database schema differs slightly
var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

// Register MongoDB connection settings from appsettings.json
// This maps the configuration section to a strongly-typed settings object
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection(nameof(MongoDBSettings)));
builder.Services.AddSingleton<MongoDBSettings>(sp =>
    sp.GetRequiredService<IOptions<MongoDBSettings>>().Value);

// Register database infrastructure services
// - MongoDBIndexService: Creates and maintains database indexes
// - DatabaseSeederService: Populates the database with initial data if empty
builder.Services.AddSingleton<MongoDBIndexService>();
builder.Services.AddScoped<DatabaseSeederService>();

// Register repositories (data access layer)
// These provide a clean abstraction over MongoDB collections
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IBannedWordRepository, BannedWordRepository>();

// Register business logic services
// These implement application business rules and work with repositories
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IContentFilterService, ContentFilterService>();

// Add HttpClient for API communication between client and server components
builder.Services.AddHttpClient();

// Configure HttpClient with appropriate base address
// Using localhost in development or configured URL in production
builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri(builder.Environment.IsDevelopment()
        ? "http://localhost:5000/api"
        : builder.Configuration["ApplicationUrl"] ?? "/")
    });

// Register client-side services for Blazor WebAssembly
// These services provide an API wrapper for the client components
builder.Services.AddScoped<CineScope.Client.ClientServices.MovieClientService>();
builder.Services.AddScoped<CineScope.Client.ClientServices.UserClientService>();
builder.Services.AddScoped<CineScope.Client.ClientServices.ReviewClientService>();

// Configure detailed logging for development and troubleshooting
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Build the application with all configured services
var app = builder.Build();

// Configure the HTTP request pipeline based on environment
if (app.Environment.IsDevelopment())
{
    // In development, show detailed error pages and enable WebAssembly debugging
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    // In production, redirect to a generic error page
    app.UseExceptionHandler("/Error");
}

// Configure MIME types for Blazor WebAssembly files
// These mappings ensure the browser correctly interprets WebAssembly files
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".dll"] = "application/octet-stream";
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".dat"] = "application/octet-stream";
provider.Mappings[".blat"] = "application/octet-stream";

// Serve static files with the configured MIME types
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

// Enable serving Blazor WebAssembly framework files (required for client-side Blazor)
app.UseBlazorFrameworkFiles();

// Set up routing middleware to handle request routing
app.UseRouting();

// Apply the CORS policy to allow cross-origin requests
app.UseCors("AllowAll");

// Add anti-forgery protection (required for Blazor forms)
app.UseAntiforgery();

// Map API controllers to handle REST API requests
app.MapControllers();

// Map Razor Pages for server-side rendered pages
app.MapRazorPages();

// Configure Blazor component routing, supporting both server and WebAssembly rendering
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()    // Support Server-side rendering (faster initial load)
    .AddInteractiveWebAssemblyRenderMode();  // Support WebAssembly rendering (client-side interactivity)

// Initialize MongoDB indexes before the application starts
// This ensures the database has proper indexing for efficient queries
var indexService = app.Services.GetRequiredService<MongoDBIndexService>();
indexService.CreateIndexesAsync().GetAwaiter().GetResult();

// Seed the database with initial data if it's empty
// This ensures the application has some data to work with on first run
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
    seeder.SeedDatabaseAsync().GetAwaiter().GetResult();
}

// Configure fallback route to index.html for SPA navigation
// This enables client-side routing to work with page refreshes
app.MapFallbackToFile("index.html");

// Start the application and begin listening for requests
app.Run();