using CineScope.Shared.Models;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace CineScope.Services
{
    public class MongoDBIndexService
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDBSettings _settings;

        public MongoDBIndexService(MongoDBSettings settings)
        {
            _settings = settings;
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        public async Task CreateIndexesAsync()
        {
            // User collection indexes
            var usersCollection = _database.GetCollection<UserModel>(_settings.UsersCollectionName);
            await usersCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<UserModel>(
                    Builders<UserModel>.IndexKeys.Ascending(user => user.Username),
                    new CreateIndexOptions { Unique = true }));

            await usersCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<UserModel>(
                    Builders<UserModel>.IndexKeys.Ascending(user => user.Email),
                    new CreateIndexOptions { Unique = true }));

            await usersCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<UserModel>(
                    Builders<UserModel>.IndexKeys.Descending(user => user.LastLogin)));

            // Movie collection indexes
            var moviesCollection = _database.GetCollection<MovieModel>(_settings.MoviesCollectionName);
            await moviesCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<MovieModel>(
                    Builders<MovieModel>.IndexKeys.Text(movie => movie.Title)));

            await moviesCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<MovieModel>(
                    Builders<MovieModel>.IndexKeys.Ascending(movie => movie.Genres)));

            await moviesCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<MovieModel>(
                    Builders<MovieModel>.IndexKeys.Descending(movie => movie.ReleaseDate)));

            await moviesCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<MovieModel>(
                    Builders<MovieModel>.IndexKeys.Descending(movie => movie.AverageRating)));

            // Review collection indexes
            var reviewsCollection = _database.GetCollection<ReviewModel>(_settings.ReviewsCollectionName);
            await reviewsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<ReviewModel>(
                    Builders<ReviewModel>.IndexKeys.Ascending(review => review.MovieId)));

            await reviewsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<ReviewModel>(
                    Builders<ReviewModel>.IndexKeys.Ascending(review => review.UserId)));

            await reviewsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<ReviewModel>(
                    Builders<ReviewModel>.IndexKeys.Descending(review => review.CreatedAt)));

            await reviewsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<ReviewModel>(
                    Builders<ReviewModel>.IndexKeys
                        .Ascending(review => review.MovieId)
                        .Descending(review => review.CreatedAt)));
        }
    }
}