using CineScope.Client.Models;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Interfaces
{
    public interface IMovieRepository
    {
        Task<List<MovieModel>> GetAllAsync();
        Task<MovieModel> GetByIdAsync(ObjectId id);
        Task<List<MovieModel>> GetByGenreAsync(string genre);
        Task<List<MovieModel>> GetTopRatedAsync(int limit = 10);
        Task<List<MovieModel>> GetRecentAsync(int limit = 10);
        Task<MovieModel> CreateAsync(MovieModel movie);
        Task UpdateAsync(ObjectId id, MovieModel movie);
        Task DeleteAsync(ObjectId id);
    }
}