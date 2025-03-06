using CineScope.Shared.Interfaces;
using CineScope.Shared.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CineScope.Services
{
    public class DatabaseSeederService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IBannedWordRepository _bannedWordRepository;

        public DatabaseSeederService(
            IUserRepository userRepository,
            IMovieRepository movieRepository,
            IReviewRepository reviewRepository,
            IBannedWordRepository bannedWordRepository)
        {
            _userRepository = userRepository;
            _movieRepository = movieRepository;
            _reviewRepository = reviewRepository;
            _bannedWordRepository = bannedWordRepository;
        }

        public async Task SeedDatabaseAsync()
        {
            await SeedUsersAsync();
            await SeedMoviesAsync();
            await SeedBannedWordsAsync();
            await SeedReviewsAsync();
        }

        private async Task SeedUsersAsync()
        {
            if ((await _userRepository.GetAllAsync()).Count > 0)
                return;

            var users = new List<UserModel>
            {
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    username = "admin",
                    Email = "admin@cinescope.com",
                    PasswordHash = HashPassword("Admin@123"),
                    Roles = ["Admin", "User"],
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                },
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    username = "johndoe",
                    Email = "john@example.com",
                    PasswordHash = HashPassword("User@123"),
                    Roles = ["User"],
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                },
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    username = "janesmith",
                    Email = "jane@example.com",
                    PasswordHash = HashPassword("User@123"),
                    Roles = ["User"],
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                }
            };

            foreach (var user in users)
            {
                await _userRepository.CreateAsync(user);
            }
        }

        private async Task SeedMoviesAsync()
        {
            if ((await _movieRepository.GetAllAsync()).Count > 0)
                return;

            var movies = new List<MovieModel>
            {
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    Title = "The Shawshank Redemption",
                    Description = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.",
                    ReleaseDate = new DateTime(1994, 9, 23),
                    Genres = ["Drama"],
                    Director = "Frank Darabont",
                    Actors = ["Tim Robbins", "Morgan Freeman", "Bob Gunton"],
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BNDE3ODcxYzMtY2YzZC00NmNlLWJiNDMtZDViZWM2MzIxZDYwXkEyXkFqcGdeQXVyNjAwNDUxODI@._V1_.jpg",
                    AverageRating = 9.3,
                    ReviewCount = 0
                },
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    Title = "The Godfather",
                    Description = "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant son.",
                    ReleaseDate = new DateTime(1972, 3, 24),
                    Genres = ["Crime", "Drama"],
                    Director = "Francis Ford Coppola",
                    Actors = ["Marlon Brando", "Al Pacino", "James Caan"],
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BM2MyNjYxNmUtYTAwNi00MTYxLWJmNWYtYzZlODY3ZTk3OTFlXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg",
                    AverageRating = 9.2,
                    ReviewCount = 0
                },
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    Title = "The Dark Knight",
                    Description = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.",
                    ReleaseDate = new DateTime(2008, 7, 18),
                    Genres = ["Action", "Crime", "Drama"],
                    Director = "Christopher Nolan",
                    Actors = ["Christian Bale", "Heath Ledger", "Aaron Eckhart"],
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BMTMxNTMwODM0NF5BMl5BanBnXkFtZTcwODAyMTk2Mw@@._V1_.jpg",
                    AverageRating = 9.0,
                    ReviewCount = 0
                }
            };

            foreach (var movie in movies)
            {
                await _movieRepository.CreateAsync(movie);
            }
        }

        private async Task SeedBannedWordsAsync()
        {
            if ((await _bannedWordRepository.GetAllAsync()).Count > 0)
                return;

            var bannedWords = new List<BannedWordModel>
            {
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    Word = "profanity1",
                    Severity = 3,
                    Category = "Profanity",
                    IsActive = true,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    Word = "profanity2",
                    Severity = 2,
                    Category = "Profanity",
                    IsActive = true,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    Word = "slur1",
                    Severity = 3,
                    Category = "Slur",
                    IsActive = true,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            foreach (var word in bannedWords)
            {
                await _bannedWordRepository.CreateAsync(word);
            }
        }

        private async Task SeedReviewsAsync()
        {
            if ((await _reviewRepository.GetAllAsync()).Count > 0)
                return;

            var users = await _userRepository.GetAllAsync();
            var movies = await _movieRepository.GetAllAsync();

            if (users.Count == 0 || movies.Count == 0)
                return;

            var reviews = new List<ReviewModel>
            {
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    UserId = users[1].Id, // johndoe
                    MovieId = movies[0].Id, // Shawshank Redemption
                    Rating = 5.0,
                    Text = "One of the best movies ever made. The story, acting, and direction are all perfect.",
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30),
                    IsApproved = true,
                    FlaggedWords = []
                },
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    UserId = users[2].Id, // janesmith
                    MovieId = movies[0].Id, // Shawshank Redemption
                    Rating = 4.5,
                    Text = "A masterpiece that stands the test of time. Morgan Freeman's narration is iconic.",
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    UpdatedAt = DateTime.UtcNow.AddDays(-25),
                    IsApproved = true,
                    FlaggedWords = []
                }
            };

            foreach (var review in reviews)
            {
                await _reviewRepository.CreateAsync(review);

                // Update the movie's average rating
                var movie = await _movieRepository.GetByIdAsync(review.MovieId);
                var movieReviews = await _reviewRepository.GetByMovieIdAsync(review.MovieId);
                var approvedReviews = movieReviews.Where(r => r.IsApproved).ToList();

                if (approvedReviews.Count > 0)
                {
                    movie.AverageRating = approvedReviews.Average(r => r.Rating);
                    movie.ReviewCount = approvedReviews.Count;
                    await _movieRepository.UpdateAsync(movie.Id, movie);
                }
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }
}