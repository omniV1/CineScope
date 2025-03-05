using CineScope.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Shared.Interfaces
{
    public interface IUserService
    {
        Task<List<UserModel>> GetAllUsersAsync();
        Task<UserModel> GetUserByIdAsync(string id);
        Task<UserModel> GetUserByUsernameAsync(string username);
        Task<UserModel> GetUserByEmailAsync(string email);
        Task<UserModel> CreateUserAsync(UserModel user);
        Task UpdateUserAsync(string id, UserModel user);
        Task DeleteUserAsync(string id);
        Task<bool> AuthenticateUserAsync(string username, string password);
        Task<bool> IsAccountLockedAsync(string username);
        Task RecordFailedLoginAttemptAsync(string username);
        Task ResetFailedLoginAttemptsAsync(string username);
    }
}