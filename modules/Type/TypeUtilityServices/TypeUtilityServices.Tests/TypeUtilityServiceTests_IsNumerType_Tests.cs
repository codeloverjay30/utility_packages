using System;
using Xunit;
using TypeUtilityServices;
using FluentAssertions;

namespace TypeUtilityServices.Tests
{
    public partial class TypeUtilityServiceTests
    {
        private readonly TypeUtilityService _service;

        public TypeUtilityServiceTests()
        {
            _service = new TypeUtilityService();
        }

        #region IsNumericType Tests

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
        public void IsNumericType_ShouldReturnTrue_ForNumericTypes(Type type)
        {
            // Act
            var result = _service.IsNumericType(type);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Guid))]
        public void IsNumericType_ShouldReturnFalse_ForNonNumericTypes(Type type)
        {
            // Act
            var result = _service.IsNumericType(type);

            // Assert
            Assert.False(result);
        }

        #endregion



    }
        // 用於測試的輔助類別
    public class SampleClass { }
}
