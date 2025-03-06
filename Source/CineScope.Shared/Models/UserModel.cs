using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CineScope.Shared.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("username")]  // Map to lowercase field name in MongoDB
        public string username { get; set; } = string.Empty;

        [BsonElement("email")]     // Map to lowercase field name in MongoDB
        public string Email { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("roles")]
        public List<string> Roles { get; set; } = new List<string>();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("lastLogin")]
        public DateTime? LastLogin { get; set; }

        [BsonElement("isLocked")]
        public bool IsLocked { get; set; }

        [BsonElement("failedLoginAttempts")]
        public int FailedLoginAttempts { get; set; }

        public UserModel(ObjectId id, string username, string email, string passwordHash, List<string> roles, DateTime createdAt, DateTime? lastLogin, bool isLocked, int failedLoginAttempts)
        {
            Id = id;
            this.username = username;
            Email = email;
            PasswordHash = passwordHash;
            Roles = roles;
            CreatedAt = createdAt;
            LastLogin = lastLogin;
            IsLocked = isLocked;
            FailedLoginAttempts = failedLoginAttempts;
        }

        public UserModel()
        {
            Id = ObjectId.GenerateNewId();
            username = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
            Roles = new List<string>();
            CreatedAt = DateTime.UtcNow;
            LastLogin = null;
            IsLocked = false;
            FailedLoginAttempts = 0;
        }
    }
}
