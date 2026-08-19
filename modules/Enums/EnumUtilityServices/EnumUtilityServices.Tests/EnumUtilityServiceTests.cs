using System;
using Xunit;
using FluentAssertions;
using EnumUtilityServices;

namespace EnumUtilityServices.Tests
{
    public class EnumUtilityServiceTests
    {
        private readonly EnumUtilityService _service;

        public EnumUtilityServiceTests()
        {
            _service = new EnumUtilityService();
        }

        // 定義測試用的 Enum
        private enum TestColor { Red, Green, Blue }

        [Fact]
        public void GetEnumNames_WithStandardEnum_ShouldReturnCorrectNames()
        {
            // Arrange
            var type = typeof(TestColor);

            // Act
            var result = _service.GetEnumNames(type);

            // Assert
            result.Should().BeEquivalentTo("Red" , "Green" , "Blue");
        }

        [Fact]
        public void GetEnumNames_WithNullableEnum_ShouldReturnCorrectNames()
        {
            // Arrange
            var type = typeof(TestColor?);

            // Act
            var result = _service.GetEnumNames(type);

            // Assert
            result.Should().BeEquivalentTo("Red" , "Green" , "Blue");
        }

        [Fact]
        public void GetEnumNames_WithNonEnumType_ShouldReturnEmptyArray()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = _service.GetEnumNames(type);

            // Assert
            result.Should().BeEmpty();
            result.Should().NotBeNull();
        }
    }
}
