using CineScope.Shared.Helpers;
using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Repositories
{
    /// <summary>
    /// Repository class for managing user data in the application
    /// Implements IUserRepository interface to handle CRUD operations for users
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserModel> _users;

        /// <summary>
        /// Constructor for UserRepository
        /// </summary>
        /// <param name="settings">MongoDB connection and database settings</param>
        public UserRepository(MongoDBSettings settings)
        {
            // Initialize MongoDB client using the connection helper
            var client = MongoDbConnectionHelper.CreateClient(settings);

            // Get reference to the database
            var database = client.GetDatabase(settings.DatabaseName);

            // Get reference to the users collection
            _users = database.GetCollection<UserModel>(settings.UsersCollectionName);
        }

        /// <summary>
        /// Retrieves all users from the database
        /// </summary>
        /// <returns>A list of all users</returns>
        public async Task<List<UserModel>> GetAllAsync()
        {
            // Find all documents in the users collection
            return await _users.Find(user => true).ToListAsync();
        }

        /// <summary>
        /// Finds a user by their unique identifier
        /// </summary>
        /// <param name="id">The ObjectId of the user to retrieve</param>
        /// <returns>The user if found, null otherwise</returns>
        public async Task<UserModel> GetByIdAsync(ObjectId id)
        {
            // Find the first document matching the given id
            return await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Finds a user by their username
        /// </summary>
        /// <param name="username">The username to search for</param>
        /// <returns>The user if found, null otherwise</returns>
        public async Task<UserModel> GetByUsernameAsync(string username)
        {
            // Find the user with the specified username
            // Note: Case sensitive comparison based on how MongoDB stores the data
            return await _users.Find(user => user.username == username).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Finds a user by their email address
        /// </summary>
        /// <param name="email">The email address to search for</param>
        /// <returns>The user if found, null otherwise</returns>
        public async Task<UserModel> GetByEmailAsync(string email)
        {
            // Find the user with the specified email address
            // Note: Case sensitive comparison based on how MongoDB stores the data
            return await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Creates a new user in the database
        /// </summary>
        /// <param name="user">The user to create</param>
        /// <returns>The created user with generated ID</returns>
        public async Task<UserModel> CreateAsync(UserModel user)
        {
            // Insert the new user into the collection
            await _users.InsertOneAsync(user);

            // Return the inserted user (now with an ID)
            return user;
        }

        /// <summary>
        /// Updates an existing user in the database
        /// </summary>
        /// <param name="id">The ID of the user to update</param>
        /// <param name="updatedUser">The updated user data</param>
        public async Task UpdateAsync(ObjectId id, UserModel updatedUser)
        {
            // Replace the existing user document with the updated one
            await _users.ReplaceOneAsync(user => user.Id == id, updatedUser);
        }

        /// <summary>
        /// Deletes a user from the database
        /// </summary>
        /// <param name="id">The ID of the user to delete</param>
        public async Task DeleteAsync(ObjectId id)
        {
            // Delete the user document with the specified id
            await _users.DeleteOneAsync(user => user.Id == id);
        }
    }
}
