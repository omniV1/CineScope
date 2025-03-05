using CineScope.Interfaces;
using CineScope.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CineScope.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private const int MAX_FAILED_ATTEMPTS = 3;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<UserModel> GetUserByIdAsync(string id)
        {
            return await _userRepository.GetByIdAsync(new ObjectId(id));
        }

        public async Task<UserModel> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<UserModel> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<UserModel> CreateUserAsync(UserModel user)
        {
            // Hash the password before storing
            user.PasswordHash = HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;
            user.LastLogin = DateTime.MinValue;
            user.IsLocked = false;
            user.FailedLoginAttempts = 0;

            return await _userRepository.CreateAsync(user);
        }

        public async Task UpdateUserAsync(string id, UserModel user)
        {
            await _userRepository.UpdateAsync(new ObjectId(id), user);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _userRepository.DeleteAsync(new ObjectId(id));
        }

        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || user.IsLocked)
                return false;

            if (VerifyPassword(password, user.PasswordHash))
            {
                // Update last login and reset failed attempts
                user.LastLogin = DateTime.UtcNow;
                user.FailedLoginAttempts = 0;
                await _userRepository.UpdateAsync(user.Id, user);
                return true;
            }

            return false;
        }

        public async Task<bool> IsAccountLockedAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user != null && user.IsLocked;
        }

        public async Task RecordFailedLoginAttemptAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user != null)
            {
                user.FailedLoginAttempts++;

                // Lock account after max failed attempts
                if (user.FailedLoginAttempts >= MAX_FAILED_ATTEMPTS)
                {
                    user.IsLocked = true;
                }

                await _userRepository.UpdateAsync(user.Id, user);
            }
        }

        public async Task ResetFailedLoginAttemptsAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user != null)
            {
                user.FailedLoginAttempts = 0;
                user.IsLocked = false;
                await _userRepository.UpdateAsync(user.Id, user);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string providedPassword, string storedHash)
        {
            var hashedProvidedPassword = HashPassword(providedPassword);
            return hashedProvidedPassword == storedHash;
        }
    }
}