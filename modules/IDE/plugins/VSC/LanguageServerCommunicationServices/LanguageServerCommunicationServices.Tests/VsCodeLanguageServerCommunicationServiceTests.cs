using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CliUtilityServices;
using Commands.Infrastructure;
using CommandResult.Infrastructure;
using EnvironmentUtilityServices;
using FluentAssertions;
using LanguageServerCommunicationService;
using LanguageServerUtilityServices.Infrastructure.Interfaces;
using Moq;
using Xunit;
using LanguageServerCommunicationServices;

namespace LanguageServerCommunicationService.Tests;

/// <summary>
/// Unit tests for VsCodeLanguageServerCommunicationService implementation.
/// </summary>
public class VsCodeLanguageServerCommunicationServiceTests
{
    private readonly Mock<ILanguageServerUtilityService> _languageServerUtilityServiceMock;
    private readonly VsCodeLanguageServerCommunicationService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="VsCodeLanguageServerCommunicationServiceTests"/> class.
    /// </summary>
    public VsCodeLanguageServerCommunicationServiceTests()
    {
        _languageServerUtilityServiceMock = new Mock<ILanguageServerUtilityService>(MockBehavior.Strict);
        _sut = new VsCodeLanguageServerCommunicationService(_languageServerUtilityServiceMock.Object);
    }

    /// <summary>
    /// Verifies that ShowInfoAsync executes successfully when valid message and plugin info are provided.
    /// </summary>
    [Fact]
    public async Task ShowInfoAsync_WithValidInputs_ShouldInvokeUtilityServiceSuccessfully()
    {
        // Arrange
        var pluginInfo = new PluginInfo { Name = "testPlugin" };
        string message = "Hello VS Code";
        var expectedResult = new CommandExecutionResult(string.Empty, string.Empty, 0, TimeSpan.FromMilliseconds(40));

        _languageServerUtilityServiceMock
            .Setup(x => x.ShowMessageAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        Func<Task> act = async () => await _sut.ShowInfoAsync(message, pluginInfo, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();

        _languageServerUtilityServiceMock.Verify(
            x => x.ShowMessageAsync(
                It.Is<string>(cmd => cmd == "code"),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that ShowInfoAsync throws ArgumentException when message is null or whitespace.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ShowInfoAsync_WithNullOrWhitespaceMessage_ShouldThrowArgumentException(string invalidMessage)
    {
        // Arrange
        var pluginInfo = new PluginInfo { Name = "testPlugin" };

        // Act
        Func<Task> act = async () => await _sut.ShowInfoAsync(invalidMessage, pluginInfo, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Verifies that ShowInfoAsync throws ArgumentNullException when pluginInfo is null.
    /// </summary>
    [Fact]
    public async Task ShowInfoAsync_WhenPluginInfoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        PluginInfo? pluginInfo = null;

        // Act
        Func<Task> act = async () => await _sut.ShowInfoAsync("Valid message", pluginInfo!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that ShowInfoAsync wraps underlying timeout or cancellation into TimeoutException.
    /// </summary>
    [Fact]
    public async Task ShowInfoAsync_WhenOperationCanceled_ShouldThrowTimeoutException()
    {
        // Arrange
        var pluginInfo = new PluginInfo { Name = "testPlugin" };

        _languageServerUtilityServiceMock
            .Setup(x => x.ShowMessageAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        Func<Task> act = async () => await _sut.ShowInfoAsync("message", pluginInfo, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TimeoutException>()
            .WithMessage("*向 VS Code 發送訊息逾時*");
    }

    /// <summary>
    /// Verifies that ShowInfoAsync wraps unexpected exceptions into InvalidOperationException.
    /// </summary>
    [Fact]
    public async Task ShowInfoAsync_WhenUnexpectedExceptionOccurs_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var pluginInfo = new PluginInfo { Name = "testPlugin" };

        _languageServerUtilityServiceMock
            .Setup(x => x.ShowMessageAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Low-level failure"));

        // Act
        Func<Task> act = async () => await _sut.ShowInfoAsync("message", pluginInfo, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*無法向 VS Code 發送訊息*");
    }

    /// <summary>
    /// Verifies that ExecuteAsync executes successfully with valid command line input.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithValidCommandLineInput_ShouldInvokeStartAsync()
    {
        // Arrange
        var environmentServiceMock = new Mock<IEnvironmentService>(MockBehavior.Strict);
        var commandLineInput = new CommandLineInputBuilder()
            .WithCommand("code")
            .WithEnvironmentService(environmentServiceMock.Object)
            .Build();

        var expectedResult = new CommandExecutionResult(string.Empty, string.Empty, 0, TimeSpan.FromMilliseconds(20));

        _languageServerUtilityServiceMock
            .Setup(x => x.StartAsync(It.IsAny<CommandLineInput>()))
            .ReturnsAsync(expectedResult);

        // Act
        Func<Task> act = async () => await _sut.ExecuteAsync(commandLineInput, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();

        _languageServerUtilityServiceMock.Verify(
            x => x.StartAsync(It.Is<CommandLineInput>(ci => ci.Command == "code")),
            Times.Once);
    }
}