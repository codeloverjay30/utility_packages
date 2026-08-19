namespace EnvironmentUtilityServices;

/// <summary>
/// A unified platform service facade that bundles environment state and OS utilities for developer convenience.
/// </summary>
public interface IPlatformService : IEnvironmentService, IOsUtilityService
{
    // 這裡不需要寫任何方法，它單純繼承兩個介面，組合出新契約
}

