using System.IO.Abstractions;
using VscUtilityServices.Core.Models;
using VscUtilityServices.Validators;

namespace VscUtilityServices.Engines;

/// <summary>
/// The enterprise-level orchestration engine that handles multi-language hook execution with strict environment guarantees.
/// </summary>
public class ComprehensiveHookEngine
{
    private readonly IFileSystem _fileSystem;
    private readonly IRuntimeEnvironmentValidator _environmentValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComprehensiveHookEngine"/> class.
    /// </summary>
    /// <param name="fileSystem">The abstracted file system dependency wrapper.</param>
    /// <param name="environmentValidator">The system utility wrapper used to verify external runtime installations.</param>
    public ComprehensiveHookEngine(
        IFileSystem fileSystem,
        IRuntimeEnvironmentValidator environmentValidator
    )
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(environmentValidator);

        _fileSystem = fileSystem;
        _environmentValidator = environmentValidator;
    }

    /// <summary>
    /// Evaluates workspace hooks, verifies environment preconditions, and dispatches the task execution strategy.
    /// </summary>
    /// <param name="workspacePath">The directory tracking root path of the active VSC workspace.</param>
    /// <param name="script">The script metadata and contextual information wrapper.</param>
    /// <param name="taskDefinition">The deserialized task definition extracted from the JSON5 configuration.</param>
    /// <param name="runtimeRequirement">The runtime requirement specification extracted from the JSON5 configuration.</param>
    /// <returns>A task representing the asynchronous execution flow of the workspace lifecycle target.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null, empty, or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified workspace directory path does not exist on the file system.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the required external SDK or language runtime is missing from the host machine.</exception>
    public async Task ProcessWorkspaceLifecycleTargetAsync(
        string workspacePath,
        Script script,
        TaskDefinition taskDefinition,
        RuntimeRequirement runtimeRequirement
    )
    {
        if (string.IsNullOrWhiteSpace(workspacePath) || taskDefinition == null)
        {
            throw new ArgumentException("Workspace target path and task metadata identifiers must be supplied.");
        }
        if (!_fileSystem.Directory.Exists(workspacePath))
        {
            throw new DirectoryNotFoundException($"The target VSC workspace directory context was not found: {workspacePath}");
        }
        
        // Defensive Check: Enforce runtime requirement guard clauses before launching the script environment
        var requiredSdk = runtimeRequirement.Runtime.Sdk;
        if (!string.IsNullOrWhiteSpace(requiredSdk))
        {
            bool isEnvironmentValid = await _environmentValidator.ValidateRuntimeExistsAsync(runtimeRequirement);
            if (!isEnvironmentValid)
            {
                throw new InvalidOperationException($"Precondition Failure: Required runtime environment '{requiredSdk}' is missing on this machine.");
            }
        }

        // Strategy Factory: Resolve concrete runtime execution encapsulation
        var language = script.LanguageInfo.Name;
        ITaskExecutor executor = language.ToLowerInvariant() switch
        {
            "csharp" => new IsolatedCsharpExecutor(_fileSystem),
            "fsharp" => new IsolatedFsharpExecutor(_fileSystem),
            "python" => new ExternalProcessPythonExecutor(_fileSystem),
            _ => throw new NotSupportedException($"The requested language strategy pipeline '{language}' is not implemented within this utility package version.")
        };

        await executor.ExecuteAsync(script, taskDefinition);
    }
}
