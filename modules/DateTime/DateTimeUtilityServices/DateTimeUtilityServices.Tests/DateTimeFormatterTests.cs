using System;
using Xunit;
using DateTimeUtilityServices;

namespace DateTimeUtilityServices.Tests
{
    public class DateTimeFormatterTests
    {
        [Fact]
        public void ToStringWithTaiwanCalender_CurrentEra_ReturnsCorrectString()
        {
            // Arrange: 設定一個西元 2024 年的日期
            DateTime testDate = new DateTime(2024 , 5 , 20);
            string expected = "113年5月20日";

            // Act: 執行擴充方法
            string result = testDate.ToStringWithTaiwanCalender();

            // Assert: 驗證結果
            Assert.Equal(expected , result);
        }

        [Theory]
        [InlineData(2011 , 1 , 1 , "100年1月1日")]
        [InlineData(1912 , 1 , 1 , "1年1月1日")]
        [InlineData(2026 , 3 , 12 , "115年3月12日")]
        public void ToStringWithTawainCalender_VariousDates_ReturnsExpectedResults(int year , int month , int day , string expected)
        {
            // Arrange
            DateTime testDate = new DateTime(year , month , day);

            // Act
            string result = testDate.ToStringWithTaiwanCalender();

            // Assert
            Assert.Equal(expected , result);
        }

        [Fact]
        public void ToStringWithTawainCalender_LeapYear_ReturnsCorrectString()
        {
            // Arrange: 2024 是閏年，2月有29號
            DateTime leapDay = new DateTime(2024 , 2 , 29);
            string expected = "113年2月29日";

            // Act
            string result = leapDay.ToStringWithTaiwanCalender();

            // Assert
            Assert.Equal(expected , result);
        }
    }
}
