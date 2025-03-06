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
    .AddInteractiveWebAssemblyComponents(); // This should now work

// Add controllers with explicit assembly scanning
var mvcBuilder = builder.Services.AddControllers();
mvcBuilder.AddApplicationPart(typeof(Program).Assembly);

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
app.UseRouting();
app.UseCors("AllowAll");
app.UseAntiforgery();

Console.WriteLine("Client Assembly: " + typeof(CineScope.Client._Imports).Assembly.FullName);

// Then after all mapping is done
foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
{
    if (asm.FullName.Contains("CineScope"))
    {
        Console.WriteLine("Loaded: " + asm.FullName);
    }
}

// Map controllers
app.MapControllers();

// Map Razor components - MODIFIED THIS SECTION
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

app.Run();
