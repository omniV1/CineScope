using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CineScope.Server.Models
{
    /// <summary>
    /// Represents a user review for a movie in the CineScope application.
    /// This model maps directly to documents in the Reviews collection in MongoDB.
    /// </summary>
    public class Review
    {
        /// <summary>
        /// Unique identifier for the review.
        /// The BsonId attribute marks this as the document's primary key in MongoDB.
        /// BsonRepresentation converts between .NET string and MongoDB ObjectId.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Reference to the user who created this review.
        /// Foreign key relationship to the Users collection.
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Reference to the movie being reviewed.
        /// Foreign key relationship to the Movies collection.
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string MovieId { get; set; } = string.Empty;

        /// <summary>
        /// Numerical rating given by the user, typically on a scale (e.g., 1-5 stars).
        /// Used to calculate the movie's average rating.
        /// </summary>
        public double Rating { get; set; }   

        /// <summary>
        /// The user's written review text.
        /// Subject to content filtering before being published.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp indicating when the review was created.
        /// Automatically set to UTC time when a new review is submitted.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The username of the review author.
        /// Not stored in MongoDB but populated when needed for display.
        /// BsonIgnore prevents this property from being saved to the database.
        /// </summary>
        [BsonIgnore]
        public string Username { get; set; } = string.Empty;
    }
}