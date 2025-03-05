using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CineScope.Services
{
    public class ContentFilterService : IContentFilterService
    {
        private readonly IBannedWordRepository _bannedWordRepository;
        private readonly object _cacheLock = new();
        private Dictionary<string, BannedWordModel> _bannedWordsCache = new();
        private DateTime _lastCacheRefresh;

        public ContentFilterService(IBannedWordRepository bannedWordRepository)
        {
            _bannedWordRepository = bannedWordRepository;
            _lastCacheRefresh = DateTime.MinValue;
        }

        public async Task<bool> IsContentApprovedAsync(string text)
        {
            var flaggedWords = await GetFlaggedWordsAsync(text);
            return flaggedWords.Count == 0;
        }

        public async Task<List<string>> GetFlaggedWordsAsync(string text)
        {
            await RefreshBannedWordsCacheIfNeeded();

            var words = text.Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var flaggedWords = new List<string>();

            foreach (var word in words)
            {
                string cleanedWord = CleanWord(word);
                if (!string.IsNullOrEmpty(cleanedWord) && _bannedWordsCache.ContainsKey(cleanedWord.ToLower()))
                {
                    flaggedWords.Add(cleanedWord);
                }
            }

            return flaggedWords;
        }

        public async Task<BannedWordModel> AddBannedWordAsync(BannedWordModel bannedWord)
        {
            bannedWord.Word = bannedWord.Word.ToLower();
            bannedWord.AddedAt = DateTime.UtcNow;
            bannedWord.UpdatedAt = DateTime.UtcNow;

            var result = await _bannedWordRepository.CreateAsync(bannedWord);

            // Update cache
            lock (_cacheLock)
            {
                if (_bannedWordsCache.ContainsKey(bannedWord.Word))
                {
                    _bannedWordsCache[bannedWord.Word] = bannedWord;
                }
                else
                {
                    _bannedWordsCache.Add(bannedWord.Word, bannedWord);
                }
            }

            return result;
        }

        public async Task<List<BannedWordModel>> GetAllBannedWordsAsync()
        {
            return await _bannedWordRepository.GetAllAsync();
        }

        public async Task<BannedWordModel> GetBannedWordAsync(string id)
        {
            return await _bannedWordRepository.GetByIdAsync(new ObjectId(id));
        }

        public async Task UpdateBannedWordAsync(string id, BannedWordModel bannedWord)
        {
            bannedWord.Word = bannedWord.Word.ToLower();
            bannedWord.UpdatedAt = DateTime.UtcNow;

            await _bannedWordRepository.UpdateAsync(new ObjectId(id), bannedWord);

            // Update cache
            lock (_cacheLock)
            {
                if (_bannedWordsCache.ContainsKey(bannedWord.Word))
                {
                    _bannedWordsCache[bannedWord.Word] = bannedWord;
                }
            }
        }

        public async Task DeleteBannedWordAsync(string id)
        {
            var bannedWord = await _bannedWordRepository.GetByIdAsync(new ObjectId(id));
            if (bannedWord != null)
            {
                await _bannedWordRepository.DeleteAsync(new ObjectId(id));

                // Update cache
                lock (_cacheLock)
                {
                    if (_bannedWordsCache.ContainsKey(bannedWord.Word))
                    {
                        _bannedWordsCache.Remove(bannedWord.Word);
                    }
                }
            }
        }

        private async Task RefreshBannedWordsCacheIfNeeded()
        {
            // Refresh cache if it's older than 5 minutes
            if (DateTime.UtcNow.Subtract(_lastCacheRefresh).TotalMinutes > 5)
            {
                var bannedWords = await _bannedWordRepository.GetAllAsync();
                var newCache = new Dictionary<string, BannedWordModel>();

                foreach (var word in bannedWords.Where(w => w.IsActive))
                {
                    newCache[word.Word.ToLower()] = word;
                }

                lock (_cacheLock)
                {
                    _bannedWordsCache = newCache;
                    _lastCacheRefresh = DateTime.UtcNow;
                }
            }
        }

        private static string CleanWord(string word)
        {
            // Remove non-alphanumeric characters
            return Regex.Replace(word, @"[^\w]", "");
        }
    }
}