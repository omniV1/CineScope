using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CineScope.Server.Models
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string MovieId { get; set; }

        public double Rating { get; set; }

        public string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Not stored in MongoDB, populated when needed
        [BsonIgnore]
        public string Username { get; set; }
    }
}