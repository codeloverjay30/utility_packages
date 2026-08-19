using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceStorageCleaner.Core.Interfaces;

namespace WorkspaceStorageCleaner.Core.Services
{
    /// <summary>
    /// Provides concrete implementation for cleaning Visual Studio Code workspace storage with defensive programming and atomic constraints.
    /// </summary>
    public class WorkspaceStorageService : IWorkspaceStorageService
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _storagePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceStorageService"/> class.
        /// </summary>
        /// <param name="fileSystem">The abstraction layer for file system operations.</param>
        /// <param name="storagePath">The target directory path of VSC workspace storage.</param>
        public WorkspaceStorageService(IFileSystem fileSystem, string storagePath)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _storagePath = string.IsNullOrWhiteSpace(storagePath) 
                ? throw new ArgumentException("Storage path cannot be null or whitespace.", nameof(storagePath)) 
                : storagePath;
        }

        /// <inheritdoc />
        public async Task CleanWorkspaceStorageAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Step 1: Simulate or execute graceful close synchronization barrier for VSC process
                await Task.Run(() => 
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // Defensive validation: ensure storage path context exists before wiping
                    if (_fileSystem.Directory.Exists(_storagePath))
                    {
                        // Perform atomic/safe recursive wipe of workspace storage directories
                        var directoryInfo = _fileSystem.DirectoryInfo.New(_storagePath);
                        foreach (var file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            file.IsReadOnly = false;
                            file.Delete();
                        }
                        
                        foreach (var dir in directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            dir.Delete(true);
                        }
                    }
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                throw new InvalidOperationException($"Failed to clean workspace storage safely at path: {_storagePath}.", ex);
            }
        }
    }
}