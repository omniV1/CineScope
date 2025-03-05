using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Reflection.Metadata.Ecma335;

namespace CineScope.Client.Models
{
    public class ReviewModel
    {

        [BsonId]
        public ObjectId Id { get; set; } // Unique identifier for the movie
        public ObjectId UserId { get; set; } // ID of the user that created the review
        public ObjectId MovieId { get; set; } // ID of the movie that the review corresponds to
        public double Rating { get; set; } // Rating that the user gave the movie
        public string Text { get; set; } // Content of the review
        public DateTime CreatedAt { get; set; } // Time that the review was posted
        public DateTime UpdatedAt { get; set; } // Time that the review was updated
        public bool IsApproved { get; set; } // Time that the review was approved for publishing
        public List<string> FlaggedWords { get; set; } // List of flagged words

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="movieId"></param>
        /// <param name="rating"></param>
        /// <param name="text"></param>
        /// <param name="createdAt"></param>
        /// <param name="updatedAt"></param>
        /// <param name="isApproved"></param>
        /// <param name="flaggedWords"></param>
        public ReviewModel(ObjectId id, ObjectId userId, ObjectId movieId, double rating, string text, DateTime createdAt, DateTime updatedAt, bool isApproved, List<string> flaggedWords)
        {
            Id = id;
            UserId = userId;
            MovieId = movieId;
            Rating = rating;
            Text = text;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            IsApproved = isApproved;
            FlaggedWords = flaggedWords;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ReviewModel()
        {
            Id = ObjectId.GenerateNewId();
            UserId = ObjectId.Empty;
            MovieId = ObjectId.Empty;
            Rating = 0.0;
            Text = "";
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsApproved = false;
            FlaggedWords = new List<string>();
        }
    }
}
