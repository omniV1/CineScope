using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace CineScope.Server.Models
{
    public class Movie
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime ReleaseDate { get; set; }

        public List<string> Genres { get; set; } = new List<string>();

        public string Director { get; set; }

        public List<string> Actors { get; set; } = new List<string>();

        public string PosterUrl { get; set; }

        public double AverageRating { get; set; } = 0;

        public int ReviewCount { get; set; } = 0;
    }
}