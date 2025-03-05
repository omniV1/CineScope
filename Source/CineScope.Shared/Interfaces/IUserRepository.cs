using CineScope.Shared.Models;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Shared.Interfaces
{
    public interface IUserRepository
    {
        Task<List<UserModel>> GetAllAsync();
        Task<UserModel> GetByIdAsync(ObjectId id);
        Task<UserModel> GetByUsernameAsync(string username);
        Task<UserModel> GetByEmailAsync(string email);
        Task<UserModel> CreateAsync(UserModel user);
        Task UpdateAsync(ObjectId id, UserModel user);
        Task DeleteAsync(ObjectId id);
    }
}