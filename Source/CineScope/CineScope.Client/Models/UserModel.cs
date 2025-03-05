using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CineScope.Client.Models
{
    public class UserModel
    {

        [BsonId]
        public ObjectId Id { get; set; } // Unique identifier for the user

        public string Username { get; set; } // Username of the user
        public string Email { get; set; } // Email address of the user
        public string PasswordHash { get; set; } // Hashed password for security
        public List<string> Roles { get; set; } // Roles assigned to the user
        public DateTime CreatedAt { get; set; } // Date and time when the user was created
        public DateTime LastLogin { get; set; } // Date and time of the last login
        public bool IsLocked { get; set; } // Indicates if the account is locked
        public int FailedLoginAttempts { get; set; } // Number of failed login attempts

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="passwordHash"></param>
        /// <param name="roles"></param>
        /// <param name="createdAt"></param>
        /// <param name="lastLogin"></param>
        /// <param name="isLocked"></param>
        /// <param name="failedLoginAttempts"></param>
        public UserModel(ObjectId id, string username, string email, string passwordHash, List<string> roles, DateTime createdAt, DateTime lastLogin, bool isLocked, int failedLoginAttempts)
        {
            Id = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Roles = roles;
            CreatedAt = createdAt;
            LastLogin = lastLogin;
            IsLocked = isLocked;
            FailedLoginAttempts = failedLoginAttempts;
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        public UserModel()
        {
            Id = ObjectId.GenerateNewId();
            Username = "";
            Email = "";
            PasswordHash = "";
            Roles = new List<string>();
            CreatedAt = DateTime.UtcNow;
            LastLogin = DateTime.MinValue;
            IsLocked = false;
            FailedLoginAttempts = 0;
        }
    }
}
