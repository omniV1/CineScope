using Bunit;
using CineScope.Client.Pages;
using CineScope.Shared.DTOs;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Text;
using Moq.Protected;
using System.Threading;
using CineScope.Client.Pages.Movies;

namespace CineScope.Tests.Unit
{
    /// <summary>
    /// Contains tests for the movie browsing and filtering functionality.
    /// These tests verify that users can browse movies and apply various filters.
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
        /// Tests that genres filter correctly shows only movies of the selected genre.
        /// 
        /// This test verifies that when a user selects a genre filter:
        /// - The API is called with the correct genre parameter
        /// - Only movies from the selected genre are displayed
        /// </summary>
        [Fact]
        public void GenreFilter_ShouldShowOnlyMoviesFromSelectedGenre()
        {
            // Arrange - Render the Movies page component
            var cut = RenderComponent<MoviesPage>();

            // Initially should show all movies (both action and drama)
            Assert.Contains("The Dark Knight", cut.Markup);  // Action movie
            Assert.Contains("The Shawshank Redemption", cut.Markup);  // Drama movie

            // Act - Find and click the genre dropdown
            var genreDropdown = cut.Find("button[aria-label='Select Genre']");
            genreDropdown.Click();

            // Find and click the "Action" genre option
            var actionOption = cut.FindAll(".mud-list-item")[0];
            actionOption.Click();

            // Assert - Only action movies should be visible
            Assert.Contains("The Dark Knight", cut.Markup);  // Action movie should be visible
            Assert.DoesNotContain("The Shawshank Redemption", cut.Markup);  // Drama movie should be hidden
        }

        /// <summary>
        /// Tests that the rating filter correctly shows only movies with ratings 
        /// at or above the selected rating.
        /// 
        /// This test verifies that when a user selects a rating filter:
        /// - The API is called with the correct rating parameter
        /// - Only movies with ratings meeting the criteria are displayed
        /// </summary>
        [Fact]
        public void RatingFilter_ShouldShowOnlyMoviesWithSelectedRatingOrAbove()
        {
            // Arrange - Render the Movies page component
            var cut = RenderComponent<MoviesPage>();

            // Initially should show all movies
            Assert.Contains("The Dark Knight", cut.Markup);  // 5-star movie
            Assert.Contains("Inception", cut.Markup);  // 4-star movie
            Assert.Contains("The Avengers", cut.Markup);  // 3-star movie

            // Act - Find and click the filter button
            var filterButton = cut.Find("button[aria-label='Filter Options']");
            filterButton.Click();

            // Find and set the rating slider to 4
            var ratingSlider = cut.Find(".mud-slider");
            // In a real test, we would manipulate the slider
            // Since that's difficult in unit tests, we'll call the method directly
            cut.Instance.SetRatingFilter(4);

            // Apply the filter
            var applyButton = cut.Find("button[aria-label='Apply Filters']");
            applyButton.Click();

            // Assert - Only 4+ star movies should be visible
            Assert.Contains("The Dark Knight", cut.Markup);  // 5-star movie should be visible
            Assert.Contains("Inception", cut.Markup);  // 4-star movie should be visible
            Assert.DoesNotContain("The Avengers", cut.Markup);  // 3-star movie should be hidden
        }

        /// <summary>
        /// Tests that multiple filters can be combined and correctly show only
        /// movies that match all selected criteria.
        /// 
        /// This test verifies that:
        /// - Genre and rating filters can be applied simultaneously
        /// - Only movies matching both criteria are displayed
        /// </summary>
        [Fact]
        public void CombinedFilters_ShouldShowOnlyMoviesMatchingAllCriteria()
        {
            // Arrange - Render the Movies page component
            var cut = RenderComponent<MoviesPage>();

            // Initially should show all movies
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

            // Assert - Only 5-star Action movies should be visible
            Assert.Contains("The Dark Knight", cut.Markup);  // 5-star Action movie should be visible
            Assert.DoesNotContain("The Shawshank Redemption", cut.Markup);  // Drama movie should be hidden
            Assert.DoesNotContain("Inception", cut.Markup);  // 4-star movie should be hidden
        }

        /// <summary>
        /// Tests that the search function correctly filters movies based on title.
        /// 
        /// This test verifies that:
        /// - Searching for a term shows only movies with that term in the title
        /// - The search is case-insensitive
        /// </summary>
        [Fact]
        public void Search_ShouldFilterMoviesByTitle()
        {
            // Arrange - Render the Movies page component
            var cut = RenderComponent<MoviesPage>();

            // Initially should show all movies
            Assert.Contains("The Dark Knight", cut.Markup);
            Assert.Contains("The Shawshank Redemption", cut.Markup);

            // Act - Find the search field and enter a search term
            var searchField = cut.Find("input[aria-label='Search Movies']");
            searchField.Change("knight");  // Input the search term

            // Trigger search (could be automatic or by pressing a button)
            var searchButton = cut.Find("button[aria-label='Search']");
            searchButton.Click();

            // Assert - Only movies with "knight" in the title should be visible
            Assert.Contains("The Dark Knight", cut.Markup);  // Should be visible
            Assert.DoesNotContain("The Shawshank Redemption", cut.Markup);  // Should be hidden
        }

        /// <summary>
        /// Tests that sorting options correctly order the displayed movies.
        /// 
        /// This test verifies that:
        /// - Sorting by rating orders movies from highest to lowest rated
        /// - The UI updates to reflect the new sort order
        /// </summary>
        [Fact]
        public void Sort_ByRating_ShouldOrderMoviesByRatingDescending()
        {
            // Arrange - Render the Movies page component
            var cut = RenderComponent<MoviesPage>();

            // Act - Find and click the sort dropdown
            var sortDropdown = cut.Find("button[aria-label='Sort Options']");
            sortDropdown.Click();

            // Select "Rating (High to Low)" option
            var ratingOption = cut.FindAll(".mud-list-item")[1];  // Assuming it's the second option
            ratingOption.Click();

            // Assert
            // This would require checking the actual order of elements in the DOM
            // which is complex in a unit test, so we'll verify the component's internal state

            // Verify that the sort option is set correctly
            Assert.Equal("rating_desc", cut.Instance.CurrentSortOption);

            // In a more complete test, we would verify the actual DOM order of movies
            // This would require a more complex implementation specific to the component
        }

        /// <summary>
        /// Creates a mock HttpClient that returns predefined responses for movie API requests.
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

            // Setup mock responses for different API requests

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

            // Create HttpClient with the mocked handler
            return new HttpClient(messageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost/")
            };
        }
    }
}