using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CineScope.Server.Models
{
    public class BannedWord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Word { get; set; }

        public int Severity { get; set; }

        public string Category { get; set; }

        public bool IsActive { get; set; } = true;
    }
}