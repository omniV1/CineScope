using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace CineScope.Server.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }

        public bool IsLocked { get; set; } = false;

        public int FailedLoginAttempts { get; set; } = 0;
    }
}