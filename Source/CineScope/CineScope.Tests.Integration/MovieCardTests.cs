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


namespace CineScope.Tests.Unit
{
    public class MovieCardTests : TestContext
    {
        public MovieCardTests()
        {
            // Register MudBlazor services
            Services.AddMudServices();
        }

        [Fact]
        public void MovieCard_ShouldRenderCorrectly()
        {
            // Arrange
            var movie = new MovieDto
            {
                Id = "1",
                Title = "Test Movie",
                ReleaseDate = new DateTime(2023, 1, 1),
                Genres = new List<string> { "Action" },
                AverageRating = 4.5,
                PosterUrl = "/images/placeholder.png"
            };

            // Act
            var cut = RenderComponent<MovieCard>(parameters =>
                parameters.Add(p => p.Movie, movie));

            // Assert
            // Check that the title is rendered correctly
            var title = cut.Find("h5");
            Assert.Contains(movie.Title, title.TextContent);

            // Check that the genre and year are displayed
            var subtitle = cut.Find(".mud-typography-body2");
            Assert.Contains("Action", subtitle.TextContent);
            Assert.Contains("2023", subtitle.TextContent);

            // Check that the Learn More button exists
            var learnMoreButton = cut.Find("button[aria-label='Learn More']");
            Assert.NotNull(learnMoreButton);

            // Check that the rating component exists with correct value
            var rating = cut.FindComponent<MudRating>();
            Assert.Equal(4, rating.Instance.SelectedValue);
        }
    }
}