namespace AntiHijackUtilityServices.Abstractions;


/// <summary>
/// Defines the behavior for environment safety and integrity status checkers.
/// </summary>
public interface ISafetySensor
{
    /// <summary>
    /// Gets the name of the safety sensor.
    /// </summary>
    string SensorName { get; }

    /// <summary>
    /// Executes the detection mechanism to verify if the current environment is threatened.
    /// </summary>
    /// <returns>True if a threat (e.g., debugger or virtual machine) is detected; otherwise, false.</returns>
    bool IsThreatDetected();
}