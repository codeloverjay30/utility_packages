using CliUtilityServices;
using Commands.Infrastructure;

namespace LanguageServerUtilityServices.Infrastructure.Interfaces;

public interface ILanguageServerUtilityService
{
    /// <summary>
    /// Initializes and starts the background processing server safely with robust defensive exception handling.
    /// </summary>
    /// <param name="commandLineInput">A <see cref="global::CliUtilityServices.CommandLineInput"/> that will be executed.</param>
    /// <returns>
    /// An <see cref="global::Commands.Infrastructure.CommandExecutionResult"/> instance representing the execution result of this method.
    /// </returns>
    Task<CommandExecutionResult> StartAsync(CommandLineInput commandLineInput);

    /// <summary>
    /// Builds and executes a show message command for VS Code extensions.
    /// </summary>
    /// <param name="command">The CLI command name.</param>
    /// <param name="arguments">The command line arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the command execution result.</returns>
    Task<CommandExecutionResult> ShowMessageAsync(
        string command,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Installs a VS Code extension by its identifier <paramref name="extensionId"/>
    /// </summary>
    /// <param name="extensionId">The identifier of the extension to install.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the command execution result.</returns>
    Task<CommandExecutionResult> InstallExtensionAsync(
        string extensionId,
        CancellationToken cancellationToken = default
    );
}
