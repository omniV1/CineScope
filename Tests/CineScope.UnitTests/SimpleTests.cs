using Xunit;

namespace CineScope.UnitTests
{
    public class SimpleTests
    {
        [Fact]
        public void TrueIsTrue()
        {
            Assert.True(true);
        }

        [Fact]
        public void OneEqualsOne()
        {
            Assert.Equal(1, 1);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(2, 2, 4)]
        [InlineData(5, 5, 10)]
        public void AddTwoNumbers_ReturnsSum(int a, int b, int expected)
        {
            var result = a + b;
            Assert.Equal(expected, result);
        }
    }
} 