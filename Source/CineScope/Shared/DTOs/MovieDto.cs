using System;
using System.Collections.Generic;

namespace CineScope.Shared.DTOs
{
    public class MovieDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public string Director { get; set; }
        public List<string> Actors { get; set; } = new List<string>();
        public string PosterUrl { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}