using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;
using AssemblyUtilityServices;

namespace AssemblyUtilityServices.Tests
{
    public class AssembliesUtilityServiceTests
    {
        private readonly string _testPath = Path.Combine(Path.GetTempPath() , "TestAssemblies");
        private readonly string _globFilter = "*.dll";

        public AssembliesUtilityServiceTests()
        {
            // 確保測試目錄存在
            if(!Directory.Exists(_testPath))
            {
                Directory.CreateDirectory(_testPath);
            }
        }

        [Fact]
        public void ListAllAssemblies_ShouldReturnFiles_WhenFilesExist()
        {
            // Arrange
            var testFile = Path.Combine(_testPath , "TestAssembly.dll");
            File.WriteAllText(testFile , "fake dll content");
            var service = new AssembliesUtilityService(_testPath , _globFilter);

            // Act
            var result = service.ListAllAssemblies();

            // Assert
            Assert.Contains(testFile , result);

            // Cleanup
            if(File.Exists(testFile)) File.Delete(testFile);
        }

        [Fact]
        public void LoadAllAssemblies_ShouldThrowException_WithInvalidPaths()
        {
            // Arrange
            var service = new AssembliesUtilityService(_testPath , _globFilter);
            var invalidPaths = new List<string> { "NonExistent.dll" };

            // Act & Assert
            // 因為 Assembly.Load 找不到檔案會拋出異常
            Assert.Throws<FileNotFoundException>(() => service.LoadAllAssemblies(invalidPaths));
        }
    }
}
