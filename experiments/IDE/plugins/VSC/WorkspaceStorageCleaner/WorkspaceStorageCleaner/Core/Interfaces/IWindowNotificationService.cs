namespace WorkspaceStorageCleaner.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for interacting with the host editor's window notification system.
    /// </summary>
    public interface IWindowNotificationService
    {
        /// <summary>
        /// Displays an information notification message within the host environment UI.
        /// </summary>
        /// <param name="message">The message content to display.</param>
        void ShowInformationMessage(string message);

        /// <summary>
        /// Displays an error notification message within the host environment UI.
        /// </summary>
        /// <param name="message">The error message content to display.</param>
        void ShowErrorMessage(string message);
    }
}