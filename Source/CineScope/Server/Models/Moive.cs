using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace CineScope.Server.Models
{
    /// <summary>
    /// Represents a movie entity in the CineScope application.
    /// This model maps directly to documents in the Movies collection in MongoDB.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Movie
    {
        /// <summary>
        /// Unique identifier for the movie.
        /// The BsonId attribute marks this as the document's primary key in MongoDB.
        /// BsonRepresentation converts between .NET string and MongoDB ObjectId.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The title of the movie as it appears to users.
        /// Used for display and search functionality.
        /// </summary>

        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed synopsis or summary of the movie's plot.
        /// Provides users with information about the movie's content.
        /// </summary>

        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The official release date of the movie.
        /// Used for sorting, filtering, and display purposes.
        /// </summary>
      
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// List of genres that categorize this movie (e.g., "Action", "Comedy").
        /// Used for filtering and recommendation features.
        /// </summary>
     
        public List<string> Genres { get; set; } = new List<string>();

        /// <summary>
        /// The primary director of the movie.
        /// Used for display and search functionality.
        /// </summary>
      
        public string Director { get; set; } = string.Empty;

        /// <summary>
        /// List of primary actors appearing in the movie.
        /// Used for display and search functionality.
        /// </summary>
     
        public List<string> Actors { get; set; } = new List<string>();

        /// <summary>
        /// URL reference to the movie's poster image.
        /// Displayed in movie lists and detail views.
        /// </summary>
    
        public string PosterUrl { get; set; } = string.Empty;

        /// <summary>
        /// Calculated average rating based on user reviews.
        /// Defaults to 0 for movies with no reviews yet.
        /// </summary>
 
        public double AverageRating { get; set; } = 0;

        /// <summary>
        /// Counter tracking the total number of reviews for this movie.
        /// Used for sorting, filtering, and display purposes.
        /// </summary>
     
        public int ReviewCount { get; set; } = 0;
    }
}