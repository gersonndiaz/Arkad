namespace Arkad.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

        }

        [Fact]
        public void Sum_ShouldReturnCorrectValue()
        {
            // Arrange
            var expenses = new List<double> { 100, 200, 300 };

            // Act
            var result = expenses.Sum();

            // Assert
            Assert.Equal(600, result);
        }

        [Theory]
        [InlineData(100, 200, 300)]
        [InlineData(50, 50, 100)]
        public void Sum_ShouldWorkWithMultipleValues(double a, double b, double expected)
        {
            // Act
            var result = a + b;

            // Assert
            Assert.Equal(expected, result);
        }
    }
}