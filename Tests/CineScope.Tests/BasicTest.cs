using Xunit;

namespace CineScope.Tests
{
    public class BasicTest
    {
        [Fact]
        public void SimpleTest_ShouldPass()
        {
            // Arrange
            var expected = true;

            // Act
            var actual = true;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
} 