using Xunit;
using System.ComponentModel.DataAnnotations;
using NetRuntimeUtilityServices;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace NetRuntimeUtilityServices.Tests
{
    public class RequiresRuntimeTests
    {
        [Fact]
        public void VersionCheck_ShouldFail_IfVersionTooHighAsync()
        {
            // Arrange: 模擬一個未來版本 (99.0)，目前環境肯定不符合
            var model = new TestModel();
            // 注意：測試時可能需要另建一個測試 DTO 或動態檢查 Attribute
            var attribute = new RequiresRuntimeAttribute(99 , 0 , "WINDOWS" , "LINUX" , "OSX");

            // Act
            var result = attribute.GetValidationResult("TestValue" , new ValidationContext(new object()));

            // Assert
            Assert.NotEqual(ValidationResult.Success , result);
            Assert.Contains("less than required version" , result?.ErrorMessage);
        }

        [Fact]
        public void IsValid_ShouldFail_WhenSimulatingLinuxOnWindows()
        {
            // Arrange: 模擬一個目前是 Linux 的環境
            var mockProvider = new Mock<IRuntimeEnvironmentProvider>();
            mockProvider.Setup(p => p.IsOSPlatform("LINUX")).Returns(true);
            mockProvider.Setup(p => p.IsOSPlatform("WINDOWS")).Returns(false);
            mockProvider.Setup(p => p.GetVersion()).Returns(new Version(8 , 0));
            mockProvider.Setup(p => p.GetOSDescription()).Returns("Simulated Linux Kernel");

            // 要求必須在 Windows 運行的 Attribute
            var attribute = new RequiresRuntimeAttribute(8 , 0 , "WINDOWS");

            // 將 Mock 注入 ValidationContext
            var services = new ServiceCollection();
            services.AddSingleton(mockProvider.Object);
            var serviceProvider = services.BuildServiceProvider();

            var context = new ValidationContext(new object() , serviceProvider , null);

            // Act
            var result = attribute.GetValidationResult("TestValue" , context);

            // Assert
            Assert.NotEqual(ValidationResult.Success , result);
            Assert.Contains("Simulated Linux Kernel" , result.ErrorMessage);
        }
    }
}
