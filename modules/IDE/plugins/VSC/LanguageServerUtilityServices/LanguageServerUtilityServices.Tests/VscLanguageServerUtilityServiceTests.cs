using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using CliUtilityServices;
using Commands.Infrastructure;
using EnvironmentUtilityServices;
using FluentAssertions;
using LanguageServerUtilityServices.Infrastructure.Services;
using Moq;
using Xunit;

namespace LanguageServerUtilityServices.Tests;

/// <summary>
/// Represents unit tests for the <see cref="VscLanguageServerUtilityService"/> class, 
/// focusing specifically on the <see cref="VscLanguageServerUtilityService.InstallExtensionAsync"/> method.
/// </summary>
public sealed class VscLanguageServerUtilityServiceTests
{
    private readonly Mock<ICliCommandExecutor> _cliCommandExecutorMock;
    private readonly Mock<IEnvironmentService> _environmentServiceMock;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="VscLanguageServerUtilityServiceTests"/> class.
    /// </summary>
    public VscLanguageServerUtilityServiceTests()
    {
        _cliCommandExecutorMock = new Mock<ICliCommandExecutor>(MockBehavior.Strict);
        _environmentServiceMock = new Mock<IEnvironmentService>(MockBehavior.Strict);
        _fileSystem = new MockFileSystem(); // Defensive use of file system abstraction
    }

    /// <summary>
    /// Verifies that InstallExtensionAsync successfully executes the CLI command and returns the result when given a valid extension identifier.
    /// </summary>
    [Fact]
    public async Task InstallExtensionAsync_WithValidExtensionId_ShouldExecuteSuccessfullyAndReturnResult()
    {
        // Arrange
        var extensionId = "publisher.extension-name";
        // Corrected according to CommandExecutionResult(string StandardOutput, string StandardError, int ExitCode, TimeSpan RunTime) signature
        var expectedResult = new CommandExecutionResult("Installed successfully", string.Empty, 0, TimeSpan.FromSeconds(1));

        _cliCommandExecutorMock
            .Setup(x => x.ExecuteAutoDetectedAsync(It.IsAny<CommandLineInput>()))
            .ReturnsAsync(expectedResult);

        var service = new VscLanguageServerUtilityService(
            _cliCommandExecutorMock.Object,
            _fileSystem,
            _environmentServiceMock.Object
        );

        // Act
        var result = await service.InstallExtensionAsync(extensionId);

        // Assert
        result.Should().NotBeNull();
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Contain("Installed successfully");

        _cliCommandExecutorMock.Verify(
            x => x.ExecuteAutoDetectedAsync(It.Is<CommandLineInput>(input => 
                input.Command == "code" && 
                input.Arguments.Contains("--install-extension") && 
                input.Arguments.Contains(extensionId))),
            Times.Once
        );
    }

    /// <summary>
    /// Verifies that InstallExtensionAsync throws an ArgumentException when the extension identifier contains invalid file name characters.
    /// </summary>
    [Fact]
    public async Task InstallExtensionAsync_WithInvalidCharactersInExtensionId_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidExtensionId = "publisher/invalid|extension"; 

        var service = new VscLanguageServerUtilityService(
            _cliCommandExecutorMock.Object,
            _fileSystem,
            _environmentServiceMock.Object
        );

        // Act
        Action act = () => service.InstallExtensionAsync(invalidExtensionId).GetAwaiter().GetResult();

        // Assert
        act.Should().Throw<ArgumentException>()
           .Where(p=>p.ParamName == "extensionId")
           .WithMessage("*Extension identifier contains invalid characters*");
    }

    /// <summary>
    /// Verifies that InstallExtensionAsync throws an InvalidOperationException when the CLI command returns a non-zero exit code.
    /// </summary>
    [Fact]
    public async Task InstallExtensionAsync_WhenCliReturnsNonZeroExitCode_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var extensionId = "publisher.extension-name";
        // Corrected according to CommandExecutionResult signature[cite: 2]
        var failedResult = new CommandExecutionResult(string.Empty, "Installation failed due to network error", 1, TimeSpan.FromSeconds(1));

        _cliCommandExecutorMock
            .Setup(x => x.ExecuteAutoDetectedAsync(It.IsAny<CommandLineInput>()))
            .ReturnsAsync(failedResult);

        var service = new VscLanguageServerUtilityService(
            _cliCommandExecutorMock.Object,
            _fileSystem,
            _environmentServiceMock.Object
        );

        // Act
        Action act = () => service.InstallExtensionAsync(extensionId).GetAwaiter().GetResult();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage($"*Failed to install extension '{extensionId}'*Exit code: 1*");
    }

    /// <summary>
    /// Verifies that InstallExtensionAsync throws an OperationCanceledException when the cancellation token is already canceled.
    /// </summary>
    [Fact]
    public async Task InstallExtensionAsync_WithCanceledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var extensionId = "publisher.extension-name";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var service = new VscLanguageServerUtilityService(
            _cliCommandExecutorMock.Object,
            _fileSystem,
            _environmentServiceMock.Object
        );

        // Act
        Action act = () => service.InstallExtensionAsync(extensionId, cts.Token).GetAwaiter().GetResult();

        // Assert
        act.Should().Throw<OperationCanceledException>();
    }
}