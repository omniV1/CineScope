using Microsoft.AspNetCore.Mvc;
using CineScope.Shared.Models;
using CineScope.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace CineScope.Controllers
{
    [ApiController]
    [Route("api/debug")]
    public class DebugController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly ILogger<DebugController> _logger;

        public DebugController(IMovieService movieService, ILogger<DebugController> logger)
        {
            _movieService = movieService;
            _logger = logger;
        }

        [HttpGet("movie-ids")]
        public async Task<IActionResult> DebugMovieIds()
        {
            try
            {
                var movies = await _movieService.GetAllMoviesAsync();
                var result = new List<object>();

                foreach (var movie in movies)
                {
                    result.Add(new
                    {
                        IdValue = movie.Id.ToString(),
                        IdType = movie.Id.GetType().FullName,
                        IsEmpty = movie.Id == ObjectId.Empty,
                        Title = movie.Title,
                        RawId = movie.Id
                    });
                }

                return Ok(new
                {
                    Count = movies.Count,
                    Movies = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error debugging movie IDs");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("add-sample-movie")]
        public async Task<IActionResult> AddSampleMovie()
        {
            try
            {
                var movieId = ObjectId.GenerateNewId();

                _logger.LogInformation($"Generated new ObjectID: {movieId}");

                var movie = new MovieModel
                {
                    Id = movieId,
                    Title = "Test Movie " + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    Description = "This is a test movie added for debugging purposes.",
                    ReleaseDate = DateTime.Now,
                    Genres = new List<string> { "Test", "Debug" },
                    Director = "Debug Director",
                    Actors = new List<string> { "Actor 1", "Actor 2" },
                    PosterUrl = "https://via.placeholder.com/300x450?text=Test+Movie",
                    AverageRating = 5.0,
                    ReviewCount = 1
                };

                var createdMovie = await _movieService.CreateMovieAsync(movie);

                _logger.LogInformation($"Movie created with ID: {createdMovie.Id}");

                return Ok(new
                {
                    message = "Sample movie added successfully",
                    movie = new
                    {
                        IdValue = createdMovie.Id.ToString(),
                        IdType = createdMovie.Id.GetType().FullName,
                        IsEmpty = createdMovie.Id == ObjectId.Empty,
                        Title = createdMovie.Title
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding sample movie");
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}