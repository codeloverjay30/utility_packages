using System.IO.Abstractions;
using FluentAssertions;
using Moq;
using ScriptDiscoveryUtilityServices;
using ProgrammingLanguageUtilityServices;

namespace ProgrammingLanguageRuntimeUtilityServices.Tests;

/// <summary>
/// Contains defensive and comprehensive unit tests for the <see cref="ScriptDiscoveryEngine"/> class.
/// </summary>
public class ScriptDiscoveryEngineTests
{
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly Mock<ISignatureUtilityService> _signatureUtilityServiceMock;
    private readonly ScriptDiscoveryEngine _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptDiscoveryEngineTests"/> class with strictly mocked behaviors.
    /// </summary>
    public ScriptDiscoveryEngineTests()
    {
        _fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
        _signatureUtilityServiceMock = new Mock<ISignatureUtilityService>(MockBehavior.Strict);

        // Standard sub-service initialization to prevent hidden side-effects under Strict mode
        _fileSystemMock.Setup(fs => fs.Directory).Returns(new Mock<IDirectory>(MockBehavior.Strict).Object);
        _fileSystemMock.Setup(fs => fs.File).Returns(new Mock<IFile>(MockBehavior.Strict).Object);

        _sut = new ScriptDiscoveryEngine(_fileSystemMock.Object, _signatureUtilityServiceMock.Object);
    }

    /// <summary>
    /// Verifies that the constructor throws <see cref="ArgumentNullException"/> when the file system dependency is null.
    /// </summary>
    [Fact]
    public void Constructor_NullFileSystem_ThrowsArgumentNullException()
    {
        // Arrange
        IFileSystem nullFileSystem = null!;

        // Act
        Action act = () => new ScriptDiscoveryEngine(nullFileSystem, _signatureUtilityServiceMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("fileSystem");
    }

    /// <summary>
    /// Verifies that the constructor throws <see cref="ArgumentNullException"/> when the signature utility service dependency is null.
    /// </summary>
    [Fact]
    public void Constructor_NullSignatureUtilityService_ThrowsArgumentNullException()
    {
        // Arrange
        ISignatureUtilityService nullService = null!;

        // Act
        Action act = () => new ScriptDiscoveryEngine(_fileSystemMock.Object, nullService);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("signatureUtilityService");
    }

    /// <summary>
    /// Verifies that <see cref="ScriptDiscoveryEngine.LocateMethodSourcePath"/> throws <see cref="DirectoryNotFoundException"/>
    /// when the targeted root directory does not exist on the file system.
    /// </summary>
    [Fact]
    public void LocateMethodSourcePath_DirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        string nonExistentDir = @"C:\Mock\Invalid\Workspace";
        var directoryMock = new Mock<IDirectory>(MockBehavior.Strict);
        directoryMock.Setup(d => d.Exists(nonExistentDir)).Returns(false);
        _fileSystemMock.Setup(fs => fs.Directory).Returns(directoryMock.Object);

        // Act
        Action act = () => _sut.LocateMethodSourcePath(nonExistentDir, "ExecuteTask", "csharp");

        // Assert
        act.Should().Throw<DirectoryNotFoundException>()
           .WithMessage($"*Root container target path '{nonExistentDir}' does not exist.*");
    }

    /// <summary>
    /// Verifies that <see cref="ScriptDiscoveryEngine.LocateMethodSourcePath"/> throws <see cref="NotSupportedException"/>
    /// when an unsupported or unrecognized programming language boundary is provided.
    /// </summary>
    [Fact]
    public void LocateMethodSourcePath_UnsupportedProgrammingLanguage_ThrowsNotSupportedException()
    {
        // Arrange
        string rootDir = @"C:\Mock\Workspace";
        string unsupportedLanguage = "typescript";

        var directoryMock = new Mock<IDirectory>(MockBehavior.Strict);
        directoryMock.Setup(d => d.Exists(rootDir)).Returns(true);
        _fileSystemMock.Setup(fs => fs.Directory).Returns(directoryMock.Object);

        // Act
        Action act = () => _sut.LocateMethodSourcePath(rootDir, "SomeMethod", unsupportedLanguage);

        // Assert
        act.Should().Throw<NotSupportedException>()
           .WithMessage($"*The programming language '{unsupportedLanguage}' is not supported by the discovery engine.*");
    }

    /// <summary>
    /// Verifies that <see cref="ScriptDiscoveryEngine.LocateMethodSourcePath"/> returns null
    /// when target files exist but none of them contain a matching signature.
    /// </summary>
    [Fact]
    public void LocateMethodSourcePath_FilesExistButNoSignatureMatched_ReturnsNull()
    {
        // Arrange
        string rootDir = @"C:\Mock\Workspace";
        string targetMethod = "ProcessData";
        string language = "csharp";       // Extension pattern: *.cs
        string matchedFile = @"C:\Mock\Workspace\Service.cs";
        string fileContent = "public void MisalignedMethodName() {}";

        var directoryMock = new Mock<IDirectory>(MockBehavior.Strict);
        directoryMock.Setup(d => d.Exists(rootDir)).Returns(true);
        directoryMock.Setup(d => d.GetFiles(rootDir, "*.cs", SearchOption.AllDirectories))
                     .Returns(new[] { matchedFile });

        var fileMock = new Mock<IFile>(MockBehavior.Strict);
        fileMock.Setup(f => f.ReadAllText(matchedFile)).Returns(fileContent);

        _fileSystemMock.Setup(fs => fs.Directory).Returns(directoryMock.Object);
        _fileSystemMock.Setup(fs => fs.File).Returns(fileMock.Object);

        _signatureUtilityServiceMock.Setup(s => s.IsSignatureMatched(
            fileContent, 
            language, 
            It.Is<SignatureInfo>(info => info.MethodName == targetMethod)
        )).Returns(false);

        // Act
        string? result = _sut.LocateMethodSourcePath(rootDir, targetMethod, language);

        // Assert
        result.Should().BeNull("because no file contains the designated target method signature layout.");
    }

    /// <summary>
    /// Verifies that <see cref="ScriptDiscoveryEngine.LocateMethodSourcePath"/> successfully returns the absolute file path
    /// when a valid file containing the target method signature is discovered.
    /// </summary>
    [Fact]
    public void LocateMethodSourcePath_TargetSignatureFound_ReturnsAbsoluteFilePath()
    {
        // Arrange
        string rootDir = @"C:\Mock\Workspace";
        string targetMethod = "fetch_user_profile";
        string language = "python";       // Extension pattern: *.py
        string PythonFile1 = @"C:\Mock\Workspace\main.py";
        string PythonFile2 = @"C:\Mock\Workspace\auth.py";
        string fileContent1 = "def unrelated_func(): pass";
        string fileContent2 = "def fetch_user_profile(user_id): pass";

        var directoryMock = new Mock<IDirectory>(MockBehavior.Strict);
        directoryMock.Setup(d => d.Exists(rootDir)).Returns(true);
        directoryMock.Setup(d => d.GetFiles(rootDir, "*.py", SearchOption.AllDirectories))
                     .Returns(new[] { PythonFile1, PythonFile2 });

        var fileMock = new Mock<IFile>(MockBehavior.Strict);
        fileMock.Setup(f => f.ReadAllText(PythonFile1)).Returns(fileContent1);
        fileMock.Setup(f => f.ReadAllText(PythonFile2)).Returns(fileContent2);

        _fileSystemMock.Setup(fs => fs.Directory).Returns(directoryMock.Object);
        _fileSystemMock.Setup(fs => fs.File).Returns(fileMock.Object);

        // First file setup - un-matched
        _signatureUtilityServiceMock.Setup(s => s.IsSignatureMatched(
            fileContent1, 
            language, 
            It.Is<SignatureInfo>(info => info.MethodName == targetMethod)
        )).Returns(false);

        // Second file setup - matched
        _signatureUtilityServiceMock.Setup(s => s.IsSignatureMatched(
            fileContent2, 
            language, 
            It.Is<SignatureInfo>(info => info.MethodName == targetMethod)
        )).Returns(true);

        // Act
        string? result = _sut.LocateMethodSourcePath(rootDir, targetMethod, language);

        // Assert
        result.Should().NotBeNull("because a matching routine definition pattern was successfully parsed inside the script framework.")
              .And.Be(PythonFile2);
    }
}