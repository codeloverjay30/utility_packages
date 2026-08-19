using Moq;
using MathUtilityServices;
using SortingUtilityServices;
using Xunit;

namespace MathUtilityServices.Tests
{
    public class MathUtilityServiceTests
    {
        private readonly Mock<ISortingUtilityService> _sortingMock;
        private readonly IMathUtilityService _service;

        public MathUtilityServiceTests()
        {
            _sortingMock = new Mock<ISortingUtilityService>();
            _service = new MathUtilityService(_sortingMock.Object);
        }

        [Fact]
        public void RangeFrom_ValidRange_ReturnsExpectedSequence()
        {
            // Arrange
            int start = 1;
            int end = 5;
            int step = 1;
            _sortingMock.Setup(s => s.GetSortedPair(start , end)).Returns((start , end));

            // Act
            var result = _service.RangeFrom(start , end , step);

            // Assert
            Assert.Equal(new [ ] { 1 , 2 , 3 , 4 , 5 } , result);
        }

        [Fact]
        public void RangeFrom_WithStep_ReturnsSteppedSequence()
        {
            // Arrange
            double start = 1.0;
            double end = 2.0;
            double step = 0.5;
            _sortingMock.Setup(s => s.GetSortedPair(start , end)).Returns((start , end));

            // Act
            var result = _service.RangeFrom(start , end , step);

            // Assert
            Assert.Equal(new [ ] { 1.0 , 1.5 , 2.0 } , result);
        }

        [Fact]
        public void RangeFrom_UnsortedInputs_CallsSortingUtility()
        {
            // Arrange
            int start = 10;
            int end = 5;
            // 模擬排序服務將其翻轉為 (5, 10)
            _sortingMock.Setup(s => s.GetSortedPair(start , end)).Returns((5 , 10));

            // Act
            var result = _service.RangeFrom(start , end).ToList();

            // Assert
            _sortingMock.Verify(s => s.GetSortedPair(start , end) , Times.Once);
            Assert.Equal(result.First(),5);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void RangeFrom_InvalidStep_ThrowsArgumentOutOfRangeException(int invalidStep)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _service.RangeFrom(1 , 10 , invalidStep).ToList());
            // 注意：由於使用 yield return，需呼叫 ToList() 觸發執行
        }
    }
}
