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
    public class MovieListTests : TestContext
    {
        public class MockMovieCard : ComponentBase
        {
            [Parameter]
            public MovieDto Movie { get; set; }

            protected override void BuildRenderTree(RenderTreeBuilder builder)
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "mock-movie-card");
                builder.AddContent(2, Movie?.Title ?? "No Title");
                builder.CloseElement();
            }
        }

        public MovieListTests()
        {
            // Register MudBlazor services
            Services.AddMudServices();

            // Register our custom mock component that we control
            ComponentFactories.Add<MovieCard, MockMovieCard>();
        }

        [Fact]
        public void MovieList_ShouldRenderMoviesCorrectly()
        {
            // Arrange
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

            // Act
            var cut = RenderComponent<MovieList>(parameters =>
                parameters.Add(p => p.Movies, movies)
                .Add(p => p.Title, "Test Movies"));

            // Debug output
            Console.WriteLine("Rendered markup:");
            Console.WriteLine(cut.Markup);

            // Assert
            // Verify the title
            Assert.Contains("Test Movies", cut.Markup);

            // Verify our mock cards are rendered and movies are passed
            var mockCards = cut.FindAll(".mock-movie-card");
            Assert.Equal(2, mockCards.Count);
            Assert.Contains("The Shawshank Redemption", cut.Markup);
            Assert.Contains("The Godfather", cut.Markup);
        }

        [Fact]
        public void MovieList_WithNoMovies_ShouldShowEmptyMessage()
        {
            // Arrange
            var movies = new List<MovieDto>();

            // Act
            var cut = RenderComponent<MovieList>(parameters =>
                parameters.Add(p => p.Movies, movies)
                .Add(p => p.Title, "Empty List")
                .Add(p => p.EmptyMessage, "No movies available"));

            // Debug output
            Console.WriteLine("Rendered markup:");
            Console.WriteLine(cut.Markup);

            // Assert
            // Verify the title
            Assert.Contains("Empty List", cut.Markup);

            // Verify the empty message
            Assert.Contains("No movies available", cut.Markup);

            // Verify no mock cards
            var mockCards = cut.FindAll(".mock-movie-card");
            Assert.Empty(mockCards);
        }
    }
}