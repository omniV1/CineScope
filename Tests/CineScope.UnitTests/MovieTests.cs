using Xunit;
using CineScope.Shared.DTOs;

namespace CineScope.UnitTests
{
    public class MovieTests
    {
        [Fact]
        public void MovieDto_WhenCreated_HasCorrectProperties()
        {
            // Arrange
            var movieDto = new MovieDto
            {
                Id = "1",
                Title = "Test Movie",
                Description = "Test Description",
                ReleaseDate = new DateTime(2024, 1, 1),
                AverageRating = 4.5,
                Genres = new List<string> { "Action", "Drama" }
            };

            // Assert
            Assert.Equal("1", movieDto.Id);
            Assert.Equal("Test Movie", movieDto.Title);
            Assert.Equal("Test Description", movieDto.Description);
            Assert.Equal(new DateTime(2024, 1, 1), movieDto.ReleaseDate);
            Assert.Equal(4.5, movieDto.AverageRating);
            Assert.Contains("Action", movieDto.Genres);
            Assert.Contains("Drama", movieDto.Genres);
        }

        [Theory]
        [InlineData(5.0, true)]
        [InlineData(4.0, true)]
        [InlineData(3.0, false)]
        [InlineData(2.0, false)]
        public void MovieDto_IsHighlyRated_ReturnsCorrectValue(double rating, bool expectedResult)
        {
            // Arrange
            var movieDto = new MovieDto
            {
                Id = "1",
                Title = "Test Movie",
                AverageRating = rating
            };

            // Act
            var isHighlyRated = movieDto.AverageRating >= 4.0;

            // Assert
            Assert.Equal(expectedResult, isHighlyRated);
        }
    }
} 