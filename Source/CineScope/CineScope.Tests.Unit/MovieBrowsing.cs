using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Moq;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Text;
using Moq.Protected;
using System.Threading;
using CineScope.Client.Pages.Movies;
using CineScope.Shared.DTOs;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace CineScope.Tests.Unit
{
    /// <summary>
    /// Tests for the movie browsing and filtering functionality.
    /// These tests verify requirements FR-2.1, FR-2.2, and FR-2.3.
    /// </summary>
    public class MovieBrowsingTests : TestContext
    {
        /// <summary>
        /// Sets up the test context with required services.
        /// </summary>
        public MovieBrowsingTests()
        {
            // Register MudBlazor services required by our components
            Services.AddMudServices();

            // Register mock HttpClient
            Services.AddSingleton<HttpClient>(GetMockHttpClient());
        }

        /// <summary>
        /// Tests that the genre filter correctly shows only movies of the selected genre.
        /// Verifies FR-2.1: The system shall provide filters for movie browsing and discovery.
        /// Verifies FR-2.2: The system shall allow users to select a filter category.
        /// </summary>
        [Fact]
        public void GenreFilter_ShouldShowOnlyMoviesFromSelectedGenre()
        {
            // Arrange - Render the MoviesPage component
            var cut = RenderComponent<MoviesPage>();

            // Verify initial state shows all movies
            Assert.Contains("The Dark Knight", cut.Markup);  // Action movie
            Assert.Contains("The Shawshank Redemption", cut.Markup);  // Drama movie

            // Act - Find and click the genre dropdown
            var genreDropdown = cut.Find("button[aria-label='Select Genre']");
            genreDropdown.Click();

            // Find and click the "Action" genre option
            var actionOption = cut.FindAll(".mud-list-item")[0];
            actionOption.Click();

            // Assert - Verify FR-2.3: The system shall update the displayed reviews based on the selected filter
            Assert.Contains("The Dark Knight", cut.Markup);  // Action movie should remain visible
            Assert.DoesNotContain("The Shawshank Redemption", cut.Markup);  // Drama movie should be filtered out
        }

        /// <summary>
        /// Tests that the rating filter correctly shows only movies with ratings at or above the selected rating.
        /// Further verification of FR-2.1, FR-2.2, and FR-2.3.
        /// </summary>
        [Fact]
        public void RatingFilter_ShouldShowOnlyMoviesWithSelectedRatingOrAbove()
        {
            // Arrange - Render the MoviesPage component
            var cut = RenderComponent<MoviesPage>();

            // Verify initial state shows all movies
            Assert.Contains("The Dark Knight", cut.Markup);  // 5-star movie
            Assert.Contains("Inception", cut.Markup);  // 4-star movie
            Assert.Contains("The Avengers", cut.Markup);  // 3-star movie

            // Act - Find and click the filter button
            var filterButton = cut.Find("button[aria-label='Filter Options']");
            filterButton.Click();

            // Set the rating filter to 4 stars (calling the method directly since it's difficult to interact with slider in tests)
            cut.Instance.SetRatingFilter(4);

            // Find and click Apply Filters button
            var applyButton = cut.Find("button[aria-label='Apply Filters']");
            applyButton.Click();

            // Assert - Verify that only movies with ratings >= 4 are shown
            Assert.Contains("The Dark Knight", cut.Markup);  // 5-star movie should still be visible
            Assert.Contains("Inception", cut.Markup);  // 4-star movie should still be visible
            Assert.DoesNotContain("The Avengers", cut.Markup);  // 3-star movie should be filtered out
        }

        /// <summary>
        /// Tests that multiple filters can be combined to show only movies that match all criteria.
        /// Further verification of FR-2.1, FR-2.2, and FR-2.3 with multiple filter combinations.
        /// </summary>
        [Fact]
        public void CombinedFilters_ShouldShowOnlyMoviesMatchingAllCriteria()
        {
            // Arrange - Render the MoviesPage component
            var cut = RenderComponent<MoviesPage>();

            // Verify initial state shows all movies
            Assert.Contains("The Dark Knight", cut.Markup);  // 5-star Action movie
            Assert.Contains("The Shawshank Redemption", cut.Markup);  // 5-star Drama movie
            Assert.Contains("Inception", cut.Markup);  // 4-star Sci-Fi movie

            // Act - Apply genre filter for Action
            var genreDropdown = cut.Find("button[aria-label='Select Genre']");
            genreDropdown.Click();
            var actionOption = cut.FindAll(".mud-list-item")[0];
            actionOption.Click();

            // Then apply rating filter for 5 stars
            var filterButton = cut.Find("button[aria-label='Filter Options']");
            filterButton.Click();
            cut.Instance.SetRatingFilter(5);
            var applyButton = cut.Find("button[aria-label='Apply Filters']");
            applyButton.Click();

            // Assert - Verify that only 5-star Action movies are shown
            Assert.Contains("The Dark Knight", cut.Markup);  // 5-star Action movie should still be visible
            Assert.DoesNotContain("The Shawshank Redemption", cut.Markup);  // Drama movie should be filtered out
            Assert.DoesNotContain("Inception", cut.Markup);  // 4-star movie should be filtered out
        }

        /// <summary>
        /// Tests that the clear filters functionality works correctly.
        /// Related to FR-2 series requirements for filter management.
        /// </summary>
        [Fact]
        public void ClearAllFilters_ShouldResetToShowAllMovies()
        {
            // Arrange - Render the MoviesPage component and apply some filters first
            var cut = RenderComponent<MoviesPage>();

            // Apply genre filter
            var genreDropdown = cut.Find("button[aria-label='Select Genre']");
            genreDropdown.Click();
            var actionOption = cut.FindAll(".mud-list-item")[0];
            actionOption.Click();

            // Verify the filter is applied
            Assert.Contains("The Dark Knight", cut.Markup);
            Assert.DoesNotContain("The Shawshank Redemption", cut.Markup);

            // Act - Find and click the Clear All button
            // Find filter chips section and then the clear all button
            var clearButton = cut.Find("button[color='Error']");
            clearButton.Click();

            // Assert - Verify all movies are shown again
            Assert.Contains("The Dark Knight", cut.Markup);
            Assert.Contains("The Shawshank Redemption", cut.Markup);
            Assert.Contains("Inception", cut.Markup);
        }

        /// <summary>
        /// Tests that the search function correctly filters movies based on title.
        /// Related to FR-2 series requirements for content discovery.
        /// </summary>
        [Fact]
        public void Search_ShouldFilterMoviesByTitle()
        {
            // Arrange - Render the MoviesPage component
            var cut = RenderComponent<MoviesPage>();

            // Verify initial state shows all movies
            Assert.Contains("The Dark Knight", cut.Markup);
            Assert.Contains("The Shawshank Redemption", cut.Markup);

            // Act - Find the search field and enter a search term
            var searchField = cut.Find("input[aria-label='Search Movies']");
            searchField.Change("knight");  // Input the search term

            // Click the search button
            var searchButton = cut.Find("button[aria-label='Search']");
            searchButton.Click();

            // Assert - Only movies with "knight" in the title should be visible
            Assert.Contains("The Dark Knight", cut.Markup);  // Should be visible
            Assert.DoesNotContain("The Shawshank Redemption", cut.Markup);  // Should be hidden
        }

        /// <summary>
        /// Creates a mock HttpClient that returns predefined responses for movie API requests.
        /// This simulates the backend API for testing the UI components.
        /// </summary>
        private HttpClient GetMockHttpClient()
        {
            // Create mock movie data
            var allMovies = new List<MovieDto>
            {
                new MovieDto
                {
                    Id = "1",
                    Title = "The Dark Knight",
                    Genres = new List<string> { "Action", "Crime", "Drama" },
                    ReleaseDate = new DateTime(2008, 7, 18),
                    AverageRating = 5.0,
                    PosterUrl = "/images/dark_knight.jpg"
                },
                new MovieDto
                {
                    Id = "2",
                    Title = "The Shawshank Redemption",
                    Genres = new List<string> { "Drama" },
                    ReleaseDate = new DateTime(1994, 9, 23),
                    AverageRating = 5.0,
                    PosterUrl = "/images/shawshank.jpg"
                },
                new MovieDto
                {
                    Id = "3",
                    Title = "Inception",
                    Genres = new List<string> { "Action", "Sci-Fi" },
                    ReleaseDate = new DateTime(2010, 7, 16),
                    AverageRating = 4.0,
                    PosterUrl = "/images/inception.jpg"
                },
                new MovieDto
                {
                    Id = "4",
                    Title = "The Avengers",
                    Genres = new List<string> { "Action", "Adventure" },
                    ReleaseDate = new DateTime(2012, 5, 4),
                    AverageRating = 3.0,
                    PosterUrl = "/images/avengers.jpg"
                }
            };

            // Create a mock HttpMessageHandler
            var messageHandlerMock = new Mock<HttpMessageHandler>();

            // Configure mock handler for different API requests

            // All movies
            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("api/Movie")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(allMovies), Encoding.UTF8, "application/json")
                });

            // Action genre movies
            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("genre=Action")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(
                        allMovies.Where(m => m.Genres.Contains("Action")).ToList()),
                        Encoding.UTF8,
                        "application/json")
                });

            // Rating 4+ movies
            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("minRating=4")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(
                        allMovies.Where(m => m.AverageRating >= 4).ToList()),
                        Encoding.UTF8,
                        "application/json")
                });

            // Action genre AND rating 5 movies
            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Contains("genre=Action") &&
                        req.RequestUri.ToString().Contains("minRating=5")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(
                        allMovies.Where(m => m.Genres.Contains("Action") && m.AverageRating >= 5).ToList()),
                        Encoding.UTF8,
                        "application/json")
                });

            // Search for "knight"
            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("search=knight")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(
                        allMovies.Where(m => m.Title.ToLower().Contains("knight")).ToList()),
                        Encoding.UTF8,
                        "application/json")
                });

            // Create and return the HttpClient with mocked handler
            return new HttpClient(messageHandlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
        }
    }
}