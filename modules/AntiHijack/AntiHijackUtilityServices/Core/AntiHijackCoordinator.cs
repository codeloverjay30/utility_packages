

using AntiHijackUtilityServices.Abstractions;

namespace AntiHijackUtilityServices.Core;

/// <summary>
/// Orchestrates the validation pipelines and multiple tracking sensors to guarantee runtime ecosystem immunity.
/// </summary>
public class AntiHijackCoordinator : IAntiHijackCoordinator
{
    private readonly IOSPlatformValidator _platformValidator;
    private readonly IEnumerable<ISafetySensor> _sensors;

    /// <summary>
    /// Initializes a new instance of the <see cref="AntiHijackCoordinator"/> class.
    /// </summary>
    public AntiHijackCoordinator(
        IOSPlatformValidator platformValidator,
        IEnumerable<ISafetySensor> sensors
    )
    {
        ArgumentNullException.ThrowIfNull(platformValidator, nameof(platformValidator));
        ArgumentNullException.ThrowIfNull(sensors, nameof(sensors));
        _platformValidator = platformValidator;
        _sensors = sensors;
    }

    /// <summary>
    /// Runs all security verifications sequentially. Returns true if system is clean, false if any hijack signature hits.
    /// </summary>
    public bool VerifyEcosystemHealth()
    {
        // 1. Enforce core platform boundaries immediately
        _platformValidator.ValidateOS();

        // 2. Aggregate security signals through loose-coupling interfaces
        foreach (var sensor in _sensors)
        {
            if (sensor.IsThreatDetected())
            {
                return false;
            }
        }

        return true;
    }
}