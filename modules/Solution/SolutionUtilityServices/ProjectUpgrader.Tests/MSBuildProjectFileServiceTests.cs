using Xunit;
using Moq;
using SolutionUtilityServices;
using System.IO;

namespace ProjectUpgrader.Tests
{
    public class MSBuildProjectFileServiceTests : IDisposable
    {
        private readonly string _tempCsproj;
        private readonly MSBuildProjectFileService _service;
        private readonly Mock<INugetService> _mockNugetService;

        public MSBuildProjectFileServiceTests()
        {
            _mockNugetService = new Mock<INugetService>();
            // Inject the mocked NuGet service
            _service = new MSBuildProjectFileService(_mockNugetService.Object);
            
            _tempCsproj = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csproj");
            
            string content = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Newtonsoft.Json"" Version=""12.0.1"" />
  </ItemGroup>
</Project>";
            File.WriteAllText(_tempCsproj, content);
        }

        [Fact]
        public void SetTargetFramework_ShouldUpdateProperty()
        {
            _service.SetTargetFramework(_tempCsproj, "net8.0");

            string content = File.ReadAllText(_tempCsproj);
            Assert.Contains("<TargetFramework>net8.0</TargetFramework>", content);
        }

        [Fact]
        public void GetPackageReferences_ShouldReturnCorrectItems()
        {
            var packages = _service.GetPackageReferences(_tempCsproj).ToList();

            Assert.Single(packages);
            Assert.Equal("Newtonsoft.Json", packages[0].Name);
            Assert.Equal("12.0.1", packages[0].Version);
        }

        [Fact]
        public async Task GetLatestPackageUpdatesAsync_ShouldReturnUpdates_WhenNewerVersionExists()
        {
            // Arrange
            var currentPackages = new List<PackageReference> { new("Newtonsoft.Json", "12.0.1") };
            _mockNugetService.Setup(x => x.GetLatestStableVersionAsync("Newtonsoft.Json", It.IsAny<CancellationToken>()))
                             .ReturnsAsync("13.0.3");

            // Act
            var updates = (await _service.GetLatestPackageUpdatesAsync(currentPackages)).ToList();

            // Assert
            Assert.Single(updates);
            Assert.Equal("13.0.3", updates[0].Version);
        }

        public void Dispose()
        {
            if (File.Exists(_tempCsproj))
            {
                File.Delete(_tempCsproj);
            }
        }
    }
}