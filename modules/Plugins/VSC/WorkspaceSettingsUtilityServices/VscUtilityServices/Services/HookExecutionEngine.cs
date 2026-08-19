using System;
using System.IO.Abstractions;
using System.Text.Json;
using System.Threading.Tasks;
using VscUtilityServices.Core.Models;
using VscUtilityServices.Core.Services;


namespace WorkspaceUtility.Core.Services;

/// <summary>
/// Coordinates the orchestration lifecycle by parsing configuration payloads and executing corresponding tasks inside the defensive dispatcher infrastructure.
/// </summary>
public class HookExecutionEngine : IHookExecutionEngine
{
    private readonly IFileSystem _fileSystem;
    private readonly ITaskDispatcher _taskDispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="HookExecutionEngine"/> class.
    /// </summary>
    /// <param name="fileSystem">The abstracted file system interface.</param>
    /// <param name="taskDispatcher">The core defensive task dispatcher service.</param>
    public HookExecutionEngine(
        IFileSystem fileSystem,
        ITaskDispatcher taskDispatcher
    )
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(taskDispatcher);

        _fileSystem = fileSystem;
        _taskDispatcher = taskDispatcher;
    }

    /// <summary>
    /// Parses a targeted settings.json5 file and fires matching lifecycle target hooks triggered by VS Code workspace events.
    /// </summary>
    /// <param name="configFilePath">The full path to the settings.json5 infrastructure file.</param>
    /// <param name="workspaceRootPath">The absolute root directory path of the active workspace targeting invocation.</param>
    /// <param name="triggeredEvent">The VS Code runtime lifecycle event context identifier (e.g., vsc-workspace-onentered).</param>
    /// <exception cref="FileNotFoundException">Thrown when the specified config file is missing from the file system storage.</exception>
    /// <exception cref="JsonException">Thrown when the configuration contains illegal payload structure or corrupted fields.</exception>
    public async Task ProcessHookConfigurationAsync(
        string configFilePath,
        string workspaceRootPath,
        string triggeredEvent
    )
    {
        if (string.IsNullOrWhiteSpace(configFilePath)) throw new ArgumentException("Configuration file path cannot be empty.", nameof(configFilePath));
        if (string.IsNullOrWhiteSpace(workspaceRootPath)) throw new ArgumentException("Workspace root path cannot be empty.", nameof(workspaceRootPath));
        if (string.IsNullOrWhiteSpace(triggeredEvent)) throw new ArgumentException("Triggered event metadata cannot be empty.", nameof(triggeredEvent));

        if (!_fileSystem.File.Exists(configFilePath))
        {
            throw new FileNotFoundException($"The required infrastructure configuration file at '{configFilePath}' could not be located.");
        }

        // 防禦性讀取：實務上 json5 帶有註解，此處使用 AllowTrailingCommas 與 ReadComment 進行防禦性解讀
        string rawContent = await _fileSystem.File.ReadAllTextAsync(configFilePath);
        var serializeOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true
        };

        HookConfiguration? configPayload;
        try
        {
            configPayload = JsonSerializer.Deserialize<HookConfiguration>(rawContent, serializeOptions);
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to safely deserialize the JSON5 workspace hook payload. Check for syntax anomalies.", ex);
        }

        if (configPayload?.Hooks == null) return;

        foreach (var hookWrapper in configPayload.Hooks)
        {
            var target = hookWrapper.Target;
            if (target == null || !target.OnEvents.Contains(triggeredEvent)) 
            {
                continue;
            }

            foreach (var task in target.Tasks)
            {
                // 向上防禦導覽屬性 Null 引發的 Crash
                string rawVersion = task.RuntimeSettings?.Requires?.Runtime?.Version;
                string sdkVersion = string.IsNullOrWhiteSpace(rawVersion) ? "0.0" : rawVersion;

                await _taskDispatcher.ExecuteTaskDefensivelyAsync(
                    workspaceRootPath,
                    task.TaskName,
                    task.ProgrammingLanguage,
                    sdkVersion
                );
            }
        }
    }
}
    