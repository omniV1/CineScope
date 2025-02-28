using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.IO;

namespace CineScope.Models
{
    public class MovieModel
    {

        [BsonId]
        public ObjectId Id { get; set; } // Unique identifier for the movie

        public string Title { get; set; } // Title of the movie
        public string Description { get; set; } // Description of the movie
        public DateTime ReleaseDate { get; set; } // Release date of the movie
        public List<string> Genres { get; set; } // Genres associated with the movie
        public string Director { get; set; } // Director of the movie
        public List<string> Actors { get; set; } // List of actors in the movie
        public string PosterUrl { get; set; } // URL of the movie poster
        public double AverageRating { get; set; } // Average rating of the movie
        public int ReviewCount { get; set; } // Number of reviews for the movie

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="releaseDate"></param>
        /// <param name="genres"></param>
        /// <param name="director"></param>
        /// <param name="actors"></param>
        /// <param name="posterUrl"></param>
        /// <param name="averageRating"></param>
        /// <param name="reviewCount"></param>
        public MovieModel(ObjectId id, string title, string description, DateTime releaseDate, List<string> genres, string director, List<string> actors, string posterUrl, double averageRating, int reviewCount)
        {
            Id = id;
            Title = title;
            Description = description;
            ReleaseDate = releaseDate;
            Genres = genres;
            Director = director;
            Actors = actors;
            PosterUrl = posterUrl;
            AverageRating = averageRating;
            ReviewCount = reviewCount;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MovieModel()
        {
            Id = ObjectId.GenerateNewId();
            Title = "";
            Description = "";
            ReleaseDate = DateTime.MinValue;
            Genres = new List<string>();
            Director = "";
            Actors = new List<string>();
            PosterUrl = "";
            AverageRating = 0.0;
            ReviewCount = 0;
        }
    }
}
