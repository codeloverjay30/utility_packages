using Moq;
using StringUtilityServices;
using SortingUtilityServices;
using Xunit;

namespace StringUtilityServices.Tests
{
    public class StringUtilityServiceTests
    {
        private readonly Mock<ISortingUtilityService> _sortingMock;
        private readonly StringUtilityService _service;

        public StringUtilityServiceTests()
        {
            _sortingMock = new Mock<ISortingUtilityService>();
            _service = new StringUtilityService(_sortingMock.Object);
        }

        [Fact]
        public void RangeFrom_ShouldReturnCorrectSequence_WhenInputsAreInOrder()
        {
            // Arrange
            char start = 'a';
            char end = 'c';
            // Setup mock to return the same order
            _sortingMock.Setup(s => s.GetSortedPair(start , end))
                        .Returns((start , end));

            // Act
            var result = _service.RangeFrom(start , end).ToList();

            // Assert
            Assert.Equal(new List<char> { 'a' , 'b' , 'c' } , result);
            _sortingMock.Verify(s => s.GetSortedPair(start , end) , Times.Once);
        }

        [Fact]
        public void RangeFrom_ShouldReturnCorrectSequence_WhenInputsNeedSorting()
        {
            // Arrange
            char start = 'z';
            char end = 'x';
            // Setup mock to return them flipped (sorted)
            _sortingMock.Setup(s => s.GetSortedPair(start , end))
                        .Returns(('x' , 'z'));

            // Act
            var result = _service.RangeFrom(start , end).ToList();

            // Assert
            Assert.Equal(new List<char> { 'x' , 'y' , 'z' } , result);
        }

        [Fact]
        public void RangeFrom_ShouldReturnSingleChar_WhenStartAndEndAreEqual()
        {
            // Arrange
            char val = 'm';
            _sortingMock.Setup(s => s.GetSortedPair(val , val))
                        .Returns((val , val));

            // Act
            var result = _service.RangeFrom(val , val).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal('m' , result [ 0 ]);
        }
    }
}
