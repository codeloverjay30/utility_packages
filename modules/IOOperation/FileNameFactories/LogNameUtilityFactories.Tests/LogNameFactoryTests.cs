using Moq;
using Xunit;
using LogNameUtilityFactories;
using FileNameUtilityFactories;
using System.Reflection;

namespace LogNameUtilityFactories.Tests
{
    public class LogNameFactoryTests
    {
        private readonly Mock<IProjectNameFactory> _mockProjectNameFactory;

        public LogNameFactoryTests()
        {
            _mockProjectNameFactory = new Mock<IProjectNameFactory>();
        }

        [Fact]
        public void Create_ShouldReturnCorrectFormat_WhenAssemblyExists()
        {
            // Arrange
            var machineName = Environment.MachineName;
            var shortName = "MyApp";
            var version = "1.0.0";

            _mockProjectNameFactory.Setup(f => f.Assembly).Returns(Assembly.GetExecutingAssembly());
            _mockProjectNameFactory.Setup(f => f.Create()).Returns((shortName , version));

            var factory = new LogNameFactory(_mockProjectNameFactory.Object);

            // Act
            var result = factory.Create();

            // Assert
            // 格式預期為: {MachineName}_{ShortName}_{Version}_{yyyyMMddHHmm}.log
            Assert.Contains($"{machineName}_{shortName}_{version}_" , result);
            Assert.EndsWith(".log" , result);
        }

        [Fact]
        public void Create_ShouldReturnUnknownAssembly_WhenAssemblyIsNull()
        {
            // Arrange
            _mockProjectNameFactory.Setup(f => f.Assembly).Returns((Assembly?)null);
            var factory = new LogNameFactory(_mockProjectNameFactory.Object);

            // Act
            var result = factory.Create();

            // Assert
            Assert.Contains("UnknownAssembly_" , result);
        }
    }
}
