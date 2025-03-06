using CineScope.Client.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Add Razor Pages
builder.Services.AddRazorPages();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure MongoDB conventions
var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

// Configure MongoDB Settings
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection(nameof(MongoDBSettings)));
builder.Services.AddSingleton<MongoDBSettings>(sp =>
    sp.GetRequiredService<IOptions<MongoDBSettings>>().Value);

// Register services
builder.Services.AddSingleton<MongoDBIndexService>();
builder.Services.AddScoped<DatabaseSeederService>();

// Register repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IBannedWordRepository, BannedWordRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IContentFilterService, ContentFilterService>();

// Add HttpClient for client services
builder.Services.AddHttpClient();

// Register client services for server-side prerendering
builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri(builder.Environment.IsDevelopment()
        ? "http://localhost:5000/"
        : builder.Configuration["ApplicationUrl"] ?? "/")
    });

// Register the client services
builder.Services.AddScoped<CineScope.Client.ClientServices.MovieClientService>();
builder.Services.AddScoped<CineScope.Client.ClientServices.UserClientService>();
builder.Services.AddScoped<CineScope.Client.ClientServices.ReviewClientService>();

// Set up detailed logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

// Configure MIME types for Blazor WebAssembly
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".dll"] = "application/octet-stream";
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".dat"] = "application/octet-stream";
provider.Mappings[".blat"] = "application/octet-stream";

// Use static files with the configured MIME types
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseBlazorFrameworkFiles();
app.UseRouting();
app.UseCors("AllowAll");

// Add anti-forgery middleware (required for Blazor forms)
app.UseAntiforgery();

// Map controllers
app.MapControllers();

// Map Razor Pages and Blazor components
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();
    


// Initialize MongoDB
var indexService = app.Services.GetRequiredService<MongoDBIndexService>();
indexService.CreateIndexesAsync().GetAwaiter().GetResult();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
    seeder.SeedDatabaseAsync().GetAwaiter().GetResult();
}

// Fallback route
app.MapFallbackToFile("index.html");

app.Run();