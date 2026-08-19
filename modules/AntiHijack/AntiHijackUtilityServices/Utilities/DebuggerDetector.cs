using System.Diagnostics;
using System.Runtime.InteropServices;

using AntiHijackUtilityServices.Abstractions;
using EnvironmentUtilityServices;

namespace AntiHijackUtilityService.Sensors;

/// <summary>
/// Detects managed or native debuggers attached to the application context.
/// </summary>
public class DebuggerDetector : ISafetySensor
{
    public string SensorName => "ActiveDebuggerDetector";

    private readonly IPlatformService _platformService;

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern bool IsDebuggerPresent();

    public DebuggerDetector(
        IPlatformService platformService
    )
    {
        ArgumentNullException.ThrowIfNull(platformService, nameof(platformService));
        _platformService = platformService;
    }
    /// <summary>
    /// Checks both the .NET managed diagnostics state and Win32 kernel structures.
    /// </summary>
    public bool IsThreatDetected()
    {
        if (!_platformService.IsWindows())
        {
            throw new PlatformNotSupportedException("DebuggerDetector is only supported on Windows platforms.");
        }

        return Debugger.IsAttached || IsDebuggerPresent();
    }
}