using System.Diagnostics;
using System.IO.Abstractions;

namespace NetRuntimeUtilityServices;

/// <summary>
/// Provides concrete implementation for environment detection with defensive checks.
/// </summary>
public class RuntimeEnvironmentDetector : IRuntimeEnvironmentDetector
{
    private readonly IFileSystem _fileSystem;
    private readonly IEnvironmentProvider _environmentProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeEnvironmentDetector"/> class.
    /// </summary>
    /// <param name="fileSystem">The abstraction of the file system to prevent direct IO coupling.</param>
    /// <param name="environmentProvider">The abstraction of the <see cref="global::System.Environment"/> system to prevent direct IO coupling.</param>
    public RuntimeEnvironmentDetector(
        IFileSystem fileSystem,
        IEnvironmentProvider environmentProvider
    )
    {
        ArgumentNullException.ThrowIfNull(fileSystem, nameof(fileSystem));
        ArgumentNullException.ThrowIfNull(environmentProvider, nameof(environmentProvider));

        _fileSystem = fileSystem;
        _environmentProvider = environmentProvider;
    }

    /// <summary>
    /// Evaluates if the debugger is attached specifically via VS Code's testing panel.
    /// </summary>
    public bool IsVsCodeTestDebugging()
    {
        // 1. First-line defense: Is a debugger even attached?
        if (!Debugger.IsAttached)
        {
            return false;
        }

        try
        {
            // 2. Second-line defense: Verify VS Code specific environment indicators
            // VS Code Test Runner Extensions typically inject specific environment variables or arguments
            // string? vscPid = Environment.GetEnvironmentVariable("VSCODE_PID");
            // string? vscCwd = Environment.GetEnvironmentVariable("VSCODE_CWD");

            string? vscPid = _environmentProvider.GetEnvironmentVariable("VSCODE_PID");
            string? vscCwd = _environmentProvider.GetEnvironmentVariable("VSCODE_CWD");
            
            // Check if the parent process or environment is initiated by VS Code
            bool isFromVsCode = !string.IsNullOrEmpty(vscPid) || !string.IsNullOrEmpty(vscCwd);

            // 3. Third-line defense: Check for VSTest/Microsoft.TestHost context
            string currentProcessName = Process.GetCurrentProcess().ProcessName;
            bool isTestHost = currentProcessName.Contains("testhost", StringComparison.OrdinalIgnoreCase);

            return isFromVsCode && isTestHost;
        }
        catch (Exception ex) when (ex is NotSupportedException || ex is InvalidOperationException)
        {
            // Fail-safe: Under extreme sandbox constraints where Process cannot be accessed, 
            // fallback gracefully to basic debugger check.
            return Debugger.IsAttached;
        }
    }
}
