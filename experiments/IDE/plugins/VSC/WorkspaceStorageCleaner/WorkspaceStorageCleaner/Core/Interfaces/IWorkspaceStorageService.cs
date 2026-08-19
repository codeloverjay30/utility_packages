using System.Threading;
using System.Threading.Tasks;

namespace WorkspaceStorageCleaner.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for managing and cleaning Visual Studio Code workspace storage.
    /// </summary>
    public interface IWorkspaceStorageService
    {
        /// <summary>
        /// Gracefully attempts to shut down Visual Studio Code and atomically remove its workspace storage.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous clean operation.</returns>
        Task CleanWorkspaceStorageAsync(CancellationToken cancellationToken = default);
    }
}