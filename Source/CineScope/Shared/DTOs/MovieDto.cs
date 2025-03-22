using System;
using System.Collections.Generic;

namespace CineScope.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for movie information.
    /// Used to transfer movie data between the server and client.
    /// </summary>
    public class MovieDto
    {
        /// <summary>
        /// Unique identifier for the movie.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The title of the movie.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed synopsis or summary of the movie's plot.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The official release date of the movie.
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// List of genres that categorize this movie.
        /// </summary>
        public List<string> Genres { get; set; } = new List<string>();

        /// <summary>
        /// The primary director of the movie.
        /// </summary>
        public string Director { get; set; } = string.Empty;

        /// <summary>
        /// List of primary actors appearing in the movie.
        /// </summary>
        public List<string> Actors { get; set; } = new List<string>();

        /// <summary>
        /// URL reference to the movie's poster image.
        /// </summary>
        public string PosterUrl { get; set; } = string.Empty;

        /// <summary>
        /// Calculated average rating based on user reviews.
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Total number of reviews for this movie.
        /// </summary>
        public int ReviewCount { get; set; }
    }
}