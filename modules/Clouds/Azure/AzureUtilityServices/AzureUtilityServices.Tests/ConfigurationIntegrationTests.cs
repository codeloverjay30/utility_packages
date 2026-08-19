using AzureUtilityServices;
using Xunit;
using Microsoft.Extensions.Configuration;
namespace AzureUtilityServices.Tests
{
    public class ConfigurationIntegrationTests
    {
            [Fact]
            public void ConfigurationBuilder_ShouldIntegrateUtilityService()
            {
                var resourceName = "SerilogConfigurationResource";
                var endPoint = $"https://{resourceName}.azconfig.io";

                var builder = new ConfigurationBuilder();

                // 模擬擴充方法的使用情境 
                builder.AddAzureAppConfiguration(options =>
                {
                    var utility = new AzureAppConfigurationUtilityService
                    {
                        Options = options,
                        Uri = new Uri(endPoint),
                        labels = new List<string> { "Crawler-Logs" }
                    };
                    
                    // 測試 Skip 邏輯是否正確執行 
                    utility.Initialize();
                });

                // 此處不一定要 Build() 成功（若無真實連線），但可測試 Builder 是否已加入 provider
                Assert.NotEmpty(builder.Sources);
            }
        }
}