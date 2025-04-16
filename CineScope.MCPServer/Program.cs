using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

// Create a host builder
var builder = Host.CreateApplicationBuilder(args);

// Configure logging to use console
builder.Logging.AddConsole(options =>
{
    // Configure all logs to go to stderr
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Add MCP server with tools
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Build and run the host
await builder.Build().RunAsync();

/// <summary>
/// Movie information tool type
/// </summary>
[McpServerToolType]
public static class MovieTools
{
    private static IMongoCollection<Movie> _movieCollection;

    static MovieTools()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        var database = client.GetDatabase("CineScopeDb");
        _movieCollection = database.GetCollection<Movie>("Movies");
    }

    /// <summary>
    /// Search for movies by title
    /// </summary>
    [McpServerTool]
    [Description("Search for movies by title")]
    public static List<Movie> SearchMoviesByTitle(string title)
    {
        var filter = Builders<Movie>.Filter.Regex("Title", new BsonRegularExpression(title, "i"));
        return _movieCollection.Find(filter).ToList();
    }

    /// <summary>
    /// Get a movie by its ID
    /// </summary>
    [McpServerTool]
    [Description("Get detailed information about a movie by its ID")]
    public static Movie? GetMovieById(string id)
    {
        var filter = Builders<Movie>.Filter.Eq("Id", id);
        return _movieCollection.Find(filter).FirstOrDefault();
    }

    /// <summary>
    /// Get movie recommendations based on a genre
    /// </summary>
    [McpServerTool]
    [Description("Get movie recommendations based on a genre")]
    public static List<Movie> GetRecommendationsByGenre(string genre, int count = 3)
    {
        var filter = Builders<Movie>.Filter.AnyEq("Genres", genre);
        return _movieCollection.Find(filter).Limit(count).ToList();
    }

    /// <summary>
    /// Get top rated movies
    /// </summary>
    [McpServerTool]
    [Description("Get top rated movies")]
    public static List<Movie> GetTopRatedMovies(int count = 5)
    {
        var sort = Builders<Movie>.Sort.Descending("Rating");
        return _movieCollection.Find(FilterDefinition<Movie>.Empty).Sort(sort).Limit(count).ToList();
    }
}

/// <summary>
/// Represents a movie
/// </summary>
public class Movie
{
    /// <summary>
    /// Movie ID
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Movie title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Movie director
    /// </summary>
    public string Director { get; set; } = string.Empty;
    
    /// <summary>
    /// Release year
    /// </summary>
    public int Year { get; set; }
    
    /// <summary>
    /// Movie genres
    /// </summary>
    public List<string> Genres { get; set; } = new List<string>();
    
    /// <summary>
    /// Movie rating (0-10)
    /// </summary>
    public double Rating { get; set; }
    
    /// <summary>
    /// Movie description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Release date
    /// </summary>
    public DateTime ReleaseDate { get; set; }
    
    /// <summary>
    /// List of actors
    /// </summary>
    public List<string> Actors { get; set; } = new List<string>();
    
    /// <summary>
    /// URL to movie poster
    /// </summary>
    public string PosterUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Average user rating
    /// </summary>
    public double AverageRating { get; set; }
    
    /// <summary>
    /// Number of reviews
    /// </summary>
    public int ReviewCount { get; set; }
}
