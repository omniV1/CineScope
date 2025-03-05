using CineScope.Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Interfaces
{
    public interface IMovieService
    {
        Task<List<MovieModel>> GetAllMoviesAsync();
        Task<MovieModel> GetMovieByIdAsync(string id);
        Task<List<MovieModel>> GetMoviesByGenreAsync(string genre);
        Task<List<MovieModel>> GetTopRatedMoviesAsync(int limit = 10);
        Task<List<MovieModel>> GetRecentMoviesAsync(int limit = 10);
        Task<MovieModel> CreateMovieAsync(MovieModel movie);
        Task UpdateMovieAsync(string id, MovieModel movie);
        Task DeleteMovieAsync(string id);
    }
}