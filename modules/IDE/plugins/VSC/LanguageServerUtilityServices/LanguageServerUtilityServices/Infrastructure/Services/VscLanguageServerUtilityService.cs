using System.IO.Abstractions;
using CliUtilityServices;
using CliWrap;
using Commands.Infrastructure;
using EnvironmentUtilityServices;

using LanguageServerUtilityServices.Infrastructure.Interfaces;

namespace LanguageServerUtilityServices.Infrastructure.Services;

/// <summary>
/// Represents the core entry point and background process coordinator for the C#-based backend extension service.
/// </summary>    
public sealed class VscLanguageServerUtilityService : ILanguageServerUtilityService
{
    private readonly CancellationTokenSource _cts = new();

    private readonly IEnvironmentService _environmentService;
    private readonly IFileSystem _fileSystem = new FileSystem(); // Defensive initialization of file system abstraction

    private readonly ICliCommandExecutor _cliCommandExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="VscLanguageServerUtilityService"/> class.
    /// </summary>
    /// <param name="cliCommandExecutor">The CLI command executor service.</param>
    /// <param name="environmentService">The environment service for managing environment variables and context.</param>
    public VscLanguageServerUtilityService(
        ICliCommandExecutor cliCommandExecutor,
        IEnvironmentService environmentService
    )
    {
        ArgumentNullException.ThrowIfNull(cliCommandExecutor, nameof(cliCommandExecutor));
        ArgumentNullException.ThrowIfNull(environmentService, nameof(environmentService));

        _cliCommandExecutor = cliCommandExecutor;
        _environmentService = environmentService;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="VscLanguageServerUtilityService"/> class with file system abstraction.
    /// </summary>
    /// <param name="cliCommandExecutor">The CLI command executor service.</param>
    /// <param name="fileSystem">The file system abstraction for file operations.</param>
    /// <param name="environmentService">The environment service for managing environment variables and context.</param>
    public VscLanguageServerUtilityService(
        ICliCommandExecutor cliCommandExecutor,
        IFileSystem fileSystem,
        IEnvironmentService environmentService
    )
    {
        ArgumentNullException.ThrowIfNull(cliCommandExecutor, nameof(cliCommandExecutor));
        ArgumentNullException.ThrowIfNull(fileSystem, nameof(fileSystem));
        ArgumentNullException.ThrowIfNull(environmentService, nameof(environmentService));

        _cliCommandExecutor = cliCommandExecutor;
        _fileSystem = fileSystem;
        _environmentService = environmentService;
    }

    /// <inheritdoc cref="global::LanguageServerUtilityServices.Infrastructure.Interfaces.ILanguageServerUtilityService.StartAsync"/>
    /// <exception cref="OperationCanceledException">Thrown when the Operation Cancelled signal is received during execution.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the server fails to bind network or pipe resources or encounters an unhandled infrastructure fault.</exception>
    public async Task<CommandExecutionResult> StartAsync(CommandLineInput commandLineInput)
    {
        try
        {
            var commandExecutionResult = await _cliCommandExecutor.ExecuteAutoDetectedAsync(commandLineInput).ConfigureAwait(false);

            return commandExecutionResult;
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException("Server shutdown requested.");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            throw new InvalidOperationException($"Failed to initialize extension server process.\nDetails: {ex.Message}", ex);
        }
        catch (OutOfMemoryException ex)
        {
            throw new OutOfMemoryException($"Out of memory.\nDetails: {ex.Message}.", ex);
        }
    }
    
    /// <inheritdoc cref="ILanguageServerUtilityService.ShowMessageAsync(string, IEnumerable{string}, CancellationToken)"/>
    public async Task<CommandExecutionResult> ShowMessageAsync(
        string command,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command, nameof(command));
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));

        // 集中在底層組裝 CommandLineInput，確保 EnvironmentService 絕對一致，杜絕平行時空 Bug
        var commandInput = new CommandLineInputBuilder()
            .WithCommand(command)
            .WithArguments(arguments)
            .WithValidation(CommandResultValidation.ZeroExitCode)
            .WithEnvironmentService(_environmentService)
            .Build();

        return await StartAsync(commandInput);
    }

    /// <inheritdoc cref="ILanguageServerUtilityService.InstallExtensionAsync(string, CancellationToken)"/>
    public async Task<CommandExecutionResult> InstallExtensionAsync(
        string extensionId,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Defensive validation: Ensure file system is accessible before triggering CLI or extension installation
        if (_fileSystem == null)
        {
            throw new InvalidOperationException("File system abstraction is not initialized.");
        }

        // Defensive validation: Ensure extension identifier format is structurally safe before command execution
        if (extensionId.IndexOfAny(_fileSystem.Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("Extension identifier contains invalid characters.", nameof(extensionId));
        }

        try
        {
            // TODO: Integrate with CliUtilityServices for actual VSIX installation logic or execute via VS Code command piping
            // Example integration point for C# backend CLI utility:
            // var cliResult = await CliUtilityServices.ExecuteCommandAsync("code", new[] { "--install-extension", extensionId }, cancellationToken);

            CommandLineInput commandLineInput = new CommandLineInputBuilder()
                .WithCommand("code")
                .WithArguments(new[] { "--install-extension", extensionId })
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .WithEnvironmentService(_environmentService)
                .Build();

            // 設定逾時防禦機制（例如：300 秒逾時，避免程序無回應卡死）
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(300)); // 300 秒逾時
            var cliResult = await _cliCommandExecutor.ExecuteAutoDetectedAsync(commandLineInput);
            if (cliResult.ExitCode != 0)
            {
                throw new InvalidOperationException($"""
                Failed to install extension '{extensionId}'.
                Exit code: {cliResult.ExitCode}.
                Output: {cliResult.StandardOutput}.
                Error: {cliResult.StandardError}
                """);

                // --- (NOT RECOMMENDED)
                // --- Alternatively, you could return a failed Task instead of throwing an exception, 
                // --- depending on the desired behavior
                // await Task.FromException(new InvalidOperationException($"""
                // Failed to install extension '{extensionId}'.
                // Exit code: {cliResult.ExitCode}.
                // Output: {cliResult.StandardOutput}.
                // Error: {cliResult.StandardError}
                // """);
            }
            return cliResult;
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException("Extension installation was canceled.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"""
            Failed to install extension '{extensionId}'.
            Details: {ex.Message}
            """, ex);
        }
    }
}
