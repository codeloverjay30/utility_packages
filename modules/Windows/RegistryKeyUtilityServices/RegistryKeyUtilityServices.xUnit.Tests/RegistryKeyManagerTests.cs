using Moq;
using Xunit;
using RegistryKeyUtilityServices;

namespace RegistryKeyUtilityServices.xUnit.Tests
{
    public class RegistryKeyManagerTests
    {
        private readonly Mock<IRegistryWrapper> _mockRegistry;
        private const string UninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

        public RegistryKeyManagerTests()
        {
            // 初始化 Mock 物件
            _mockRegistry = new Mock<IRegistryWrapper>();
        }

# region 測試 GetRegistryKeyName 方法

        [Fact]
        public void GetRegistryKeyName_ShouldReturnCorrectKey_WhenAppExists()
        {
            // Arrange (安排)
            var targetAppName = "MySuperApp";
            var expectedKeyName = "{UNIQUE-GUID-1234}";
            var manager = new RegistryKeyManager(_mockRegistry.Object)
            {
                AppDisplayedName = targetAppName
            };

            // 模擬註冊表中有兩個子機碼
            _mockRegistry.Setup(r => r.GetSubKeyNames(UninstallPath))
                         .Returns(new [ ] { "SomeOtherApp" , expectedKeyName });

            // 模擬第一個機碼不匹配，第二個機碼匹配
            _mockRegistry.Setup(r => r.GetValue($"{UninstallPath}\\SomeOtherApp" , "DisplayName"))
                         .Returns("Unrelated Software");

            _mockRegistry.Setup(r => r.GetValue($"{UninstallPath}\\{expectedKeyName}" , "DisplayName"))
                         .Returns("MySuperApp (Version 1.0)");

            // Act (執行)
            var result = manager.GetRegistryKeyName();

            // Assert (驗證)
            Assert.Equal(expectedKeyName , result);
        }

        [Fact]
        public void GetRegistryKeyName_ShouldReturnNull_WhenAppNotFound()
        {
            // Arrange
            var manager = new RegistryKeyManager(_mockRegistry.Object)
            {
                AppDisplayedName = "MissingApp"
            };

            _mockRegistry.Setup(r => r.GetSubKeyNames(UninstallPath))
                         .Returns(new [ ] { "AppA" , "AppB" });

            _mockRegistry.Setup(r => r.GetValue(It.IsAny<string>() , "DisplayName"))
                         .Returns("Different Name");

            // Act
            var result = manager.GetRegistryKeyName();

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region 測試 GetAppSetting 方法

        [Fact]
        public void GetAppSetting_ShouldReturnValue_WhenKeyAndValueExist()
        {
            // Arrange
            var targetAppName = "TestApp";
            var keyName = "TestApp_Key";
            var valueName = "InstallLocation";
            var expectedValue = @"C:\Program Files\TestApp";

            var manager = new RegistryKeyManager(_mockRegistry.Object)
            {
                AppDisplayedName = targetAppName
            };

            // 1. 先讓 GetRegistryKeyName 成功找到 Key
            _mockRegistry.Setup(r => r.GetSubKeyNames(UninstallPath))
                         .Returns(new [ ] { keyName });
            _mockRegistry.Setup(r => r.GetValue($"{UninstallPath}\\{keyName}" , "DisplayName"))
                         .Returns(targetAppName);

            // 2. 模擬 GetCurrentUserValue 回傳我們想要的設定值
            _mockRegistry.Setup(r => r.GetCurrentUserValue(keyName , valueName))
                         .Returns(expectedValue);

            // Act
            var result = manager.GetAppSetting(valueName);

            // Assert
            Assert.Equal(expectedValue , result);
        }

        [Fact]
        public void GetAppSetting_ShouldReturnNull_WhenKeyDoesNotExist()
        {
            // Arrange
            var manager = new RegistryKeyManager(_mockRegistry.Object)
            {
                AppDisplayedName = "GhostApp"
            };

            // 模擬找不到任何 Key
            _mockRegistry.Setup(r => r.GetSubKeyNames(UninstallPath))
                         .Returns(Enumerable.Empty<string>());

            // Act
            var result = manager.GetAppSetting("AnyValue");

            // Assert
            Assert.Null(result);
        }

        #endregion
    }
}
