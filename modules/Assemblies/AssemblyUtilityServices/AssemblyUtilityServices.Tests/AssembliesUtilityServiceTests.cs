using FluentAssertions;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using Xunit;

namespace AssemblyUtilityServices.Tests;

public sealed class AssembliesUtilityServiceTests
{
    [Fact]
    public void ListAllAssemblies_WhenFilesExist_ShouldReturnOnlyMatchingFilesInDeterministicOrder()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        string root = fileSystem.Path.GetFullPath("solution");
        string firstAssembly = fileSystem.Path.Combine(root, "a.dll");
        string secondAssembly = fileSystem.Path.Combine(root, "z.dll");
        string textFile = fileSystem.Path.Combine(root, "readme.txt");

        fileSystem.AddFile(firstAssembly, new MockFileData(string.Empty));
        fileSystem.AddFile(secondAssembly, new MockFileData(string.Empty));
        fileSystem.AddFile(textFile, new MockFileData(string.Empty));

        var service = new AssembliesUtilityService(
            root,
            "*.dll",
            fileSystem,
            new RecordingAssemblyLoader());

        // Act
        IEnumerable<string> result = service.ListAllAssemblies();

        // Assert
        result.Should().Equal(
            firstAssembly,
            secondAssembly);
    }

    [Fact]
    public void ListAllAssemblies_WhenDirectoryDoesNotExist_ShouldThrow()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        string missingRoot = fileSystem.Path.GetFullPath("missing");

        var service = new AssembliesUtilityService(
            missingRoot,
            "*.dll",
            fileSystem,
            new RecordingAssemblyLoader());

        // Act
        Action act = () => service.ListAllAssemblies().ToArray();

        // Assert
        act.Should()
            .Throw<DirectoryNotFoundException>()
            .WithMessage("*does not exist*");
    }

    [Fact]
    public void LoadAllAssemblies_WhenPathsAreValid_ShouldUseLoaderInOriginalOrder()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var loader = new RecordingAssemblyLoader();
        string root = fileSystem.Path.GetFullPath("solution");

        var service = new AssembliesUtilityService(
            root,
            "*.dll",
            fileSystem,
            loader);

        string[] paths =
        [
            fileSystem.Path.Combine(root, "a.dll"),
            fileSystem.Path.Combine(root, "b.dll")
        ];

        // Act
        List<Assembly> result = service.LoadAllAssemblies(paths);

        // Assert
        loader.LoadedPaths.Should().Equal(paths);
        result.Should().HaveCount(2);
    }

    [Fact]
    public void LoadAllAssemblies_WhenPathIsWhitespace_ShouldThrowWithRealMessage()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        string root = fileSystem.Path.GetFullPath("solution");

        var service = new AssembliesUtilityService(
            root,
            "*.dll",
            fileSystem,
            new RecordingAssemblyLoader());

        // Act
        Action act = () => service.LoadAllAssemblies([" "]);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*cannot contain null, empty, or whitespace values*");
    }

    private sealed class RecordingAssemblyLoader : IAssemblyLoader
    {
        internal List<string> LoadedPaths { get; } = [];

        public Assembly LoadFromPath(string assemblyPath)
        {
            LoadedPaths.Add(assemblyPath);
            return typeof(RecordingAssemblyLoader).Assembly;
        }
    }
}
