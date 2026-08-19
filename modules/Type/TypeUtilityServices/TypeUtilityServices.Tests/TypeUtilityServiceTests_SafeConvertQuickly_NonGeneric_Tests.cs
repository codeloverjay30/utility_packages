using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TypeUtilityServices.Tests
{
    public partial class TypeUtilityServiceTests
    {
        #region DateTime & TimeSpan Special Cases (針對優化邏輯)

        [Fact]
        public void SafeConvertQuickly_DateTime_RoundTrip_ShouldMaintainValue()
        {
            // Arrange: 測試日期轉字串再轉回日期的回路 (Round-trip)
            DateTime original = new DateTime(2026 , 4 , 1 , 18 , 30 , 0);
            string isoString = original.ToString("o" , CultureInfo.InvariantCulture); // ISO 8601 格式

            // Act
            var result = _service.SafeConvertQuickly<DateTime>(isoString);

            // Assert
            result.Should().Be(original);
        }

        [Fact]
        public void SafeConvertQuickly_TimeSpan_FromMilliseconds_ShouldSucceed()
        {
            // Arrange: 測試數值轉 TimeSpan
            double ms = 5000.5;
            TimeSpan expected = TimeSpan.FromMilliseconds(ms);

            // Act
            var result = _service.SafeConvertQuickly<TimeSpan>(ms);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("00:05:00" , 300000)] // 5分鐘
        public void SafeConvertQuickly_TimeSpan_ParseString_ShouldSucceed(string input , double expectedMs)
        {
            // Act
            var result = _service.SafeConvertQuickly<TimeSpan>(input);

            // Assert
            result.TotalMilliseconds.Should().Be(expectedMs);
        }

        #endregion

        #region Performance Optimization Logic Tests (針對 Pattern Matching)

        [Theory]
        [InlineData(1 , true)]
        [InlineData(0 , false)]
        [InlineData("1" , true)]
        [InlineData("0" , false)]
        [InlineData(1L , true)]
        public void SafeConvertQuickly_BooleanPatternMatching_ShouldWorkCorrectly(object input , bool expected)
        {
            // 測試優化版中的 switch(TypeCode.Boolean) 與模式匹配
            var result = _service.SafeConvertQuickly<bool>(input);
            result.Should().Be(expected);
        }

        [Fact]
        public void SafeConvertQuickly_ShouldUseInstanceOfType_FastPath()
        {
            // Arrange: 測試已經是相同型別時的快速通道 (Fast Path)
            Guid original = Guid.NewGuid();

            // Act
            var result = _service.SafeConvertQuickly<Guid>(original);

            // Assert
            result.Should().Be(original);
        }

        #endregion

        #region Edge Cases (邊際情況)

        [Theory]
        [InlineData("" , typeof(int) , 0)]
        [InlineData("   " , typeof(double) , 0.0)]
        public void SafeConvertQuickly_EmptyOrWhitespaceString_ShouldReturnDefault(string input , Type targetType , object expected)
        {
            // 測試無效字串是否會安全地返回 default 而非拋出異常
            if(targetType == typeof(int))
                _service.SafeConvertQuickly<int>(input).Should().Be((int)expected);
            else if(targetType == typeof(double))
                _service.SafeConvertQuickly<double>(input).Should().Be((double)expected);
        }

        [Fact]
        public void SafeConvertQuickly_InvalidEnumString_ShouldReturnDefault()
        {
            // Arrange
            string invalidEnum = "NotAKind";

            // Act
            var result = _service.SafeConvertQuickly<DateTimeKind>(invalidEnum);

            // Assert
            result.Should().Be(default(DateTimeKind)); // Unspecified
        }

        [Fact]
        public void SafeConvertQuickly_CharConversion_FromSingleLengthString()
        {
            // Arrange
            string input = "A";

            // Act
            var result = _service.SafeConvertQuickly<char>(input);

            // Assert
            result.Should().Be('A');
        }

        #endregion
    }
}
