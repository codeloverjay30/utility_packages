using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceStorageCleaner.Core.Interfaces;
using WorkspaceStorageCleaner.Core.Services;

namespace WorkspaceStorageCleaner.Infrastructure
{
    /// <summary>
    /// Registers commands, status bar menu bindings, and extension lifecycle hooks for Visual Studio Code integration.
    /// </summary>
    public static class ExtensionActivation
    {
        private static IWorkspaceStorageService _storageService;
        private static IFileSystem _fileSystem;

        /// <summary>
        /// Initializes and activates the extension components, binding the workspace storage cleanup routine to the UI context menu and navigation bar.
        /// </summary>
        /// <param name="storagePath">The target workspace storage directory path on disk.</param>
        public static void RegisterCommands(string storagePath)
        {
            // Defensive validation for initialization parameters
            if (string.IsNullOrWhiteSpace(storagePath))
            {
                throw new ArgumentException("Storage path cannot be null or whitespace during extension registration.", nameof(storagePath));
            }

            _fileSystem = new FileSystem();
            _storageService = new WorkspaceStorageService(_fileSystem, storagePath);

            // Register the VSC command ID mapping to the execution handler
            RegisterCommandInternal("workspaceStorageCleaner.cleanStorage", async () =>
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                try
                {
                    // Execute the atomic shutdown and deletion sequence
                    await _storageService.CleanWorkspaceStorageAsync(cts.Token).ConfigureAwait(false);
                    
                    // Output success notification to VS Code UI channel (Simulated invocation context)
                    ShowInformationMessage("VS Code workspace storage successfully cleaned and process terminated atomically.");
                }
                catch (OperationCanceledException)
                {
                    ShowErrorMessage("The workspace storage cleaning operation was canceled due to a timeout or user interruption.");
                }
                catch (Exception ex)
                {
                    // Defensive error interception to protect the host editor environment
                    ShowErrorMessage($"Failed to execute workspace storage cleanup: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Internal bridge helper to register commands into the VS Code extension command registry.
        /// </summary>
        /// <param name="commandId">The unique identifier of the command.</param>
        /// <param name="handler">The asynchronous callback handler when the command is invoked.</param>
        private static void RegisterCommandInternal(string commandId, Func<Task> handler)
        {
            if (string.IsNullOrWhiteSpace(commandId))
            {
                throw new ArgumentNullException(nameof(commandId));
            }

            _ = handler ?? throw new ArgumentNullException(nameof(handler));

            // Simulating VS Code API: vscode.commands.registerCommand(commandId, async () => await handler())
        }

        /// <summary>
        /// Displays an information notification message within the Visual Studio Code user interface.
        /// </summary>
        /// <param name="message">The message content to display.</param>
        private static void ShowInformationMessage(string message)
        {
            // Integration stub for vscode.window.showInformationMessage
        }

        /// <summary>
        /// Displays an error notification message within the Visual Studio Code user interface.
        /// </summary>
        /// <param name="message">The error message content to display.</param>
        private static void ShowErrorMessage(string message)
        {
            // Integration stub for vscode.window.showErrorMessage
        }
    }
}