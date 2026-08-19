using System;
using System.Runtime.InteropServices;
using AntiHijackUtilityServices.Abstractions;
using EnvironmentUtilityServices;

namespace AntiHijackUtilityServices.Core;

/// <summary>
/// Validates the execution environment platform to ensure compatibility and stability.
/// </summary>
public class OSPlatformValidator:IOSPlatformValidator
{
    private readonly IPlatformService _platformService;
    public OSPlatformValidator(
        IPlatformService platformService
    )
    {
        ArgumentNullException.ThrowIfNull(platformService, nameof(platformService));
        _platformService = platformService;
    }
    /// <summary>
    /// Validates whether the application is running on the supported Windows operating system.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Thrown when the operating system is not Windows.</exception>
    public void ValidateOS()
    {
        if (!_platformService.IsWindows())
        {
            throw new PlatformNotSupportedException("This application only supports the Windows operating system.");
        }
    }
}