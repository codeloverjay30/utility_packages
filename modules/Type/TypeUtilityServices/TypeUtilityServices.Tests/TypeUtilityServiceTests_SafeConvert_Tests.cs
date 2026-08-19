using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace TypeUtilityServices.Tests
{
    public partial class TypeUtilityServiceTests
    {
        #region SafeConvert Tests
#if NETCOREAPP3_0_OR_GREATER
        [Fact]
        public void SafeConvert_ShouldReturnDefault_WhenValueIsNull()
        {
            // Act
            var result = _service.SafeConvert<int?>(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void SafeConvert_ShouldReturnDefault_WhenValueIsDBNull()
        {
            // Act
            var result = _service.SafeConvert<int>(DBNull.Value);

            // Assert
            Assert.Equal(0 , result);
        }

        [Fact]
        public void SafeConvert_ShouldConvertSuccessfully_WhenValidValueProvided()
        {
            // Act
            var intResult = _service.SafeConvert<int>("123");
            var doubleResult = _service.SafeConvert<double>(123.45);
            var nullableIntResult = _service.SafeConvert<int?>("456");

            // Assert
            Assert.Equal(123 , intResult);
            Assert.Equal(123.45 , doubleResult);
            Assert.Equal(456 , nullableIntResult);
        }

        [Fact]
        public void SafeConvert_ShouldReturnDefault_WhenConversionFails()
        {
            // Act
            var result = _service.SafeConvert<int>("NotANumber");

            // Assert
            Assert.Equal(0 , result);
        }

        [Fact]
        public void SafeConvert_ShouldHandleDateTime_Successfully()
        {
            // Arrange
            var dateStr = "2024-01-01";

            // Act
            var result = _service.SafeConvert<DateTime>(dateStr);

            // Assert
            Assert.Equal(new DateTime(2024 , 1 , 1) , result);
        }
#endif

        #endregion

        #region SafeConvert Extended Tests
#if NETCOREAPP3_0_OR_GREATER

        [Theory]
        [InlineData("1" , true)]
        [InlineData("0" , false)]
        [InlineData("true" , true)]
        [InlineData("False" , false)]
        public void SafeConvert_ShouldHandleBooleanConversion_Successfully(object value , bool expected)
        {
            // Act
            var result = _service.SafeConvert<bool>(value);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void SafeConvert_ShouldHandleEnumConversion_FromValidStringAndInt()
        {
            // Arrange
            var stringValue = "Utc";
            var intValue = 1; // DateTimeKind.Utc 的值通常是 1

            // Act
            var resultFromString = _service.SafeConvert<DateTimeKind>(stringValue);
            var resultFromInt = _service.SafeConvert<DateTimeKind>(intValue);

            // Assert
            resultFromString.Should().Be(DateTimeKind.Utc);
            resultFromInt.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void SafeConvert_ShouldReturnDefault_WhenNumericOverflowOccurs()
        {
            // Arrange
            // long.MaxValue 顯然超過了 int 的範圍
            object largeValue = long.MaxValue;

            // Act
            var result = _service.SafeConvert<int>(largeValue);

            // Assert
            result.Should().Be(default);
        }

        [Theory]
        [InlineData(123.45 , 123)] // 浮點數轉整數（會捨去小數）
        [InlineData("123.45" , 123.45)] // 字串轉 Double
        public void SafeConvert_ShouldHandleNumericPrecision_Correctly(object input , object expected)
        {
            if(expected is int expectedInt)
            {
                var result = _service.SafeConvert<int>(input);
                result.Should().Be(expectedInt);
            }
            else if(expected is double expectedDouble)
            {
                var result = _service.SafeConvert<double>(input);
                result.Should().Be(expectedDouble);
            }
        }

        [Fact]
        public void SafeConvert_ShouldHandleGuidConversion_Successfully()
        {
            // Arrange
            var guidStr = "74996917-0932-4e94-9549-014902148496";
            var expectedGuid = new Guid(guidStr);

            // Act
            var result = _service.SafeConvert<Guid>(guidStr);

            // Assert
            result.Should().Be(expectedGuid);
        }

        [Fact]
        public void SafeConvert_ShouldReturnDefault_WhenTargetTypeIsIncompatible()
        {
            // Arrange
            var complexObject = new SampleClass();

            // Act
            // 嘗試將自定義類別物件轉成 int，Convert.ChangeType 通常會失敗
            var result = _service.SafeConvert<int>(complexObject);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void SafeConvert_ShouldHandleNullableEnum_WithNullValue()
        {
            // Act
            var result = _service.SafeConvert<DateTimeKind?>(null);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("" , typeof(int) , 0)]
        [InlineData("   " , typeof(double) , 0.0)]
        [InlineData("" , typeof(Guid) , "00000000-0000-0000-0000-000000000000")]
        public void SafeConvert_ShouldReturnDefault_WhenStringIsInvalidOrEmpty(string input , Type targetType , object expectedDefault)
        {
            // 這裡可以使用反射來呼叫泛型方法，或直接寫多個 Fact
            var result = _service.SafeConvert<int>(input);
            result.Should().Be(0);
        }

#endif
        #endregion

        #region Specialized Conversion Tests
#if NETCOREAPP3_0_OR_GREATER

        [Fact]
        public void SafeConvert_ShouldHandleDateTimeToStringAndBack_Successfully()
        {
            // Arrange
            DateTime originalDate = DateTime.Now;
            // 轉成字串，通常這是從資料庫或 API 拿到的格式
            string dateString = originalDate.ToString("o"); // 使用 Round-trip 格式確保精度

            // Act
            var convertedDate = _service.SafeConvert<DateTime>(dateString);

            // Assert
            // 檢查數值是否一致（精確到毫秒，避免 tick 差異）
            convertedDate.Should().BeCloseTo(originalDate , precision: TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public void SafeConvert_ShouldHandleTimeSpanToIntAndBack_BehaviorCheck()
        {
            // Arrange
            TimeSpan originalSpan = TimeSpan.FromMinutes(5);

            // 1. 第一步：TimeSpan 轉 int (這在 C# 中沒有直接轉換邏輯)
            // Convert.ChangeType(TimeSpan, typeof(int)) 會拋出 InvalidCastException
            var intResult = _service.SafeConvert<int>(originalSpan);

            // 2. 第二步：將剛才得到的結果轉回 TimeSpan
            var finalSpan = _service.SafeConvert<TimeSpan>(intResult);

            // Assert
            // 根據你的實作，TimeSpan -> int 會失敗並回傳 0
            intResult.Should().Be(0);
            // 0 轉回 TimeSpan 則是 TimeSpan.Zero
            finalSpan.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void SafeConvert_ShouldHandleTimeSpanToDouble_Successfully()
        {
            // 補充：如果你是想測試數值化，通常會轉成 TotalMilliseconds
            // Arrange
            TimeSpan originalSpan = TimeSpan.FromSeconds(30);
            double totalMs = originalSpan.TotalMilliseconds;

            // Act
            // 注意：Convert.ChangeType 不支援 Double -> TimeSpan，這通常需要自定義邏輯
            var result = _service.SafeConvert<TimeSpan>(totalMs);

            // Assert
            // 因為 SafeConvert 目前內部是用 Convert.ChangeType，
            result.Should().Be(originalSpan);
        }

        [Fact]
        public void SafeConvert_DateTime_RoundTrip_ShouldMaintainValue()
        {
            // Arrange
            DateTime original = new DateTime(2026 , 4 , 1 , 18 , 30 , 0);
            string isoString = original.ToString("o"); // Round-trip 格式: 2026-04-01T18:30:00.0000000

            // Act
            var converted = _service.SafeConvert<DateTime>(isoString);

            // Assert
            converted.Should().Be(original);
        }

        [Theory]
        [InlineData("2026-04-01")]
        [InlineData("2026/04/01 18:30:00")]
        [InlineData("Wed, 01 Apr 2026 18:30:00 GMT")] // RFC1123
        public void SafeConvert_DateTime_VariousFormats_ShouldSucceed(string dateStr)
        {
            // Act
            var result = _service.SafeConvert<DateTime>(dateStr);

            // Assert
            result.Should().NotBe(default(DateTime));
            result.Year.Should().Be(2026);
        }

        [Fact]
        public void SafeConvert_DateTime_InvalidString_ShouldReturnDefault()
        {
            // Arrange
            string invalidDate = "NotADateString";

            // Act
            var result = _service.SafeConvert<DateTime>(invalidDate);

            // Assert
            result.Should().Be(default(DateTime));
        }

#endif
        #endregion
    }
}
