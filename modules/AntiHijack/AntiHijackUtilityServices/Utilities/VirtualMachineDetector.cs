using System;
using System.Management;
using System.Runtime.Versioning;
using AntiHijackUtilityServices.Abstractions;
using EnvironmentUtilityServices;

namespace AntiHijackUtilityService.Sensors;

/// <summary>
/// Detects if the operational instance is running inside specific virtualization vendor layers via WMI infrastructure.
/// </summary>
[SupportedOSPlatform("windows")]
public class VirtualMachineDetector : ISafetySensor
{
    public string SensorName => "WmiVirtualMachineDetector";

    private readonly IPlatformService _platformService;

    public VirtualMachineDetector(
        IPlatformService platformService
    )
    {
        ArgumentNullException.ThrowIfNull(platformService, nameof(platformService));
        _platformService = platformService;
    }

    /// <summary>
    /// Querying WMI structures defensively with strict enumerator disposal.
    /// </summary>
    public bool IsThreatDetected()
    {
        if (!_platformService.IsWindows()) 
        {
            throw new PlatformNotSupportedException("VirtualMachineDetector is only supported on Windows platforms.");
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem");
            using var collection = searcher.Get();
            
            // 【效能重構】：手動調用並釋放列舉器，防止微軟底層 WMI 迭代過程中的 Handle 洩漏
            var enumerator = collection.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    using (ManagementBaseObject baseObject = enumerator.Current)
                    {
                        if (baseObject is not ManagementObject item) continue;

                        string manufacturer = item["Manufacturer"]?.ToString()?.ToLowerInvariant() ?? string.Empty;
                        string model = item["Model"]?.ToString()?.ToLowerInvariant() ?? string.Empty;

                        if ((manufacturer.Contains("microsoft") && model.Contains("virtual"))
                            || manufacturer.Contains("vmware")
                            || model.Contains("virtualbox"))
                        {
                            return true;
                        }
                    }
                }
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }
        catch (ManagementException)
        {
            return false;
        }

        return false;
    }
}