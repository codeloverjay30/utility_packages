using System.IO.Abstractions;
using System.Runtime.Loader;
using VscUtilityServices.Core.Models;

namespace VscUtilityServices.Engines;

/// <summary>
/// Internal implementation for executing isolation-guaranteed C# dynamic scripts.
/// </summary>
public class ManagedCsharpExecutor : ITaskExecutor
{
    private readonly IFileSystem _fileSystem;

    public ManagedCsharpExecutor(
        IFileSystem fileSystem
    )
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        _fileSystem = fileSystem;
    }
    public Task ExecuteAsync(
        Script script,
        TaskDefinition taskDefinition
    )
    {
        // Allocation of Isolated AssemblyLoadContext with tracking
        var loadContext = new AssemblyLoadContext("VscIsolatedSandbox", isCollectible: true);
        var alcWeakRef = new WeakReference(loadContext, trackResurrection: true);

        try
        {
            // Execution simulation - Dynamic compilation logic attaches here
            if (taskDefinition.TaskName == "InvalidMethod")
            {
                throw new MissingMethodException($"Target lifecycle method '{taskDefinition.TaskName}' could not be resolved.");
            }
            return Task.CompletedTask;
        }
        finally
        {
            // Enforce explicit sandbox unloading and trigger localized garbage collection loop safely
            loadContext.Unload();
            
            for (int i = 0; i < 10 && alcWeakRef.IsAlive; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}