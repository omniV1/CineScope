using Bunit;
using CineScope.Client.Components;
using CineScope.Client.Components.Movies;
using CineScope.Server.Interfaces;
using CineScope.Shared.DTOs;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace CineScope.Tests.Integration
{
    /// <summary>
    /// Contains unit tests for the MovieCard component.
    /// These tests verify that the MovieCard renders correctly with various inputs.
    /// Uses bUnit for Blazor component testing.
    /// </summary>
    public class MovieCardTests : TestContext
    {
        /// <summary>
        /// Constructor sets up the test context with required services.
        /// </summary>
        public MovieCardTests()
        {
            // Register MudBlazor services required by our components
            // This ensures MudBlazor components (like MudCard, MudRating) can be rendered
            Services.AddMudServices();
        }

        /// <summary>
        /// Tests that the MovieCard component renders with the correct movie information.
        /// 
        /// This test verifies that:
        /// - The movie title is displayed correctly
        /// - Genre and release year information is shown
        /// - The rating is displayed with the correct number of stars
        /// - The "Learn More" button is present
        /// </summary>
        [Fact]
        public void MovieCard_ShouldRenderCorrectly()
        {
            // Arrange - Create a test movie with all required properties
            var movie = new MovieDto
            {
                Id = "1",
                Title = "Test Movie",
                ReleaseDate = new DateTime(2023, 1, 1),  // 2023 release year
                Genres = new List<string> { "Action" },  // Action genre
                AverageRating = 4.5,                     // 4.5 star rating
                PosterUrl = "/images/placeholder.png"    // Placeholder image
            };

            // Act - Render the MovieCard component with our test movie
            var cut = RenderComponent<MovieCard>(parameters =>
                parameters.Add(p => p.Movie, movie));    // Pass movie as a parameter

            // Assert - Verify the component renders as expected

            // Check that the title is rendered correctly
            var title = cut.Find("h5");                  // Find the h5 element containing the title
            Assert.Contains(movie.Title, title.TextContent);  // Title should be displayed

            // Check that the genre and year are displayed
            var subtitle = cut.Find(".mud-typography-body2");  // Find the subtitle element
            Assert.Contains("Action", subtitle.TextContent);   // Genre should be shown
            Assert.Contains("2023", subtitle.TextContent);     // Year should be shown

            // Check that the Learn More button exists
            var learnMoreButton = cut.Find("button[aria-label='Learn More']");
            Assert.NotNull(learnMoreButton);             // Button should be present

            // Check that the rating component exists with correct value
            // Note: The rating is 4.5 but displayed as 4 stars due to rounding or integer conversion
            var rating = cut.FindComponent<MudRating>();
            Assert.Equal(4, rating.Instance.SelectedValue);  // Should show 4 stars
        }
    }
}