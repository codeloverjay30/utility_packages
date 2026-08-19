using Xunit;
using Moq;
using System.IO.Abstractions.TestingHelpers;
using SolutionUtilityServices;
using CommonModels;

namespace ProjectUpgrader.Tests
{
    public class CSharpProjectUpdaterTests
    {
        private readonly Mock<IProjectFileService> _mockProjectService;
        private readonly Mock<INetSdkInfo> _mockNetSdkInfo;
        private readonly Mock<ICommandRunner> _mockCommandRunner;
        private readonly MockFileSystem _mockFileSystem;
        private readonly string _solutionPath = @"C:\TestSolution";

        public CSharpProjectUpdaterTests()
        {
            _mockProjectService = new Mock<IProjectFileService>();
            _mockNetSdkInfo = new Mock<INetSdkInfo>();
            _mockCommandRunner = new Mock<ICommandRunner>();
            _mockFileSystem = new MockFileSystem();

            // Mock a project file in the virtual file system
            _mockFileSystem.AddFile($@"{_solutionPath}\Project1.csproj", new MockFileData("<Project></Project>"));
        }

        [Fact]
        public async Task FullUpgradeAsync_ShouldExecuteAllStepsSuccessfully()
        {
            // Arrange
            _mockNetSdkInfo.Setup(x => x.GetInstalledLatestVersion()).Returns("8.0");
            
            _mockProjectService.Setup(x => x.GetPackageReferences(It.IsAny<string>()))
                .Returns(new List<PackageReference> { new("Newtonsoft.Json", "12.0.1") });

            _mockProjectService.Setup(x => x.GetLatestPackageUpdatesAsync(It.IsAny<IEnumerable<PackageReference>>()))
                .ReturnsAsync(new List<PackageReference> { new("Newtonsoft.Json", "13.0.3") });

            var updater = new CSharpProjectUpdater(
                _solutionPath,
                _mockNetSdkInfo.Object,
                _mockFileSystem,
                _mockCommandRunner.Object,
                _mockProjectService.Object
            );

            // Act
            var result = await updater.FullUpgradeAsync();

            // Assert
            _mockProjectService.Verify(x => x.SetTargetFramework(It.IsAny<string>(), "net8.0"), Times.AtLeastOnce);
            _mockProjectService.Verify(x => x.UpdatePackageVersions(It.IsAny<string>(), It.IsAny<IEnumerable<PackageReference>>()), Times.AtLeastOnce);
            _mockCommandRunner.Verify(x => x.RunCommand("dotnet", "restore", _solutionPath), Times.Once);

            Assert.True(result.StatusList.All(s => s.IsSuccess));
        }
    }
}