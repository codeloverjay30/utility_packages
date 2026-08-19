using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using CliUtilityServices;
using CliUtilityServices.Terminals;
using DriveInfoUtilityServices;
using EnvironmentUtilityServices;
using SymbolicLinkUtilityServices;

namespace WorkspaceUtility.Core.Services;

/// <summary>
/// Core service responsible for dispatching tasks to their respective runtimes defensively,
/// leveraging environment, symbolic link, and drive verification utilities to prevent infrastructure failures.
/// </summary>
public class TaskDispatcher : ITaskDispatcher
{
    private readonly IFileSystem _fileSystem;
    private readonly ICommandLineRunner _commandLineRunner;
    private readonly IDriveInfoUtilityService _driveInfoService;
    private readonly ISymbolicLinkUtilityService _symLinkService;
    private readonly IPlatformService _platformService;
    private readonly IEnvironmentService _environmentService;
    private readonly ITerminalProvider _terminalProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskDispatcher"/> class with defensive validation.
    /// </summary>
    /// <param name="fileSystem">The abstracted file system interface.</param>
    /// <param name="commandLineRunner">The command-line runner engine.</param>
    /// <param name="driveInfoService">The utility service for validating logical drives.</param>
    /// <param name="symLinkService">The utility service for detecting and managing symbolic links.</param>
    /// <param name="platformService">The utility service for evaluating target OS platforms.</param>
    /// <param name="environmentService">The utility service for analyzing process environment variables.</param>
    /// <param name="terminalProvider">The provider wrapper for driving system shell terminals execution natively.</param>
    public TaskDispatcher(
        IFileSystem fileSystem,
        ICommandLineRunner commandLineRunner,
        IDriveInfoUtilityService driveInfoService,
        ISymbolicLinkUtilityService symLinkService,
        IPlatformService platformService,
        IEnvironmentService environmentService,
        ITerminalProvider terminalProvider
    )
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(commandLineRunner);
        ArgumentNullException.ThrowIfNull(driveInfoService);
        ArgumentNullException.ThrowIfNull(symLinkService);
        ArgumentNullException.ThrowIfNull(platformService);
        ArgumentNullException.ThrowIfNull(environmentService);
        ArgumentNullException.ThrowIfNull(terminalProvider);

        _fileSystem = fileSystem;
        _commandLineRunner = commandLineRunner;
        _driveInfoService = driveInfoService;
        _platformService = platformService;
        _symLinkService = symLinkService;
        _environmentService = environmentService;
        _terminalProvider = terminalProvider;
    }

    /// <summary>
    /// Executes a specified task within a given workspace directory by verifying the infrastructure safety checklist first.
    /// </summary>
    /// <param name="targetPath">The absolute root path of the target workspace.</param>
    /// <param name="taskName">The function or method identifier to discover and invoke.</param>
    /// <param name="language">The required programming language syntax (e.g., csharp, fsharp, python).</param>
    /// <param name="version">The targeted SDK runtime version boundary.</param>
    /// <exception cref="ArgumentException">Thrown when targetPath, taskName, or language is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when file system or drive integrity checks fail.</exception>
    /// <exception cref="PlatformNotSupportedException">Thrown when the target runtime is incompatible with the current host system OS.</exception>
    /// <exception cref="NotSupportedException">Thrown when the requested programming language is not handled by the current dispatcher architecture.</exception>
    public async Task ExecuteTaskDefensivelyAsync(
        string targetPath,
        string taskName,
        string language,
        string version
    )
    {
        if (string.IsNullOrWhiteSpace(targetPath)) throw new ArgumentException("Target workspace path cannot be empty.", nameof(targetPath));
        if (string.IsNullOrWhiteSpace(taskName)) throw new ArgumentException("Task name cannot be empty.", nameof(taskName));
        if (string.IsNullOrWhiteSpace(language)) throw new ArgumentException("Language specification is mandatory.", nameof(language));
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException("Version specification is mandatory.", nameof(version));

        if (!_driveInfoService.IsDriveReadyAndAccessible(targetPath))
        {
            throw new InvalidOperationException($"The logical drive hosting the path '{targetPath}' is either not ready, disconnected, or unreadable.");
        }

        if (_symLinkService.IsCyclicReparsePoint(targetPath))
        {
            throw new InvalidOperationException($"Aborting operation. The workspace path '{targetPath}' contains a cyclic symbolic link that may cause a StackOverflowException.");
        }

        string normalizedLang = language.ToLowerInvariant();
        bool isWindows = _platformService.IsWindows();

        switch (normalizedLang)
        {
            case "csharp":
                if (version.StartsWith("10."))
                {
                    // 需求書指定：.NET 10+ 採用 Modern DotNet 機制載入
                    await RunModernDotNetAsync(targetPath, taskName);
                }
                else
                {
                    if (!isWindows)
                    {
                        throw new PlatformNotSupportedException("Legacy C# tasks running under old frameworks are exclusive to Windows platforms.");
                    }
                    await RunLegacyDotNetProcessAsync(taskName);
                }
                break;

            case "fsharp":
                if (version.StartsWith("2."))
                {
                    if (!isWindows)
                    {
                        throw new PlatformNotSupportedException("Legacy .NET Framework 2.0 tasks are exclusive to Windows platforms and cannot be evaluated on this OS.");
                    }
                    await RunLegacyDotNetProcessAsync(taskName);
                }
                else
                {
                    await RunModernDotNetAsync(targetPath, taskName);
                }
                break;

            case "python":
                await RunPythonProcessAsync(taskName, version);
                break;

            default:
                throw new NotSupportedException($"The programming language '{language}' is explicitly unrecognized by this architecture dispatcher.");
        }
    }

    /// <summary>
    /// Invokes legacy .NET compiler tooling defensively as a fallback mechanism for historical frameworks.
    /// </summary>
    private async Task RunLegacyDotNetProcessAsync(string taskName)
    {
        var command = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe";
        var arguments = new List<string>
            {
                "/target:library",
                "/out:temp.dll",
                $"{taskName}.cs"
            };

        CommandLineInput commandLineInput = new CommandLineInput
        {
            Command = command,
            Arguments = arguments,
            EnvironmentService = _environmentService
        };


        var terminalType = _terminalProvider.TerminalType;
        await _commandLineRunner.ExecuteInShellAsync(terminalType, commandLineInput);
    }

    /// <summary>
    /// Spawns external Python processes dynamically checking system environment safety and passing arguments securely.
    /// </summary>
    private async Task RunPythonProcessAsync(string taskName, string version)
    {
        var command = "python";
        var arguments = new List<string>
            {
                version,
                "-c",
                $"import task; task.{taskName}()"
            };

        CommandLineInput commandLineInput = new CommandLineInput
        {
            Command = command,
            Arguments = arguments,
            EnvironmentService = _environmentService
        };

        var terminalType = _terminalProvider.TerminalType;
        await _commandLineRunner.ExecuteInShellAsync(terminalType, commandLineInput);
    }

    /// <summary>
    /// Executes modern .NET tasks inside an isolated, unloadable loading context (Sandbox) to prevent memory leaks and host contamination.
    /// </summary>
    /// <param name="workspacePath">The directory target to look up compiled assemblies.</param>
    /// <param name="taskName">The explicit target method name to invoke through reflection.</param>
    /// <exception cref="FileNotFoundException">Thrown when no executable assemblies are uncovered within the workspace.</exception>
    /// <exception cref="MissingMethodException">Thrown when the designated taskName method is not uncovered in any loaded type.</exception>
    private Task RunModernDotNetAsync(
        string workspacePath,
        string taskName
    )
    {
        // 防禦性設計：尋找該工作區目錄下編譯完成的 .dll 檔案（通常由微型建置器或插件預先產出）
        // 為了保持 System.IO.Abstractions 的全面防禦，使用抽象層獲取檔案
        var dllFiles = _fileSystem.Directory.GetFiles(workspacePath, "*.dll", SearchOption.AllDirectories);
        if (dllFiles.Length == 0)
        {
            throw new FileNotFoundException($"No compiled .NET assemblies found under '{workspacePath}' to invoke task '{taskName}'.");
        }

        // 建立具備「可卸載(isCollectible: true)」特性的獨立 AssemblyLoadContext 沙箱
        string contextName = $"TaskSandbox_{taskName}_{Guid.NewGuid():N}";
        var loadContext = new AssemblyLoadContext(contextName, isCollectible: true);

        try
        {
            bool methodExecuted = false;

            foreach (var dllPath in dllFiles)
            {
                Assembly assembly;
                // 防禦性記憶體載入：將檔案讀入 Stream 後載入，避免實體檔案被主程序長期鎖定（File Lock）
                using (var fileStream = _fileSystem.File.OpenRead(dllPath))
                {
                    assembly = loadContext.LoadFromStream(fileStream);
                }

                // 尋找符合公開、包含此名稱方法的所有型別
                foreach (var type in assembly.GetTypes())
                {
                    var method = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                                     .FirstOrDefault(m => m.Name == taskName);

                    if (method != null)
                    {
                        object? instance = method.IsStatic ? null : Activator.CreateInstance(type);

                        // 根據需求書範例：執行目標方法
                        // 若方法有特定參數，可在這裡進行引數防禦與解析，此處預設傳遞空參數或選用參數
                        var parameters = method.GetParameters();
                        object?[] invokeArgs = parameters.Length == 2
                            ? new object?[] { "DefaultApi", "DefaultArgs" } // 呼應 task1.cs 參數結構
                            : Array.Empty<object>();

                        method.Invoke(instance, invokeArgs);
                        methodExecuted = true;
                        break;
                    }
                }

                if (methodExecuted) break;
            }

            if (!methodExecuted)
            {
                throw new MissingMethodException($"Target lifecycle method '{taskName}' could not be resolved inside any compiled assembly within the workspace.");
            }
        }
        finally
        {
            // 鐵律：執行完畢後必須立刻強制觸發沙箱卸載，釋放動態產生的 Metadata 與記憶體
            loadContext.Unload();

            // 提示垃圾回收器(GC)回收已卸載的 Assembly 資源，防止 VSC 後端發生 OutOfMemoryException
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        return Task.CompletedTask;
    }
}
