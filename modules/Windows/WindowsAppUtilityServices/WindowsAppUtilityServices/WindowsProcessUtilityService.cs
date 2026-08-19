using System;
using System.Diagnostics.Abstractions;

namespace WindowsAppUtilityServices.Diagnostics;

/// <summary>
/// Provides enterprise-grade defensive process diagnostics and lifecycle handling using System.Diagnostics.Abstractions.
/// </summary>
public class WindowsProcessUtilityService : IProcessUtilityService
{
    private readonly IProcessFactory _processFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsProcessUtilityService"/> class.
    /// </summary>
    /// <param name="processFactory">factory of <see cref="global::System.Diagnostics.Abstractions.IProcess"/></param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="processFactory"/> is specified as null</exception>
    public WindowsProcessUtilityService(
        IProcessFactory processFactory
    )
    {
        ArgumentNullException.ThrowIfNull(processFactory, nameof(processFactory));

        _processFactory = processFactory;
    }

    /// <inheritdoc cref="global::WindowsAppUtilityServices.Diagnostics.IWindowsProcessUtilityService.SafeKillAndExit(IProcess)"/>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="process"/> is specified as null</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when termination sequence fails under tight security constraints.</exception>
    public void SafeKillAndExit(IProcess process)
    {
        ArgumentNullException.ThrowIfNull(process);

        try
        {
            if (!process.HasExited)
            {
                process.Kill();
                // 防禦性設計：加入超時機制，防止底層程序卡死導致主執行緒無限期掛起
                process.WaitForExit(5000);
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            // 防禦性攔截：將作業系統底層非預期異常（如 Win32Exception 拒絕存取）轉譯為高階商業邏輯異常
            throw new InvalidOperationException("Defensive killing sequence aborted due to internal OS constraints.", ex);
        }
    }

    /// <inheritdoc cref="global::WindowsAppUtilityServices.Diagnostics.IWindowsProcessUtilityService.SafeKillAndExit(IProcess[])"/>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="processes"/> is specified as null</exception>
    public void SafeKillAndExit(IProcess[] processes)
    {
        ArgumentNullException.ThrowIfNull(processes,nameof(processes));
        if (processes.Any())
        {
            foreach (var p in processes)
            {
                SafeKillAndExit(p);
            }
        }
    }
    
    /// <inheritdoc cref="global::WindowsAppUtilityServices.Diagnostics.IWindowsProcessUtilityService.SafeKillAndExit(string)"/>
    public void SafeKillAndExit(string proccessName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(proccessName);

        var processes = _processFactory.GetProcessesByName(proccessName);
        
        SafeKillAndExit(processes);
    }
}