using Xunit;
using AzureUtilityServices;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Moq;

namespace AzureUtilityServices.Tests
{
    public class AzureUtilityServiceTests
    {
        [Fact]
        public void Initialize_WithValidParams_ShouldNotThrow()
        {
            var resourceName = "SerilogConfigurationResource";
            var endPoint = $"https://{resourceName}.azconfig.io";
            // Arrange: 準備必要的參數
            var mockOptions = new Mock<AzureAppConfigurationOptions>();
            var service = new AzureAppConfigurationUtilityService
            {
                Options = new AzureAppConfigurationOptions(), // 使用真實物件，因為 Options 是密封類別擴充
                Uri = new Uri(endPoint),
                labels = new[] { "Crawler", "Dev" },
                RefreshSetup = r => r.Register("Sentinel", refreshAll: true)
            };

            // Act & Assert: 驗證初始化流程是否能跑完 
            var exception = Record.Exception(() => service.Initialize());
            
            Assert.Null(exception);
            Assert.NotNull(service.Credential); // 驗證預設認證物件已建立 
        }
    }
}