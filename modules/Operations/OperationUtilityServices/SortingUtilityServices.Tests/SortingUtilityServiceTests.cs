using Xunit;
using SortingUtilityServices;
using System.Collections.Generic;

namespace SortingUtilityServices.Tests
{
    public class SortingUtilityServiceTests
    {
        private readonly ISortingUtilityService _service;

        public SortingUtilityServiceTests()
        {
            _service = new SortingUtilityService();
        }

        [Fact]
        public void ConditionallySort_ShouldSwap_WhenFirstValueIsGreater()
        {
            // Arrange
            int a = 10;
            int b = 5;

            // Act
            (a,b) = _service.GetSortedPair(a , b);

            // Assert
            Assert.Equal(5 , a);
            Assert.Equal(10 , b);
        }

        [Fact]
        public void ConditionallySort_ShouldNotSwap_WhenSecondValueIsGreater()
        {
            // Arrange
            int a = 3;
            int b = 8;

            // Act
            (a , b) = _service.GetSortedPair(a , b);

            // Assert
            Assert.Equal(3 , a);
            Assert.Equal(8 , b);
        }

        [Fact]
        public void ConditionallySort_ShouldWork_WithCustomComparer()
        {
            // Arrange: 測試字串長度比較（由短到長）
            string a = "Banana";
            string b = "Apple";
            var lengthComparer = Comparer<string>.Create((x , y) => x.Length.CompareTo(y.Length));

            // Act: 因為 Banana 比 Apple 長，應該要交換
            (a , b) = _service.GetSortedPair(a , b, lengthComparer);

            // Assert
            Assert.Equal("Apple" , a);
            Assert.Equal("Banana" , b);
        }

        [Theory]
        [InlineData(1 , 1)]
        [InlineData(5 , 5)]
        public void ConditionallySort_ShouldHandleEqualValues(int val1 , int val2)
        {
            // Arrange
            int a = val1;
            int b = val2;

            // Act
            (a , b) = _service.GetSortedPair(a , b);

            // Assert
            Assert.Equal(val1 , a);
            Assert.Equal(val2 , b);
        }

        [Theory]
        [InlineData('A','B')]
        [InlineData('C' , 'D')]
        [InlineData('D' , 'C')]
        public void ConditionallySort_ForChar(params char[] chars)
        {
            // Act
            (chars [ 0 ] , chars [ 1 ]) = _service.GetSortedPair(chars [0] , chars [1]);

            // Assert
            Assert.True(chars [0] <= chars [1]);
        }
    }
}
