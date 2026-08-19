namespace RegistryUtilityServices;

/// <summary>
/// Abstraction for registry operations to ensure testability.
/// </summary>
public interface IRegistryService
{
    /// <summary>
    /// Retrieves a value from the registry key at the specified path.
    /// </summary>
    string? GetValue(string keyPath, string valueName);
}
