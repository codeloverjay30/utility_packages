namespace NetRuntimeUtilityServices;

/// <summary>
/// Defines capabilities for detecting the current runtime execution environment.
/// </summary>
public interface IRuntimeEnvironmentDetector
{
    /// <summary>
    /// Determines whether the current process is running under a debugger within a VS Code testing session.
    /// </summary>
    /// <returns><c>true</c> if running in VS Code test debug mode; otherwise, <c>false</c>.</returns>
    bool IsVsCodeTestDebugging();
}