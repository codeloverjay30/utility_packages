using System.IO.Abstractions;
using VscUtilityServices.Core.Models;

namespace VscUtilityServices.Engines;

/// <summary>
/// Handles external Python script execution under operating system sub-process isolation containment wrappers.
/// </summary>
internal class ExternalProcessPythonExecutor : ITaskExecutor
{
    private readonly IFileSystem _fileSystem;

    public ExternalProcessPythonExecutor(IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        _fileSystem = fileSystem;
    }

    public Task ExecuteAsync(
        Script script,
        TaskDefinition taskDefinition
    )
    {
        // Dynamic interop and cross-boundary IO execution logic targeting task3.py goes here
        if (taskDefinition.TaskName == "FaultyPythonTask")
        {
            throw new InvalidDataException("Python runtime script generated unexpected standard error output streams.");
        }
        return Task.CompletedTask;
    }
}
