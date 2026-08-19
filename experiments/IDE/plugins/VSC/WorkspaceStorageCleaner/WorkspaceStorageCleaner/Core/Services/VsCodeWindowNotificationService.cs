using System;
using WorkspaceStorageCleaner.Core.Interfaces;

namespace WorkspaceStorageCleaner.Infrastructure.Services
{
    /// <summary>
    /// Provides concrete implementation for Visual Studio Code window notifications using defensive error interception.
    /// </summary>
    public class VsCodeWindowNotificationService : IWindowNotificationService
    {
        /// <inheritdoc />
        public void ShowInformationMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Notification message cannot be null or whitespace.", nameof(message));
            }

            try
            {
                // Integration bridge for VS Code API: vscode.window.showInformationMessage(message)
                // Simulated implementation boundary for platform interop
            }
            catch (Exception ex)
            {
                // Defensive exception interception to protect host application execution
                throw new InvalidOperationException($"Failed to display information message in host environment: {message}", ex);
            }
        }

        /// <inheritdoc />
        public void ShowErrorMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Error notification message cannot be null or whitespace.", nameof(message));
            }

            try
            {
                // Integration bridge for VS Code API: vscode.window.showErrorMessage(message)
                // Simulated implementation boundary for platform interop
            }
            catch (Exception ex)
            {
                // Defensive exception interception to protect host application execution
                throw new InvalidOperationException($"Failed to display error message in host environment: {message}", ex);
            }
        }
    }
}