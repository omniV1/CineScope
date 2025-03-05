using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CineScope.Models
{
    public class BannedWordModel
    {

        [BsonId]
        public ObjectId Id { get; set; } // Unique identifier for the movie
        public string Word { get; set; } // Word that is banned
        public int Severity { get; set; } // Severity of the banned word
        public string Category { get; set; } // Category of the banned word
        public bool IsActive { get; set; } // If the banned word is active or not
        public DateTime AddedAt { get; set; } // Time that the word was added
        public DateTime UpdatedAt { get; set; } // Time that the word was updated

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="word"></param>
        /// <param name="severity"></param>
        /// <param name="category"></param>
        /// <param name="isActive"></param>
        /// <param name="addedAt"></param>
        /// <param name="updatedAt"></param>
        public BannedWordModel(ObjectId id, string word, int severity, string category, bool isActive, DateTime addedAt, DateTime updatedAt)
        {
            Id = id;
            Word = word;
            Severity = severity;
            Category = category;
            IsActive = isActive;
            AddedAt = addedAt;
            UpdatedAt = updatedAt;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BannedWordModel()
        {
            Id = ObjectId.GenerateNewId();
            Word = "";
            Severity = 0;
            Category = "";
            IsActive = true;
            AddedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
