using System.Collections.Generic;
using System.Threading.Tasks;
using CineScope.Shared.DTOs;

namespace CineScope.Server.Interfaces
{
    public interface IMovieService
    {
        Task<List<MovieDto>> GetAllMoviesAsync();
        Task<MovieDto> GetMovieByIdAsync(string id);
        Task<List<MovieDto>> GetMoviesByGenreAsync(string genre);
    }
}