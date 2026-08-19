using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace TypeUtilityServices.Tests
{
    public partial class TypeUtilityServiceTests
    {
        #region IsNullableType Tests

        [Theory]
        [InlineData(typeof(int?))]
        [InlineData(typeof(uint?))]
        [InlineData(typeof(long?))]
        [InlineData(typeof(ulong?))]
        [InlineData(typeof(short?))]
        [InlineData(typeof(ushort?))]
        [InlineData(typeof(byte?))]
        [InlineData(typeof(float?))]
        [InlineData(typeof(double?))]
        [InlineData(typeof(decimal?))]
        [InlineData(typeof(bool?))]
        [InlineData(typeof(DateTime?))]
        [InlineData(typeof(DateOnly?))]
        [InlineData(typeof(DateTimeKind?))]
        [InlineData(typeof(DateTimeOffset?))]
        public void IsNullableType_ShouldReturnTrue_ForNullableValueTypes(Type type)
        {
            // Act
            var result = _service.IsNullableType(type);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        [InlineData(typeof(string [ ]))]

        public void IsNullableType_ShouldReturnTrue_ForReferenceTypes(Type type)
        {
            // Act
            var result = _service.IsNullableType(type);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(long))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(short))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateOnly))]
        [InlineData(typeof(DateTimeKind))]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(TimeOnly))]
        [InlineData(typeof(TimeSpan))]
        [InlineData(typeof(Guid))]
        public void IsNullableType_ShouldReturnFalse_ForNonNullableValueTypes(Type type)
        {
            // Act
            var result = _service.IsNullableType(type);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
