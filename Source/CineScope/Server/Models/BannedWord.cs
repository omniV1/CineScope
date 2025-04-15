using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CineScope.Server.Models
{
    /// <summary>
    /// Represents a banned word or phrase in the content filtering system.
    /// This model maps directly to documents in the BannedWords collection in MongoDB.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class BannedWord
    {
        /// <summary>
        /// Unique identifier for the banned word entry.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The actual word or phrase that should be filtered.
        /// Used in content filtering to scan user-generated content.
        /// </summary>
        [BsonElement("word")]
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// Indicates the level of severity for this banned term (e.g., 1-5).
        /// Can be used to implement graduated responses based on severity.
        /// </summary>
        [BsonElement("severity")]
        public int Severity { get; set; }

        /// <summary>
        /// Categorizes the type of banned content (e.g., "Profanity", "Hate Speech").
        /// Useful for reporting and management of the banned word list.
        /// </summary>
        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Flag indicating if this banned word is currently active in filtering.
        /// Allows disabling entries without deleting them from the database.
        /// </summary>
        [BsonElement("isactive")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the banned word was added.
        /// </summary>
        [BsonElement("addedat")]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the banned word was last updated.
        /// </summary>
        [BsonElement("updatedat")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}