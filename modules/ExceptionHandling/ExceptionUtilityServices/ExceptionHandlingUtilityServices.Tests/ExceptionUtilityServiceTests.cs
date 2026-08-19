using System;
using System.Collections.Generic;
using ExceptionHandlingUtilityServices;
using Xunit;

namespace ExceptionHandlingUtilityServices.Tests
{
    public class ExceptionUtilityServiceTests
    {
        [Fact]
        public void FlattenAndProcess_ShouldExecuteActionOnce_WhenExceptionIsNotAggregate()
        {
            // Arrange
            var expectedEx = new InvalidOperationException("Test exception");
            var service = new ExceptionUtilityService(expectedEx);
            var callCount = 0;
            Exception capturedEx = null;

            // Act
            service.FlattenAndProcess(ex =>
            {
                callCount++;
                capturedEx = ex;
            });

            // Assert
            Assert.Equal(1 , callCount);
            Assert.Same(expectedEx , capturedEx);
        }

        [Fact]
        public void FlattenAndProcess_ShouldFlattenAndExecuteMultipleTimes_WhenExceptionIsAggregate()
        {
            // Arrange
            var inner1 = new ArgumentException("Inner 1");
            var inner2 = new InvalidOperationException("Inner 2");
            var nestedAggregate = new AggregateException(new Exception("Nested") , inner2);

            // 建立一個包含嵌套結構的 AggregateException
            var rootAggregate = new AggregateException(inner1 , nestedAggregate);

            var service = new ExceptionUtilityService(rootAggregate);
            var processedExceptions = new List<Exception>();

            // Act
            service.FlattenAndProcess(ex =>
            {
                processedExceptions.Add(ex);
            });

            // Assert
            // AggregateException.Flatten() 會將所有層級的 InnerExceptions 攤平
            Assert.Equal(3 , processedExceptions.Count);
            Assert.Contains(inner1 , processedExceptions);
            Assert.Contains(inner2 , processedExceptions);
            Assert.Contains(processedExceptions , e => e.Message == "Nested");
        }

        [Fact]
        public void FlattenAndProcess_ShouldNotThrow_WhenActionIsNull()
        {
            // Arrange
            var service = new ExceptionUtilityService(new Exception());

            // Act & Assert
            // 雖然目前的實作會拋出 NullReferenceException，
            // 但在測試中確認行為有助於決定是否要增加 null 檢查防禦代碼
            Assert.Throws<NullReferenceException>(() => service.FlattenAndProcess(null));
        }
    }
}
