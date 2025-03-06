using CineScope.Client.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components.WebAssembly.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add controllers with explicit assembly scanning
var mvcBuilder = builder.Services.AddControllers();
mvcBuilder.AddApplicationPart(typeof(Program).Assembly);
mvcBuilder.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// Add Razor Pages (without runtime compilation since you're missing the package)
builder.Services.AddRazorPages();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("http://localhost:5000", "https://localhost:5001")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
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

// Controller debugging
builder.Logging.AddFilter("Microsoft.AspNetCore.Mvc", LogLevel.Debug);

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

app.UseStaticFiles();
app.UseBlazorFrameworkFiles(); // Serve Blazor WebAssembly files
app.UseRouting();
app.UseCors("AllowAll");
app.UseAntiforgery();

// Logging assemblies for debugging
Console.WriteLine("Client Assembly: " + typeof(CineScope.Client._Imports).Assembly.FullName);
foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
{
    if (asm.FullName.Contains("CineScope"))
    {
        Console.WriteLine("Loaded: " + asm.FullName);
    }
}

// Map controllers
app.MapControllers();

// Initialize MongoDB
var indexService = app.Services.GetRequiredService<MongoDBIndexService>();
indexService.CreateIndexesAsync().GetAwaiter().GetResult();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
    seeder.SeedDatabaseAsync().GetAwaiter().GetResult();
}

// This must be the last middleware in the pipeline
app.MapFallbackToFile("index.html");

app.Run();