using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Microsoft.Build.Locator;
using Moq;
using SolutionUtilityServices;

namespace ProjectUpgrader.Tests
{
    public class CSharpProjectUpdaterTests_old
    {
        private readonly Mock<INetSdkInfo> _mockNetSdkInfo;
        private readonly Mock<ICommandRunner> _mockCommandRunner;
        private readonly Mock<IProjectFileService> _mockProjectService;
        private readonly MockFileSystem _mockFileSystem;
        private const string SolutionPath = @"C:\src\MySolution";

        public CSharpProjectUpdaterTests_old()
        {
            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }
            
            _mockNetSdkInfo = new Mock<INetSdkInfo>();
            _mockCommandRunner = new Mock<ICommandRunner>();
            _mockProjectService = new Mock<IProjectFileService>();
            _mockFileSystem = new MockFileSystem();
        }

        [Fact]
        public void GetLatestSdkVersion_ShouldReturnVersionFromNetSdkInfo()
        {
            _mockNetSdkInfo.Setup(x => x.GetInstalledLatestVersion()).Returns("8.0");
            
            var updater = new CSharpProjectUpdater(
                SolutionPath, 
                _mockNetSdkInfo.Object, 
                _mockFileSystem, 
                _mockCommandRunner.Object,
                _mockProjectService.Object
            );

            var version = updater.GetLatestSdkVersion();

            version.Should().Be("8.0");
        }

        [Fact]
        public async Task FullUpgradeAsync_ShouldExecuteSteps_WithMockedService()
        {
            // Arrange
            _mockNetSdkInfo.Setup(x => x.GetInstalledLatestVersion()).Returns("8.0");
            var csprojPath = Path.Combine(SolutionPath, "App.csproj");
            _mockFileSystem.AddFile(csprojPath, new MockFileData("<Project></Project>"));

            var updater = new CSharpProjectUpdater(
                SolutionPath, 
                _mockNetSdkInfo.Object, 
                _mockFileSystem, 
                _mockCommandRunner.Object, 
                _mockProjectService.Object);

            // Act
            var results = await updater.FullUpgradeAsync();

            // Assert
            results.StatusList.Should().NotBeEmpty();
            _mockCommandRunner.Verify(x => x.RunCommand("dotnet", "restore", SolutionPath), Times.Once);
        }
    }
}