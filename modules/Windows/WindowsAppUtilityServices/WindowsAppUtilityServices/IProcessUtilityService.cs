using System.Diagnostics.Abstractions;

namespace WindowsAppUtilityServices.Diagnostics;

/// <summary>
/// Defines extended process management behaviors tailored for Windows application migration,
/// built on top of the standard <see cref="global::System.Diagnostics.Abstractions.IProcess"/> abstraction.
/// </summary>
public interface IProcessUtilityService
{
    /// <summary>
    /// Safely terminates the specified process and guarantees execution control return within defensive boundaries.
    /// </summary>
    /// <param name="process">The target abstracted process to terminate.</param>
    /// <exception cref="System.InvalidOperationException">Thrown when termination sequence fails under tight security constraints.</exception>
    void SafeKillAndExit(IProcess process);
    
    /// <summary>
    /// Overload of <see cref="global::WindowsAppUtilityServices.Diagnostics.IProcessUtilityService.SafeKillAndExit(IProcess)" />
    /// </summary>
    /// <param name="proccessName"></param>
    void SafeKillAndExit(IProcess[] proccesses);

    /// <summary>
    /// Overload of <see cref="global::WindowsAppUtilityServices.Diagnostics.IProcessUtilityService.SafeKillAndExit(IProcess)" />
    /// </summary>
    /// <param name="proccessName"></param>
    void SafeKillAndExit(string proccessName);
}