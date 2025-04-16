# MongoDB Integration Guide for CineScope Blazor Application

This guide will walk you through implementing MongoDB integration with your C# models for the CineScope Blazor project. We'll cover setting up the MongoDB connection, creating model classes, implementing repositories, configuring dependency injection, and Blazor-specific integration points.

## Table of Contents
1. [Project Setup](#project-setup)
2. [Model Creation](#model-creation)
3. [MongoDB Configuration](#mongodb-configuration)
4. [Repository Implementation](#repository-implementation)
5. [Service Layer Integration](#service-layer-integration)
6. [Dependency Injection Setup](#dependency-injection-setup)
7. [Blazor Integration](#blazor-integration)
8. [Testing Your Implementation](#testing-your-implementation)
9. [Common Issues and Solutions](#common-issues-and-solutions)

## Project Setup

First, make sure you have the required NuGet packages installed:

```bash
dotnet add package MongoDB.Driver
```

## Model Creation

### Step 1: Create Model Classes

Create your model classes in a dedicated Models folder. Each model should include the MongoDB attributes to map to the database.

#### Example: User Model

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace CineScope.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("username")]
        public string Username { get; set; }
        
        [BsonElement("email")]
        public string Email { get; set; }
        
        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; }
        
        [BsonElement("roles")]
        public List<string> Roles { get; set; } = new List<string>();
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [BsonElement("lastLogin")]
        public DateTime? LastLogin { get; set; }
        
        [BsonElement("isLocked")]
        public bool IsLocked { get; set; } = false;
        
        [BsonElement("failedLoginAttempts")]
        public int FailedLoginAttempts { get; set; } = 0;
    }
}
```

#### Example: Movie Model

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace CineScope.Models
{
    public class Movie
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("title")]
        public string Title { get; set; }
        
        [BsonElement("description")]
        public string Description { get; set; }
        
        [BsonElement("releaseDate")]
        public DateTime ReleaseDate { get; set; }
        
        [BsonElement("genres")]
        public List<string> Genres { get; set; } = new List<string>();
        
        [BsonElement("director")]
        public string Director { get; set; }
        
        [BsonElement("actors")]
        public List<string> Actors { get; set; } = new List<string>();
        
        [BsonElement("posterUrl")]
        public string PosterUrl { get; set; }
        
        [BsonElement("averageRating")]
        public double AverageRating { get; set; } = 0;
        
        [BsonElement("reviewCount")]
        public int ReviewCount { get; set; } = 0;
    }
}
```

#### Example: Review Model

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace CineScope.Models
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        
        [BsonElement("movieId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MovieId { get; set; }
        
        [BsonElement("rating")]
        public double Rating { get; set; }
        
        [BsonElement("text")]
        public string Text { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        [BsonElement("isApproved")]
        public bool IsApproved { get; set; } = true;
        
        [BsonElement("flaggedWords")]
        public List<string> FlaggedWords { get; set; } = new List<string>();
    }
}
```

#### Example: BannedWord Model

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CineScope.Models
{
    public class BannedWord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("word")]
        public string Word { get; set; }
        
        [BsonElement("severity")]
        public int Severity { get; set; }
        
        [BsonElement("category")]
        public string Category { get; set; }
        
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
        
        [BsonElement("addedAt")]
        public DateTime AddedAt { get; set; } = DateTime.Now;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
```

### Important Notes on Model Creation

1. **MongoDB IDs**: The `[BsonId]` attribute marks the primary key field. By default, MongoDB uses `ObjectId` type.

2. **Field Mapping**: The `[BsonElement]` attribute maps the C# property to the MongoDB field name.

3. **Data Type Representation**: `[BsonRepresentation]` specifies how to convert between .NET and BSON types.

4. **Relationships**: MongoDB doesn't enforce relationships like SQL. Foreign keys (like `UserId` in `Review`) are stored but not enforced by the database.

## MongoDB Configuration

### Step 1: Create Database Settings Class

Create a class to hold your MongoDB connection settings:

```csharp
namespace CineScope.Settings
{
    public class MongoDBSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string UsersCollectionName { get; set; }
        public string MoviesCollectionName { get; set; }
        public string ReviewsCollectionName { get; set; }
        public string BannedWordsCollectionName { get; set; }
    }
}
```

### Step 2: Configure Settings in appsettings.json

Add MongoDB settings to your `appsettings.json` file:

```json
{
  "MongoDBSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "CineScopeDb",
    "UsersCollectionName": "Users",
    "MoviesCollectionName": "Movies",
    "ReviewsCollectionName": "Reviews",
    "BannedWordsCollectionName": "BannedWords"
  }
}
```

### Step 3: Register Settings in Startup.cs

Configure MongoDB settings in your `Startup.cs` file:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure MongoDB Settings
    services.Configure<MongoDBSettings>(
        Configuration.GetSection(nameof(MongoDBSettings)));
    
    services.AddSingleton<IMongoDBSettings>(sp =>
        sp.GetRequiredService<IOptions<MongoDBSettings>>().Value);
        
    // Other service configurations...
}
```

## Repository Implementation

### Step 1: Create Repository Interfaces

Create interfaces for each repository in a dedicated `Interfaces` folder:

```csharp
using CineScope.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User> GetByIdAsync(string id);
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(string id, User user);
        Task DeleteAsync(string id);
    }
}
```

Create similar interfaces for `IMovieRepository`, `IReviewRepository`, and `IBannedWordRepository`.

### Step 2: Implement Repository Classes

Create concrete repository implementations in a `Repositories` folder:

```csharp
using CineScope.Interfaces;
using CineScope.Models;
using CineScope.Settings;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(MongoDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>(settings.UsersCollectionName);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _users.Find(user => true).ToListAsync();
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _users.Find(user => user.Username == username).FirstOrDefaultAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task UpdateAsync(string id, User updatedUser)
        {
            await _users.ReplaceOneAsync(user => user.Id == id, updatedUser);
        }

        public async Task DeleteAsync(string id)
        {
            await _users.DeleteOneAsync(user => user.Id == id);
        }
    }
}
```

Implement similar repository classes for `MovieRepository`, `ReviewRepository`, and `BannedWordRepository`.

### Step 3: Create Indexes for Performance

To implement the indexing strategy mentioned in your technical design document, create a service that sets up indexes when the application starts:

```csharp
using CineScope.Settings;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace CineScope.Services
{
    public class MongoDBIndexService
    {
        private readonly IMongoDatabase _database;

        public MongoDBIndexService(MongoDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        public async Task CreateIndexesAsync()
        {
            // User collection indexes
            var usersCollection = _database.GetCollection<Models.User>("Users");
            await usersCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.User>(
                    Builders<Models.User>.IndexKeys.Ascending(user => user.Username),
                    new CreateIndexOptions { Unique = true }));
            
            await usersCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.User>(
                    Builders<Models.User>.IndexKeys.Ascending(user => user.Email),
                    new CreateIndexOptions { Unique = true }));
            
            await usersCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.User>(
                    Builders<Models.User>.IndexKeys.Descending(user => user.LastLogin)));

            // Movie collection indexes
            var moviesCollection = _database.GetCollection<Models.Movie>("Movies");
            await moviesCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.Movie>(
                    Builders<Models.Movie>.IndexKeys.Text(movie => movie.Title)));
            
            await moviesCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.Movie>(
                    Builders<Models.Movie>.IndexKeys.Ascending(movie => movie.Genres)));
            
            await moviesCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.Movie>(
                    Builders<Models.Movie>.IndexKeys.Descending(movie => movie.ReleaseDate)));
            
            await moviesCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.Movie>(
                    Builders<Models.Movie>.IndexKeys.Descending(movie => movie.AverageRating)));

            // Review collection indexes
            var reviewsCollection = _database.GetCollection<Models.Review>("Reviews");
            await reviewsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.Review>(
                    Builders<Models.Review>.IndexKeys.Ascending(review => review.MovieId)));
            
            await reviewsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.Review>(
                    Builders<Models.Review>.IndexKeys.Ascending(review => review.UserId)));
            
            await reviewsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.Review>(
                    Builders<Models.Review>.IndexKeys.Descending(review => review.CreatedAt)));
            
            await reviewsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.Review>(
                    Builders<Models.Review>.IndexKeys
                        .Ascending(review => review.MovieId)
                        .Descending(review => review.CreatedAt)));
        }
    }
}
```

## Service Layer Integration

### Step 1: Create Service Interfaces

Create service interfaces in the `Interfaces` folder:

```csharp
using CineScope.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Interfaces
{
    public interface IMovieService
    {
        Task<List<Movie>> GetAllMoviesAsync();
        Task<Movie> GetMovieByIdAsync(string id);
        Task<List<Movie>> GetMoviesByGenreAsync(string genre);
        Task<List<Movie>> GetTopRatedMoviesAsync(int limit = 10);
        Task<List<Movie>> GetRecentMoviesAsync(int limit = 10);
        Task<Movie> CreateMovieAsync(Movie movie);
        Task UpdateMovieAsync(string id, Movie movie);
        Task DeleteMovieAsync(string id);
    }
}
```

Create similar interfaces for `IUserService`, `IReviewService`, etc.

### Step 2: Implement Service Classes

Implement the service classes in a `Services` folder:

```csharp
using CineScope.Interfaces;
using CineScope.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;

        public MovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public async Task<List<Movie>> GetAllMoviesAsync()
        {
            return await _movieRepository.GetAllAsync();
        }

        public async Task<Movie> GetMovieByIdAsync(string id)
        {
            return await _movieRepository.GetByIdAsync(id);
        }

        public async Task<List<Movie>> GetMoviesByGenreAsync(string genre)
        {
            return await _movieRepository.GetByGenreAsync(genre);
        }

        public async Task<List<Movie>> GetTopRatedMoviesAsync(int limit = 10)
        {
            return await _movieRepository.GetTopRatedAsync(limit);
        }

        public async Task<List<Movie>> GetRecentMoviesAsync(int limit = 10)
        {
            return await _movieRepository.GetRecentAsync(limit);
        }

        public async Task<Movie> CreateMovieAsync(Movie movie)
        {
            movie.CreatedAt = DateTime.Now;
            return await _movieRepository.CreateAsync(movie);
        }

        public async Task UpdateMovieAsync(string id, Movie movie)
        {
            await _movieRepository.UpdateAsync(id, movie);
        }

        public async Task DeleteMovieAsync(string id)
        {
            await _movieRepository.DeleteAsync(id);
        }
    }
}
```

## Dependency Injection Setup

### Configure Dependency Injection in Startup.cs

Register your repositories and services in `Startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // MongoDB Settings
    services.Configure<MongoDBSettings>(
        Configuration.GetSection(nameof(MongoDBSettings)));
    
    services.AddSingleton<IMongoDBSettings>(sp =>
        sp.GetRequiredService<IOptions<MongoDBSettings>>().Value);
    
    // Register MongoDB Index Service
    services.AddSingleton<MongoDBIndexService>();
    
    // Register Repositories
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IMovieRepository, MovieRepository>();
    services.AddScoped<IReviewRepository, ReviewRepository>();
    services.AddScoped<IBannedWordRepository, BannedWordRepository>();
    
    // Register Services
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IMovieService, MovieService>();
    services.AddScoped<IReviewService, ReviewService>();
    services.AddScoped<IContentFilterService, ContentFilterService>();
    
    // Other service configurations...
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Other middleware configurations...
    
    // Create MongoDB indexes at application startup
    var indexService = app.ApplicationServices.GetRequiredService<MongoDBIndexService>();
    indexService.CreateIndexesAsync().Wait();
}
```

## Testing Your Implementation

### Create a Simple Test Controller

To quickly test your MongoDB integration, create a simple test controller:

```csharp
using CineScope.Interfaces;
using CineScope.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public TestController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet("movies")]
        public async Task<ActionResult<List<Movie>>> GetMovies()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            return Ok(movies);
        }

        [HttpPost("movies")]
        public async Task<ActionResult<Movie>> CreateMovie([FromBody] Movie movie)
        {
            var newMovie = await _movieService.CreateMovieAsync(movie);
            return CreatedAtAction(nameof(GetMovies), new { id = newMovie.Id }, newMovie);
        }
    }
}
```

## Common Issues and Solutions

### 1. Connection String Issues

**Problem**: Unable to connect to MongoDB.
**Solution**: Verify your connection string format.

For local development:
```
mongodb://localhost:27017
```

For MongoDB Atlas:
```
mongodb+srv://<username>:<password>@<cluster-address>/test?retryWrites=true&w=majority
```

### 2. ObjectId Issues

**Problem**: "Invalid ObjectId" errors when working with IDs.
**Solution**: Ensure you're handling ObjectId creation and conversion correctly.

```csharp
// Valid way to create a new ObjectId
string newId = ObjectId.GenerateNewId().ToString();

// Check if a string is a valid ObjectId
if (ObjectId.TryParse(idString, out _))
{
    // Valid ObjectId
}
```

### 3. Handling References Across Collections

**Problem**: Managing relationships between collections.
**Solution**: Implement lookup operations in your repositories.

Example: Getting a movie with its reviews:

```csharp
public async Task<Movie> GetMovieWithReviewsAsync(string movieId)
{
    var movie = await _movies.Find(m => m.Id == movieId).FirstOrDefaultAsync();
    
    if (movie != null)
    {
        var reviews = await _reviews.Find(r => r.MovieId == movieId).ToListAsync();
        // Add reviews to a movie object property or create a view model
    }
    
    return movie;
}
```

### 4. Index Creation Failures

**Problem**: Unable to create unique indexes due to duplicate data.
**Solution**: Clean up duplicate data before creating unique indexes.

```csharp
// Find and remove duplicates before creating unique index
var duplicateUsers = _users.Aggregate()
    .Group(u => u.Username, g => new { Username = g.Key, Count = g.Count() })
    .Match(g => g.Count > 1)
    .ToList();

foreach (var dup in duplicateUsers)
{
    var dupUsers = _users.Find(u => u.Username == dup.Username).ToList();
    // Keep one and delete the rest
    for (int i = 1; i < dupUsers.Count; i++)
    {
        _users.DeleteOne(u => u.Id == dupUsers[i].Id);
    }
}
```

## Advanced Topics

### 1. Optimizing Queries with Projections

To optimize query performance, use projections to limit returned fields:

```csharp
public async Task<List<MovieSummary>> GetMovieSummariesAsync()
{
    var projection = Builders<Movie>.Projection
        .Include(m => m.Id)
        .Include(m => m.Title)
        .Include(m => m.PosterUrl)
        .Include(m => m.AverageRating);
    
    return await _movies.Find(FilterDefinition<Movie>.Empty)
        .Project<MovieSummary>(projection)
        .ToListAsync();
}
```

### 2. Implementing Pagination

For large collections, implement pagination to limit the amount of data transferred:

```csharp
public async Task<List<Movie>> GetPaginatedMoviesAsync(int page, int pageSize)
{
    return await _movies.Find(FilterDefinition<Movie>.Empty)
        .Skip((page - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();
}
```

### 3. Implementing Search with Text Indexes

To implement the movie search feature, use MongoDB's text search capabilities:

```csharp
public async Task<List<Movie>> SearchMoviesAsync(string searchTerm)
{
    var filter = Builders<Movie>.Filter.Text(searchTerm);
    
    return await _movies.Find(filter)
        .Sort(Builders<Movie>.Sort.MetaTextScore("score"))
        .ToListAsync();
}
```

Remember to create a text index on the relevant fields first:

```csharp
await moviesCollection.Indexes.CreateOneAsync(
    new CreateIndexModel<Models.Movie>(
        Builders<Models.Movie>.IndexKeys.Text(m => m.Title).Text(m => m.Description)
    )
);
```

## Blazor Integration

Since CineScope is being built as a Blazor C# ASP.NET Core Web App (MVC) application, there are specific considerations for integrating MongoDB with Blazor components. This section covers the key aspects of using your MongoDB repositories and services within the Blazor framework.

### Dependency Injection in Blazor Components

Blazor has built-in dependency injection that makes it easy to use your services directly in components using the `@inject` directive:

```razor
@page "/movies"
@inject IMovieService MovieService

<h1>Movies</h1>

@if (movies == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <div class="movie-grid">
        @foreach (var movie in movies)
        {
            <div class="movie-card">
                <img src="@movie.PosterUrl" alt="@movie.Title" />
                <h3>@movie.Title</h3>
                <div class="rating">@movie.AverageRating.ToString("0.0")</div>
            </div>
        }
    </div>
}

@code {
    private List<Movie> movies;

    protected override async Task OnInitializedAsync()
    {
        movies = await MovieService.GetAllMoviesAsync();
    }
}
```

### State Management in Blazor

Blazor components maintain their own state, so you might want to implement caching to optimize database calls and improve performance:

```csharp
public class CachedMovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMemoryCache _cache;

    public CachedMovieService(IMovieRepository movieRepository, IMemoryCache cache)
    {
        _movieRepository = movieRepository;
        _cache = cache;
    }

    public async Task<List<Movie>> GetTopRatedMoviesAsync(int limit = 10)
    {
        string cacheKey = $"TopRatedMovies_{limit}";
        if (!_cache.TryGetValue(cacheKey, out List<Movie> movies))
        {
            movies = await _movieRepository.GetTopRatedAsync(limit);
            _cache.Set(cacheKey, movies, TimeSpan.FromMinutes(10));
        }
        return movies;
    }
    
    // Other methods...
}
```

To use this cached service, register it in `Startup.cs`:

```csharp
services.AddMemoryCache();
services.AddScoped<IMovieService, CachedMovieService>();
```

### Component Communication for Real-time Updates

For features like real-time review updates or notifications, implement component communication:

#### Using EventCallback for Parent-Child Communication

```razor
@* ParentComponent.razor *@
<ChildComponent OnReviewAdded="HandleReviewAdded" />

@code {
    private async Task HandleReviewAdded(Review review)
    {
        // Handle the new review
        await RefreshMovieData();
    }
}

@* ChildComponent.razor *@
@inject IReviewService ReviewService

<button @onclick="AddReview">Submit Review</button>

@code {
    [Parameter]
    public EventCallback<Review> OnReviewAdded { get; set; }
    
    private Review newReview = new Review();
    
    private async Task AddReview()
    {
        var createdReview = await ReviewService.CreateReviewAsync(newReview);
        await OnReviewAdded.InvokeAsync(createdReview);
    }
}
```

#### Using a State Container Service for Unrelated Components

```csharp
// StateContainer.cs
public class StateContainer
{
    private List<Movie> featuredMovies = new List<Movie>();
    
    public List<Movie> FeaturedMovies 
    { 
        get => featuredMovies; 
        set 
        { 
            featuredMovies = value; 
            NotifyStateChanged(); 
        } 
    }
    
    public event Action OnChange;
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}
```

Register in `Startup.cs`:

```csharp
services.AddScoped<StateContainer>();
```

Use in components:

```razor
@inject StateContainer State
@implements IDisposable

@code {
    protected override void OnInitialized()
    {
        State.OnChange += StateHasChanged;
    }

    public void Dispose()
    {
        State.OnChange -= StateHasChanged;
    }
}
```

### Working with Forms in Blazor

For forms like movie reviews, use Blazor's form handling capabilities:

```razor
@page "/review/{MovieId}"
@inject IReviewService ReviewService
@inject NavigationManager NavigationManager

<h3>Write a Review for @movie?.Title</h3>

<EditForm Model="@review" OnValidSubmit="HandleValidSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <div class="form-group">
        <label>Rating:</label>
        <InputNumber @bind-Value="review.Rating" class="form-control" min="1" max="5" />
        <ValidationMessage For="@(() => review.Rating)" />
    </div>
    
    <div class="form-group">
        <label>Review:</label>
        <InputTextArea @bind-Value="review.Text" class="form-control" rows="5" />
        <ValidationMessage For="@(() => review.Text)" />
    </div>
    
    <button type="submit" class="btn btn-primary">Submit Review</button>
</EditForm>

@code {
    [Parameter]
    public string MovieId { get; set; }
    
    private Movie movie;
    private Review review = new Review();
    
    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(MovieId))
        {
            // Get the movie details to show title
            var movieService = ScopedServices.GetRequiredService<IMovieService>();
            movie = await movieService.GetMovieByIdAsync(MovieId);
            
            // Set up the review
            review.MovieId = MovieId;
            review.UserId = "CurrentUserId"; // Get from auth system
        }
    }
    
    private async Task HandleValidSubmit()
    {
        await ReviewService.CreateReviewAsync(review);
        NavigationManager.NavigateTo($"/movie/{MovieId}");
    }
}
```

### Server-Side Blazor vs. WebAssembly Considerations

#### For Blazor Server (which appears to be your choice based on the technical document):

- The repositories and services work directly as described in this guide
- Components can directly inject and use services that communicate with MongoDB
- Connection state is maintained on the server
- Consider implementing proper error handling for database operations

#### For Blazor WebAssembly (if you decide to switch):

- You'll need to create API endpoints that the client-side Blazor app can call
- Direct MongoDB access isn't possible from WebAssembly running in the browser
- You would need to implement a separate ASP.NET Core Web API project for data access

### Loading Indicators and Error Handling

Since MongoDB operations are asynchronous, implement loading indicators and error handling:

```razor
@page "/moviedetails/{Id}"
@inject IMovieService MovieService

<h1>Movie Details</h1>

@if (loading)
{
    <div class="spinner-border" role="status">
        <span class="sr-only">Loading...</span>
    </div>
}
else if (error != null)
{
    <div class="alert alert-danger">
        @error
    </div>
}
else if (movie != null)
{
    <div class="movie-details">
        <h2>@movie.Title</h2>
        <p>@movie.Description</p>
        <!-- Additional movie details -->
    </div>
}

@code {
    [Parameter]
    public string Id { get; set; }
    
    private Movie movie;
    private bool loading = true;
    private string error;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            loading = true;
            movie = await MovieService.GetMovieByIdAsync(Id);
            if (movie == null)
            {
                error = "Movie not found";
            }
        }
        catch (Exception ex)
        {
            error = $"Error loading movie: {ex.Message}";
        }
        finally
        {
            loading = false;
        }
    }
}
```

### Implementing Movie Search with MongoDB Text Search

Create a search component utilizing MongoDB's text search capabilities:

```razor
@page "/search"
@inject IMovieService MovieService

<h1>Search Movies</h1>

<div class="search-container">
    <input @bind="searchTerm" @bind:event="oninput" @onkeypress="HandleKeyPress" placeholder="Search for movies..." />
    <button @onclick="PerformSearch">Search</button>
</div>

@if (loading)
{
    <p>Searching...</p>
}
else if (movies?.Count > 0)
{
    <div class="search-results">
        @foreach (var movie in movies)
        {
            <div class="movie-card">
                <img src="@movie.PosterUrl" alt="@movie.Title" />
                <h3>@movie.Title</h3>
                <p>@movie.ReleaseDate.Year</p>
            </div>
        }
    </div>
}
else if (!string.IsNullOrEmpty(searchTerm) && !loading)
{
    <p>No movies found matching '@searchTerm'</p>
}

@code {
    private string searchTerm = "";
    private List<Movie> movies;
    private bool loading;
    
    private async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await PerformSearch();
        }
    }
    
    private async Task PerformSearch()
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return;
            
        loading = true;
        try
        {
            movies = await MovieService.SearchMoviesAsync(searchTerm);
        }
        finally
        {
            loading = false;
        }
    }
}
```

## Conclusion

This guide covers the essential steps to integrate MongoDB with your C# models in the CineScope Blazor project. By following these steps, you'll establish a robust data access layer that effectively communicates with your MongoDB database while maintaining a clean separation of concerns in your architecture.

The Blazor-specific sections highlight how to integrate your MongoDB services directly with Blazor components, handle component state management, implement real-time updates, and optimize for performance in a Blazor Server application.

Remember that MongoDB is a schema-less database, which provides flexibility but also requires discipline in your application code to maintain data integrity. Use the models, repositories, and services pattern to enforce consistent data structures throughout your application.
