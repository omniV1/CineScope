using CineScope.Shared.Helpers;
using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserModel> _users;

        public UserRepository(MongoDBSettings settings)
        {
            var client = MongoDbConnectionHelper.CreateClient(settings);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<UserModel>(settings.UsersCollectionName);
        }

        public async Task<List<UserModel>> GetAllAsync()
        {
            return await _users.Find(user => true).ToListAsync();
        }

        public async Task<UserModel> GetByIdAsync(ObjectId id)
        {
            return await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
        }

        public async Task<UserModel> GetByUsernameAsync(string username)
        {
            return await _users.Find(user => user.username == username).FirstOrDefaultAsync();
        }

        public async Task<UserModel> GetByEmailAsync(string email)
        {
            return await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
        }

        public async Task<UserModel> CreateAsync(UserModel user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task UpdateAsync(ObjectId id, UserModel updatedUser)
        {
            await _users.ReplaceOneAsync(user => user.Id == id, updatedUser);
        }

        public async Task DeleteAsync(ObjectId id)
        {
            await _users.DeleteOneAsync(user => user.Id == id);
        }
    }
}