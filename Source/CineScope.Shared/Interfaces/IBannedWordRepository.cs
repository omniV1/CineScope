using CineScope.Shared.Models;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Shared.Interfaces
{
    public interface IBannedWordRepository
    {
        Task<List<BannedWordModel>> GetAllAsync();
        Task<BannedWordModel> GetByIdAsync(ObjectId id);
        Task<BannedWordModel> GetByWordAsync(string word);
        Task<List<BannedWordModel>> GetByCategoryAsync(string category);
        Task<BannedWordModel> CreateAsync(BannedWordModel bannedWord);
        Task UpdateAsync(ObjectId id, BannedWordModel bannedWord);
        Task DeleteAsync(ObjectId id);
    }
}