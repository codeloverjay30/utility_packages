using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Moq;
using NUnit.Framework;
using SolutionUtilityServices;
using SolutionUtilityServices;

namespace SolutionUtilityServices.Tests
{
    [TestFixture]
    public class SolutionExtractorTests
    {
        private MockFileSystem _fileSystem;
        private SolutionModel _sourceSolution;
        private SolutionModel _targetSolution;

        [SetUp]
        public void Setup()
        {
            Configure();
        }

        private void Configure()
        {
            _fileSystem = new MockFileSystem();

            // 設定模擬來源路徑
            _sourceSolution = new SolutionModel
            {
                SolutionName = "OldSln" ,
                RootPath = @"C:\Source" ,
                Projects = new List<ProjectModel>
                {
                    new ProjectModel { ProjectName = "Proj1", RootPath = @"C:\Source\Proj1", RootNamespace = "OldNS" }
                }
            };

            // 設定模擬目標路徑
            _targetSolution = new SolutionModel
            {
                SolutionName = "NewSln" ,
                RootPath = @"C:\Target" ,
                Projects = new List<ProjectModel>
                {
                    new ProjectModel { ProjectName = "Proj1", RootPath = @"C:\Target\Proj1", RootNamespace = "NewNS" }
                }
            };
        }

        [Test]
        public void ExtractSpecificProjects_ShouldCopyFilesAndReplaceNamespace()
        {
            // Arrange
            var sourceFilePath = @"C:\Source\Proj1\Class1.cs";

            _fileSystem.AddDirectory(@"C:\Source\Proj1\");
            _fileSystem.AddFile(@"C:\Source\Proj1\Proj1.csproj",new MockFileData(""));
            _fileSystem.AddFile(sourceFilePath , new MockFileData("namespace OldNS { }"));


            var extractor = new SolutionExtractor(
                _sourceSolution ,
                _targetSolution ,
                _fileSystem ,
                new FileExtensionChecker(_fileSystem) ,
                new ExcludedEntriesUtilityService()
            );

            // Act
            extractor.ExtractSpecificProjects(new List<ProjectModel> { _sourceSolution.Projects [ 0 ] });

            // Assert
            // 檢查目標資料夾是否建立
            Assert.That(_fileSystem.Directory.Exists(@"C:\Target\Proj1"),Is.True);

            var targetContent = _fileSystem.File.ReadAllText(@"C:\Target\Proj1\Class1.cs");
            Assert.That(targetContent.Contains("namespace NewNS { }"));
        }

        [Test]
        public void ExtractWholeSolution_ShouldGenerateSlnxFile()
        {
            // Arrange
            _fileSystem.AddDirectory(@"C:\Source\Proj1");
            var extractor = new SolutionExtractor(
                _sourceSolution ,
                _targetSolution ,
                _fileSystem ,
                new FileExtensionChecker(_fileSystem) ,
                new ExcludedEntriesUtilityService()
            );

            // Act
            extractor.ExtractWholeSolution();

            // Assert
            string expectedSlnx = @"C:\Target\NewSln.slnx";
            Assert.That(_fileSystem.File.Exists(expectedSlnx),Is.True);

            var slnxContent = _fileSystem.File.ReadAllText(expectedSlnx);
            Assert.That(slnxContent , Contains.Substring("<Solution"));
            Assert.That(slnxContent , Contains.Substring("Proj1.csproj"));
        }
    }
}
