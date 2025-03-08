using Bunit;
using CineScope.Client.Components.Movies;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace CineScope.Tests.Unit
{
    /// <summary>
    /// Contains unit tests for the MovieList component.
    /// These tests verify that the MovieList renders correctly with various inputs.
    /// Uses bUnit for Blazor component testing.
    /// </summary>
    public class MovieListTests : TestContext
    {
        /// <summary>
        /// Mock implementation of the MovieCard component to simplify testing.
        /// 
        /// This mock component accepts a Movie parameter and renders just the title,
        /// making it easier to verify which movies are being passed to child components.
        /// </summary>
        public class MockMovieCard : ComponentBase
        {
            /// <summary>
            /// Movie data to be displayed by this card.
            /// </summary>
            [Parameter]
            public MovieDto Movie { get; set; } = new MovieDto();

            /// <summary>
            /// Builds the component's render tree.
            /// </summary>
            /// <param name="builder">The builder used to construct the render tree.</param>
            protected override void BuildRenderTree(RenderTreeBuilder builder)
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "mock-movie-card");
                builder.AddContent(2, Movie?.Title ?? "No Title");
                builder.CloseElement();
            }
        }

        /// <summary>
        /// Constructor sets up the test context with required services and component replacements.
        /// </summary>
        public MovieListTests()
        {
            // Register MudBlazor services required by our components
            Services.AddMudServices();

            // Replace the actual MovieCard component with our mock version
            // This simplifies testing by focusing only on the MovieList behavior
            ComponentFactories.Add<MovieCard, MockMovieCard>();
        }

        /// <summary>
        /// Tests that the MovieList component renders a list of movies correctly.
        /// 
        /// This test verifies that:
        /// - The section title is displayed correctly
        /// - The correct number of movie cards are rendered
        /// - Each movie's information is passed to the corresponding card
        /// </summary>
        [Fact]
        public void MovieList_ShouldRenderMoviesCorrectly()
        {
            // Arrange - Create a list of sample movies
            var movies = new List<MovieDto>
            {
                new MovieDto
                {
                    Id = "1",
                    Title = "The Shawshank Redemption",
                    ReleaseDate = new DateTime(1994, 9, 23)
                },
                new MovieDto
                {
                    Id = "2",
                    Title = "The Godfather",
                    ReleaseDate = new DateTime(1972, 3, 24)
                }
            };

            // Act - Render the MovieList component with our test movies
            var cut = RenderComponent<MovieList>(parameters =>
                parameters.Add(p => p.Movies, movies)     // Pass the movie list
                .Add(p => p.Title, "Test Movies"));       // Set the section title

            // Output rendered markup for debugging
            Console.WriteLine("Rendered markup:");
            Console.WriteLine(cut.Markup);

            // Assert - Verify the component renders as expected
            // Verify the title is displayed
            Assert.Contains("Test Movies", cut.Markup);

            // Verify the correct number of movie cards are rendered
            var mockCards = cut.FindAll(".mock-movie-card");
            Assert.Equal(2, mockCards.Count);

            // Verify each movie title appears in the output
            Assert.Contains("The Shawshank Redemption", cut.Markup);
            Assert.Contains("The Godfather", cut.Markup);
        }

        /// <summary>
        /// Tests that the MovieList component shows an empty message when there are no movies.
        /// 
        /// This test verifies that:
        /// - The section title is still displayed
        /// - The empty message is shown instead of movie cards
        /// - No movie cards are rendered
        /// </summary>
        [Fact]
        public void MovieList_WithNoMovies_ShouldShowEmptyMessage()
        {
            // Arrange - Create an empty movie list
            var movies = new List<MovieDto>();

            // Act - Render the MovieList component with an empty list
            var cut = RenderComponent<MovieList>(parameters =>
                parameters.Add(p => p.Movies, movies)           // Pass empty movie list
                .Add(p => p.Title, "Empty List")                // Set section title
                .Add(p => p.EmptyMessage, "No movies available")); // Set empty message

            // Output rendered markup for debugging
            Console.WriteLine("Rendered markup:");
            Console.WriteLine(cut.Markup);

            // Assert - Verify the component renders as expected
            // Verify the title is displayed
            Assert.Contains("Empty List", cut.Markup);

            // Verify the empty message is displayed
            Assert.Contains("No movies available", cut.Markup);

            // Verify no movie cards are rendered
            var mockCards = cut.FindAll(".mock-movie-card");
            Assert.Empty(mockCards);
        }
    }
}