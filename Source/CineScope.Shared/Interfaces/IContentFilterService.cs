using CineScope.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Shared.Interfaces
{
    public interface IContentFilterService
    {
        Task<bool> IsContentApprovedAsync(string text);
        Task<List<string>> GetFlaggedWordsAsync(string text);
        Task<BannedWordModel> AddBannedWordAsync(BannedWordModel bannedWord);
        Task<List<BannedWordModel>> GetAllBannedWordsAsync();
        Task<BannedWordModel> GetBannedWordAsync(string id);
        Task UpdateBannedWordAsync(string id, BannedWordModel bannedWord);
        Task DeleteBannedWordAsync(string id);
    }
}