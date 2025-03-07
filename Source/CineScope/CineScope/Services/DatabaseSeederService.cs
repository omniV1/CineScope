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
    /// <summary>
    /// Service responsible for seeding the database with initial data
    /// Provides sample users, movies, reviews, and banned words for development and testing
    /// </summary>
    public class DatabaseSeederService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IBannedWordRepository _bannedWordRepository;

        /// <summary>
        /// Constructor for DatabaseSeederService
        /// </summary>
        /// <param name="userRepository">Repository for user operations</param>
        /// <param name="movieRepository">Repository for movie operations</param>
        /// <param name="reviewRepository">Repository for review operations</param>
        /// <param name="bannedWordRepository">Repository for banned word operations</param>
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

        /// <summary>
        /// Seeds the database with initial data if collections are empty
        /// </summary>
        /// <returns>Task representing the seeding operation</returns>
        public async Task SeedDatabaseAsync()
        {
            // Seed each collection in sequence
            await SeedUsersAsync();
            await SeedMoviesAsync();
            await SeedBannedWordsAsync();
            await SeedReviewsAsync();
        }

        /// <summary>
        /// Seeds the database with sample users if the users collection is empty
        /// </summary>
        /// <returns>Task representing the user seeding operation</returns>
        private async Task SeedUsersAsync()
        {
            // Check if users already exist to avoid duplicate seeding
            if ((await _userRepository.GetAllAsync()).Count > 0)
                return;

            // Create sample users including an admin and regular users
            var users = new List<UserModel>
            {
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    username = "admin",
                    Email = "admin@cinescope.com",
                    PasswordHash = HashPassword("Admin@123"),
                    Roles = ["Admin", "User"], // Admin has multiple roles
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
                    Roles = ["User"], // Regular user role
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
                    Roles = ["User"], // Regular user role
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                }
            };

            // Add each user to the database
            foreach (var user in users)
            {
                await _userRepository.CreateAsync(user);
            }
        }

        /// <summary>
        /// Seeds the database with sample movies if the movies collection is empty
        /// </summary>
        /// <returns>Task representing the movie seeding operation</returns>
        private async Task SeedMoviesAsync()
        {
            // Check if movies already exist to avoid duplicate seeding
            if ((await _movieRepository.GetAllAsync()).Count > 0)
                return;

            // Create sample movies with realistic data
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
                    AverageRating = 9.3, // Initial rating
                    ReviewCount = 0      // No reviews yet
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
                    AverageRating = 9.2, // Initial rating
                    ReviewCount = 0      // No reviews yet
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
                    AverageRating = 9.0, // Initial rating
                    ReviewCount = 0      // No reviews yet
                }
            };

            // Add each movie to the database
            foreach (var movie in movies)
            {
                await _movieRepository.CreateAsync(movie);
            }
        }

        /// <summary>
        /// Seeds the database with sample banned words if the banned words collection is empty
        /// </summary>
        /// <returns>Task representing the banned words seeding operation</returns>
        private async Task SeedBannedWordsAsync()
        {
            // Check if banned words already exist to avoid duplicate seeding
            if ((await _bannedWordRepository.GetAllAsync()).Count > 0)
                return;

            // Create sample banned words with different severity levels and categories
            // Note: Using placeholders instead of actual profanity for the sample data
            var bannedWords = new List<BannedWordModel>
            {
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    Word = "profanity1",         // Placeholder for actual profanity
                    Severity = 3,                // High severity (scale 1-3)
                    Category = "Profanity",
                    IsActive = true,             // This word is currently being filtered
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    Word = "profanity2",         // Placeholder for actual profanity
                    Severity = 2,                // Medium severity (scale 1-3)
                    Category = "Profanity",
                    IsActive = true,             // This word is currently being filtered
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    Word = "slur1",              // Placeholder for actual slur
                    Severity = 3,                // High severity (scale 1-3)
                    Category = "Slur",
                    IsActive = true,             // This word is currently being filtered
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            // Add each banned word to the database
            foreach (var word in bannedWords)
            {
                await _bannedWordRepository.CreateAsync(word);
            }
        }

        /// <summary>
        /// Seeds the database with sample reviews if the reviews collection is empty
        /// Also updates movie ratings based on these reviews
        /// </summary>
        /// <returns>Task representing the reviews seeding operation</returns>
        private async Task SeedReviewsAsync()
        {
            // Check if reviews already exist to avoid duplicate seeding
            if ((await _reviewRepository.GetAllAsync()).Count > 0)
                return;

            // Get users and movies to reference in reviews
            var users = await _userRepository.GetAllAsync();
            var movies = await _movieRepository.GetAllAsync();

            // Ensure we have users and movies to create reviews for
            if (users.Count == 0 || movies.Count == 0)
                return;

            // Create sample reviews for the first movie
            var reviews = new List<ReviewModel>
            {
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    UserId = users[1].Id,          // johndoe
                    MovieId = movies[0].Id,        // Shawshank Redemption
                    Rating = 5.0,                  // Perfect rating
                    Text = "One of the best movies ever made. The story, acting, and direction are all perfect.",
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30),
                    IsApproved = true,             // Review is approved and visible
                    FlaggedWords = []              // No flagged words in this review
                },
                new()
                {
                    Id = ObjectId.GenerateNewId(),
                    UserId = users[2].Id,          // janesmith
                    MovieId = movies[0].Id,        // Shawshank Redemption
                    Rating = 4.5,                  // Very good rating
                    Text = "A masterpiece that stands the test of time. Morgan Freeman's narration is iconic.",
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    UpdatedAt = DateTime.UtcNow.AddDays(-25),
                    IsApproved = true,             // Review is approved and visible
                    FlaggedWords = []              // No flagged words in this review
                }
            };

            // Add each review and update movie ratings
            foreach (var review in reviews)
            {
                // Save the review
                await _reviewRepository.CreateAsync(review);

                // Update the movie's average rating based on all approved reviews
                var movie = await _movieRepository.GetByIdAsync(review.MovieId);
                var movieReviews = await _reviewRepository.GetByMovieIdAsync(review.MovieId);
                var approvedReviews = movieReviews.Where(r => r.IsApproved).ToList();

                if (approvedReviews.Count > 0)
                {
                    // Calculate new average rating and update review count
                    movie.AverageRating = approvedReviews.Average(r => r.Rating);
                    movie.ReviewCount = approvedReviews.Count;
                    await _movieRepository.UpdateAsync(movie.Id, movie);
                }
            }
        }

        /// <summary>
        /// Creates a simple hash of a password using SHA256
        /// Note: For production, use a more secure method with salt and proper password hashing
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>The hashed password</returns>
        private static string HashPassword(string password)
        {
            // Create SHA256 hasher
            using var sha256 = SHA256.Create();

            // Convert password to bytes and compute hash
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Convert bytes to string (lowercase hex format without dashes)
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }
}

