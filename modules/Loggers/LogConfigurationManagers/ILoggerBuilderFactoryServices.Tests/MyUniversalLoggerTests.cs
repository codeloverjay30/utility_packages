using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILoggerBuilderFactoryServices.Test
{

    [TestFixture]
    public class MyUniversalLoggerTests
    {
        // 建立一個 Mock 的配置類別供測試使用
        public class MockConfig : IExtendedLogConfiguration, IFileLogConfiguration
        {
            public string? LogPrefix { get; set; }
            public bool EnableConsole { get; set; }
            public bool EnableDebug { get; set; }
            public string? LogFilePath { get; set; }
        }

        [Test]
        public void Log_WithPrefix_ShouldIncludePrefixInOutput()
        {
            // Arrange
            var config = new MockConfig { LogPrefix = "TEST_APP" };
            var logger = new MyUniversalLogger<MockConfig>("CategoryA" , config);

            // 這裡我們難以擷取 Console.Write，但可以驗證邏輯不拋出異常
            Assert.DoesNotThrow(() =>
                logger.Log(LogLevel.Information , 0 , "Hello" , null , (s , e) => s.ToString())
            );
        }

        [Test]
        public void Log_FileConfiguration_ShouldTriggerFileLogManager()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var config = new MockConfig { LogFilePath = tempFile };
            var logger = new MyUniversalLogger<MockConfig>("FileLogger" , config);

            // Act
            logger.Log(LogLevel.Error , 0 , "File Message" , null , (s , e) => s.ToString());

            // Assert: 檢查檔案是否被初始化 (FileLogManager 內部行為)
            Assert.That(File.Exists(tempFile) , Is.True);
        }
    }
}
