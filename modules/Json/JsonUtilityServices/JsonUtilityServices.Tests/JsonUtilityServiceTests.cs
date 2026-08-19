using Xunit;
using JsonUtilityServices;
using System;
using TypeUtilityServices;

namespace JsonUtilityServices.Tests
{
    public class JsonUtilityServiceTests
    {
        private readonly JsonUtilityService _service;
        private readonly TypeUtilityService _typeUtilityService;

        public JsonUtilityServiceTests()
        {
            // 初始化受測對象
            _typeUtilityService = new TypeUtilityService();
            _service = new JsonUtilityService(_typeUtilityService);
            
        }

        [Theory]
        [InlineData(typeof(int) , "number")]
        [InlineData(typeof(long) , "number")]
        [InlineData(typeof(double) , "number")]
        [InlineData(typeof(float) , "number")]
        [InlineData(typeof(bool) , "boolean")]
        [InlineData(typeof(string) , "string")]
        [InlineData(typeof(DateTime) , "other")] // 測試預設回傳值
        [InlineData(typeof(object) , "other")]   // 測試未知型別
        public void GetJsonType_ShouldReturnCorrectJsonType(Type inputType , string expected)
        {
            // Act
            string result = _service.GetJsonType(inputType);

            // Assert
            Assert.Equal(expected , result);
        }
    }
}
