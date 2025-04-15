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
    private static readonly List<Movie> _movies = new()
    {
        new Movie { Id = "1", Title = "The Shawshank Redemption", Director = "Frank Darabont", Year = 1994, Genres = new List<string> { "Drama" }, Rating = 9.3, Description = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.", ReleaseDate = new DateTime(1994, 9, 23), Actors = new List<string> { "Tim Robbins", "Morgan Freeman" }, PosterUrl = "https://m.media-amazon.com/images/M/MV5BMDFkYTc0MGEtZmNhMC00ZDIzLWFmNTEtODM1ZmRlYWMwMWFmXkEyXkFqcGdeQXVyMTMxODk2OTU@._V1_.jpg", AverageRating = 9.3, ReviewCount = 2431 },
        new Movie { Id = "2", Title = "The Godfather", Director = "Francis Ford Coppola", Year = 1972, Genres = new List<string> { "Crime", "Drama" }, Rating = 9.2, Description = "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant son.", ReleaseDate = new DateTime(1972, 3, 24), Actors = new List<string> { "Marlon Brando", "Al Pacino", "James Caan" }, PosterUrl = "https://m.media-amazon.com/images/M/MV5BM2MyNjYxNmUtYTAwNi00MTYxLWJmNWYtYzZlODY3ZTk3OTFlXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg", AverageRating = 9.2, ReviewCount = 1789 },
        new Movie { Id = "3", Title = "The Dark Knight", Director = "Christopher Nolan", Year = 2008, Genres = new List<string> { "Action", "Crime", "Drama", "Thriller" }, Rating = 9.0, Description = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.", ReleaseDate = new DateTime(2008, 7, 18), Actors = new List<string> { "Christian Bale", "Heath Ledger", "Aaron Eckhart" }, PosterUrl = "https://m.media-amazon.com/images/M/MV5BMTMxNTMwODM0NF5BMl5BanBnXkFtZTcwODAyMTk2Mw@@._V1_.jpg", AverageRating = 9.0, ReviewCount = 2321 },
        new Movie { Id = "4", Title = "The Godfather Part II", Director = "Francis Ford Coppola", Year = 1974, Genres = new List<string> { "Crime", "Drama" }, Rating = 9.0, Description = "The early life and career of Vito Corleone in 1920s New York City is portrayed, while his son, Michael, expands and tightens his grip on the family crime syndicate.", ReleaseDate = new DateTime(1974, 12, 18), Actors = new List<string> { "Al Pacino", "Robert De Niro", "Robert Duvall" }, PosterUrl = "https://m.media-amazon.com/images/M/MV5BMWMwMGQzZTItY2JlNC00OWZiLWIyMDctNDk2ZDQ2YjRjMWQ0XkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg", AverageRating = 9.0, ReviewCount = 1132 },
        new Movie { Id = "5", Title = "12 Angry Men", Director = "Sidney Lumet", Year = 1957, Genres = new List<string> { "Crime", "Drama" }, Rating = 9.0, Description = "A jury holdout attempts to prevent a miscarriage of justice by forcing his colleagues to reconsider the evidence.", ReleaseDate = new DateTime(1957, 4, 10), Actors = new List<string> { "Henry Fonda", "Lee J. Cobb", "Martin Balsam" }, PosterUrl = "https://m.media-amazon.com/images/M/MV5BMWU4N2FjNzYtNTVkNC00NzQ0LTg0MjAtYTJlMjFhNGUxZDFmXkEyXkFqcGdeQXVyNjc1NTYyMjg@._V1_.jpg", AverageRating = 9.0, ReviewCount = 732 },
        new Movie { Id = "6", Title = "Schindler's List", Director = "Steven Spielberg", Year = 1993, Genres = new List<string> { "Biography", "Drama", "History" }, Rating = 9.0, Description = "In German-occupied Poland during World War II, industrialist Oskar Schindler gradually becomes concerned for his Jewish workforce after witnessing their persecution by the Nazis.", ReleaseDate = new DateTime(1993, 12, 15), Actors = new List<string> { "Liam Neeson", "Ralph Fiennes", "Ben Kingsley" }, PosterUrl = "https://m.media-amazon.com/images/M/MV5BNDE4OTMxMTctNmRhYy00NWE2LTg3YzItYTk3M2UwOTU5Njg4XkEyXkFqcGdeQXVyNjU0OTQ0OTY@._V1_.jpg", AverageRating = 9.0, ReviewCount = 1283 },
        new Movie { Id = "7", Title = "The Lord of the Rings: The Return of the King", Director = "Peter Jackson", Year = 2003, Genres = new List<string> { "Action", "Adventure", "Drama", "Fantasy" }, Rating = 9.0, Description = "Gandalf and Aragorn lead the World of Men against Sauron's army to draw his gaze from Frodo and Sam as they approach Mount Doom with the One Ring.", ReleaseDate = new DateTime(2003, 12, 17), Actors = new List<string> { "Elijah Wood", "Viggo Mortensen", "Ian McKellen" }, PosterUrl = "https://m.media-amazon.com/images/M/MV5BNzA5ZDNlZWMtM2NhNS00NDJjLTk4NDItYTRmY2EwMWZlMTY3XkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg", AverageRating = 9.0, ReviewCount = 1764 },
        new Movie { Id = "8", Title = "Pulp Fiction", Director = "Quentin Tarantino", Year = 1994, Genres = new List<string> { "Crime", "Drama" }, Rating = 8.9, Description = "The lives of two mob hitmen, a boxer, a gangster and his wife, and a pair of diner bandits intertwine in four tales of violence and redemption.", ReleaseDate = new DateTime(1994, 10, 14), Actors = new List<string> { "John Travolta", "Uma Thurman", "Samuel L. Jackson" }, PosterUrl = "https://m.media-amazon.com/images/M/MV5BNGNhMDIzZTUtNTBlZi00MTRlLWFjM2ItYzViMjE3YzI5MjljXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg", AverageRating = 8.9, ReviewCount = 1892 },
        new Movie { Id = "9", Title = "The Lord of the Rings: The Fellowship of the Ring", Director = "Peter Jackson", Year = 2001, Genres = new List<string> { "Action", "Adventure", "Drama", "Fantasy" }, Rating = 8.8, Description = "A meek Hobbit from the Shire and eight companions set out on a journey to destroy the powerful One Ring and save Middle-earth from the Dark Lord Sauron.", ReleaseDate = new DateTime(2001, 12, 19), Actors = new List<string> { "Elijah Wood", "Ian McKellen", "Orlando Bloom" }, PosterUrl = "https://m.media-amazon.com/images/M/MV5BN2EyZjM3NzUtNWUzMi00MTgxLWI0NTctMzY4M2VlOTdjZWRiXkEyXkFqcGdeQXVyNDUzOTQ5MjY@._V1_.jpg", AverageRating = 8.8, ReviewCount = 1584 },
        new Movie { Id = "10", Title = "Forrest Gump", Director = "Robert Zemeckis", Year = 1994, Genres = new List<string> { "Drama", "Romance" }, Rating = 8.8, Description = "The presidencies of Kennedy and Johnson, the events of Vietnam, Watergate and other historical events unfold through the perspective of an Alabama man with an IQ of 75, whose only desire is to be reunited with his childhood sweetheart.", ReleaseDate = new DateTime(1994, 7, 6), Actors = new List<string> { "Tom Hanks", "Robin Wright", "Gary Sinise" }, PosterUrl = "https://m.media-amazon.com/images/M/MV5BNWIwODRlZTUtY2U3ZS00Yzg1LWJhNzYtMmZiYmEyNmU1NjMzXkEyXkFqcGdeQXVyMTQxNzMzNDI@._V1_.jpg", AverageRating = 8.8, ReviewCount = 1921 }
    };

    /// <summary>
    /// Search for movies by title
    /// </summary>
    [McpServerTool]
    [Description("Search for movies by title")]
    public static List<Movie> SearchMoviesByTitle(string title)
    {
        return _movies.FindAll(m => m.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get a movie by its ID
    /// </summary>
    [McpServerTool]
    [Description("Get detailed information about a movie by its ID")]
    public static Movie? GetMovieById(string id)
    {
        return _movies.Find(m => m.Id == id);
    }

    /// <summary>
    /// Get movie recommendations based on a genre
    /// </summary>
    [McpServerTool]
    [Description("Get movie recommendations based on a genre")]
    public static List<Movie> GetRecommendationsByGenre(string genre, int count = 3)
    {
        var recommendations = _movies.FindAll(m => m.Genres.Contains(genre, StringComparer.OrdinalIgnoreCase));
        return recommendations.Count <= count ? recommendations : recommendations.GetRange(0, count);
    }

    /// <summary>
    /// Get top rated movies
    /// </summary>
    [McpServerTool]
    [Description("Get top rated movies")]
    public static List<Movie> GetTopRatedMovies(int count = 5)
    {
        var sortedMovies = new List<Movie>(_movies);
        sortedMovies.Sort((a, b) => b.Rating.CompareTo(a.Rating));
        return sortedMovies.Count <= count ? sortedMovies : sortedMovies.GetRange(0, count);
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
