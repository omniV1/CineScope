using CineScope.Shared.Helpers;
using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IMongoCollection<MovieModel> _movies;

        public MovieRepository(MongoDBSettings settings)
        {
            var client = MongoDbConnectionHelper.CreateClient(settings);
            var database = client.GetDatabase(settings.DatabaseName);
            _movies = database.GetCollection<MovieModel>(settings.MoviesCollectionName);
        }

        public async Task<List<MovieModel>> GetAllAsync()
        {
            return await _movies.Find(movie => true).ToListAsync();
        }

        public async Task<MovieModel> GetByIdAsync(ObjectId id)
        {
            return await _movies.Find(movie => movie.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<MovieModel>> GetByGenreAsync(string genre)
        {
            var filter = Builders<MovieModel>.Filter.AnyEq(m => m.Genres, genre);
            return await _movies.Find(filter).ToListAsync();
        }

        public async Task<List<MovieModel>> GetTopRatedAsync(int limit = 10)
        {
            return await _movies.Find(movie => true)
                .Sort(Builders<MovieModel>.Sort.Descending(m => m.AverageRating))
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<List<MovieModel>> GetRecentAsync(int limit = 10)
        {
            return await _movies.Find(movie => true)
                .Sort(Builders<MovieModel>.Sort.Descending(m => m.ReleaseDate))
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<MovieModel> CreateAsync(MovieModel movie)
        {
            await _movies.InsertOneAsync(movie);
            return movie;
        }

        public async Task UpdateAsync(ObjectId id, MovieModel updatedMovie)
        {
            await _movies.ReplaceOneAsync(movie => movie.Id == id, updatedMovie);
        }

        public async Task DeleteAsync(ObjectId id)
        {
            await _movies.DeleteOneAsync(movie => movie.Id == id);
        }
    }
}