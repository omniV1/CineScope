using CineScope.Shared.DTOs;
using System.Collections.Generic;

namespace CineScope.Server.Interfaces
{
    public interface IMovieCacheService
    {
        List<MovieDto> GetAllMovies();
        void SetAllMovies(List<MovieDto> movies);
        void ClearCache();
    }
} 