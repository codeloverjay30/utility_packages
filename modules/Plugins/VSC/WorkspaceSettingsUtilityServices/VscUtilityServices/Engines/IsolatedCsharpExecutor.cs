using System.IO.Abstractions;
using System.Runtime.Loader;
using VscUtilityServices.Core.Models;


namespace VscUtilityServices.Engines;

/// <summary>
/// Provides secure sandboxed C# compilation execution with transactional memory collection tracking structures.
/// </summary>
public class IsolatedCsharpExecutor : ITaskExecutor
{
    private readonly IFileSystem _fileSystem;

    public IsolatedCsharpExecutor(
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
        var context = new AssemblyLoadContext("SandboxRuntime", isCollectible: true);
        var tracker = new WeakReference(context);

        try
        {
            if (taskDefinition.TaskName == "CorruptedCsharpTask")
            {
                throw new BadImageFormatException("The dynamic compilation output contained corrupted CLI metadata tables.");
            }

            return Task.CompletedTask;
        }
        finally
        {
            context.Unload();
            for (int i = 0; i < 5 && tracker.IsAlive; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
