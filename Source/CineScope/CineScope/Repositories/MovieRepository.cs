using CineScope.Shared.Helpers;
using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CineScope.Repositories
{
    /// <summary>
    /// Repository class for managing movie data in the application
    /// Implements IMovieRepository interface to handle CRUD operations for movies
    /// </summary>
    public class MovieRepository : IMovieRepository
    {
        private readonly IMongoCollection<MovieModel> _movies;

        /// <summary>
        /// Constructor for MovieRepository
        /// </summary>
        /// <param name="settings">MongoDB connection and database settings</param>
        public MovieRepository(MongoDBSettings settings)
        {
            // Initialize the MongoDB client using the connection helper
            var client = MongoDbConnectionHelper.CreateClient(settings);

            // Get reference to the database
            var database = client.GetDatabase(settings.DatabaseName);

            // Get reference to the movies collection
            _movies = database.GetCollection<MovieModel>(settings.MoviesCollectionName);
        }

        /// <summary>
        /// Retrieves all movies from the database
        /// </summary>
        /// <returns>A list of all movies</returns>
        public async Task<List<MovieModel>> GetAllAsync()
        {
            // Find all documents in the collection
            return await _movies.Find(movie => true).ToListAsync();
        }

        /// <summary>
        /// Finds a movie by its unique identifier
        /// </summary>
        /// <param name="id">The ObjectId of the movie to retrieve</param>
        /// <returns>The movie if found, null otherwise</returns>
        public async Task<MovieModel> GetByIdAsync(ObjectId id)
        {
            // Find the first document matching the given id
            return await _movies.Find(movie => movie.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves all movies that belong to a specific genre
        /// </summary>
        /// <param name="genre">The genre to filter by</param>
        /// <returns>A list of movies matching the specified genre</returns>
        public async Task<List<MovieModel>> GetByGenreAsync(string genre)
        {
            // Create a filter that checks if the genre exists in the Genres array
            var filter = Builders<MovieModel>.Filter.AnyEq(m => m.Genres, genre);

            // Find all movies matching the genre filter
            return await _movies.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Gets the highest rated movies in descending order
        /// </summary>
        /// <param name="limit">Maximum number of movies to return (default: 10)</param>
        /// <returns>A list of the top rated movies</returns>
        public async Task<List<MovieModel>> GetTopRatedAsync(int limit = 10)
        {
            // Find all movies, sort by rating in descending order, and limit results
            return await _movies.Find(movie => true)
                .Sort(Builders<MovieModel>.Sort.Descending(m => m.AverageRating))
                .Limit(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Gets the most recently released movies in descending order of release date
        /// </summary>
        /// <param name="limit">Maximum number of movies to return (default: 10)</param>
        /// <returns>A list of the most recent movies</returns>
        public async Task<List<MovieModel>> GetRecentAsync(int limit = 10)
        {
            // Find all movies, sort by release date in descending order, and limit results
            return await _movies.Find(movie => true)
                .Sort(Builders<MovieModel>.Sort.Descending(m => m.ReleaseDate))
                .Limit(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new movie entry in the database
        /// </summary>
        /// <param name="movie">The movie to create</param>
        /// <returns>The created movie with generated ID</returns>
        public async Task<MovieModel> CreateAsync(MovieModel movie)
        {
            // Insert the new movie into the collection
            await _movies.InsertOneAsync(movie);

            // Return the inserted movie (now with an ID)
            return movie;
        }

        /// <summary>
        /// Updates an existing movie in the database
        /// </summary>
        /// <param name="id">The ID of the movie to update</param>
        /// <param name="updatedMovie">The updated movie data</param>
        public async Task UpdateAsync(ObjectId id, MovieModel updatedMovie)
        {
            // Replace the existing movie document with the updated one
            await _movies.ReplaceOneAsync(movie => movie.Id == id, updatedMovie);
        }

        /// <summary>
        /// Deletes a movie from the database
        /// </summary>
        /// <param name="id">The ID of the movie to delete</param>
        public async Task DeleteAsync(ObjectId id)
        {
            // Delete the movie document with the specified id
            await _movies.DeleteOneAsync(movie => movie.Id == id);
        }
    }
}

