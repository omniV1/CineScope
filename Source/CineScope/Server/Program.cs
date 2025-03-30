using CineScope.Server.Interfaces;
using CineScope.Server.Services;
using CineScope.Server.Data;
using CineScope.Server;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Show the ASCII art intro at application startup (COMMENT OUT LINE BELOW TO SKIP ANIMATION FOR DEV PURPOSES)
ConsoleIntro.ShowIntro();

// Configure MongoDB serialization settings for better compatibility
ConfigureMongoDb();

var builder = WebApplication.CreateBuilder(args);

// Enhanced logging for troubleshooting
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
// Add this line to load user secrets in Development environment
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

/// <summary>
/// Configure MVC controllers and Razor Pages for the application.
/// </summary>
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

/// <summary>
/// Configure CORS policy for development
/// </summary>
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

/// <summary>
/// Configure MongoDB connection settings from appsettings.json.
/// Bind the MongoDbSettings section to the MongoDbSettings class.
/// </summary>
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(nameof(MongoDbSettings)));

/// <summary>
/// Configure movie cache settings from appsettings.json.
/// </summary>
builder.Services.Configure<MovieCacheSettings>(
    builder.Configuration.GetSection(nameof(MovieCacheSettings)));

/// <summary>
/// Add memory cache support for application-wide caching.
/// </summary>
builder.Services.AddMemoryCache();

/// <summary>
/// Configure JWT authentication
/// </summary>
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not found in configuration.")))
    };

    // For development purposes only - don't require HTTPS
    if (builder.Environment.IsDevelopment())
    {
        options.RequireHttpsMetadata = false;
    }
});

/// <summary>
/// Register MongoDB service as a singleton to maintain the connection
/// throughout the application's lifetime.
/// </summary>
builder.Services.AddSingleton<IMongoDbService, MongoDbService>();

/// <summary>
/// Register the caching services as singletons to maintain
/// cached data throughout the application's lifetime.
/// </summary>
builder.Services.AddSingleton<MovieCacheService>();

builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<DataSeedService>();

/// <summary>
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

    // Use CORS in development
    app.UseCors("AllowAll");

    // Show detailed exceptions
    app.UseDeveloperExceptionPage();
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
/// Configure authentication and authorization middleware.
/// Must be after UseRouting and before UseEndpoints/MapControllers.
/// </summary>
app.UseAuthentication();
app.UseAuthorization();

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
/// Configure MongoDB serialization
/// </summary>
void ConfigureMongoDb()
{
    // Register ObjectId serializer to handle string to ObjectId conversion
    BsonSerializer.RegisterSerializer(new ObjectIdSerializer());

    // Note: MongoDB client logging will be configured in the MongoDbService
    // when creating the client instance

    Console.WriteLine("MongoDB serialization configured successfully");
}

// Seed data and create indexes
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<DataSeedService>();
    await seedService.SeedInitialDataAsync();
}

/// <summary>
/// Start the application.
/// </summary>
app.Run();
