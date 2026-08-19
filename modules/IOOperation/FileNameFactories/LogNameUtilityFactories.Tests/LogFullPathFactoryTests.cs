using Moq;
using Xunit;
using LogNameUtilityFactories;

namespace LogNameUtilityFactories.Tests
{
    public class LogFullPathFactoryTests : IDisposable
    {
        private readonly Mock<ILogNameFactory> _mockLogNameFactory;
        private readonly string _tempPath;

        public LogFullPathFactoryTests()
        {
            _mockLogNameFactory = new Mock<ILogNameFactory>();
            // 使用臨時目錄進行測試，避免污染實際環境
            _tempPath = Path.Combine(Path.GetTempPath() , "LogTests_" + Guid.NewGuid().ToString());
        }

        [Fact]
        public void Constructor_ShouldCreateDirectory_IfNotExist()
        {
            // Act
            var factory = new LogFullPathFactory(_tempPath , _mockLogNameFactory.Object);

            // Assert
            Assert.True(Directory.Exists(_tempPath));
        }

        [Fact]
        public void Create_ShouldReturnCombinedPath()
        {
            // Arrange
            var fileName = "test_log_2024.log";
            _mockLogNameFactory.Setup(f => f.Create()).Returns(fileName);
            var factory = new LogFullPathFactory(_tempPath , _mockLogNameFactory.Object);

            // Act
            var result = factory.Create();

            // Assert
            var expectedPath = Path.Combine(_tempPath , fileName);
            Assert.Equal(expectedPath , result);
        }

        [Theory]
        [InlineData(null)]
        public void Constructor_ShouldThrowException_WhenBaseDirectoryIsNull(string path)
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new LogFullPathFactory(path! , _mockLogNameFactory.Object));
        }

        [Theory]
        [InlineData("")]
        public void Constructor_ShouldThrowException_WhenBaseDirectoryIsEmpyString(string path)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new LogFullPathFactory(path! , _mockLogNameFactory.Object));
        }

        public void Dispose()
        {
            if(Directory.Exists(_tempPath))
            {
                Directory.Delete(_tempPath , true);
            }
        }
    }
}
