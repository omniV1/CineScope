using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CineScope.Server.Models
{
    /// <summary>
    /// Represents a banned word or phrase in the content filtering system.
    /// This model maps directly to documents in the BannedWords collection in MongoDB.
    /// </summary>
    public class BannedWord
    {
        /// <summary>
        /// Unique identifier for the banned word entry.
        /// The BsonId attribute marks this as the document's primary key in MongoDB.
        /// BsonRepresentation converts between .NET string and MongoDB ObjectId.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The actual word or phrase that should be filtered.
        /// Used in content filtering to scan user-generated content.
        /// </summary>
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// Indicates the level of severity for this banned term (e.g., 1-5).
        /// Can be used to implement graduated responses based on severity.
        /// </summary>
        public int Severity { get; set; }

        /// <summary>
        /// Categorizes the type of banned content (e.g., "Profanity", "Hate Speech").
        /// Useful for reporting and management of the banned word list.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Flag indicating if this banned word is currently active in filtering.
        /// Allows disabling entries without deleting them from the database.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}