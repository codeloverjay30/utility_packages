namespace FileExplorerUtilityServices;

/// <summary>
/// Defines a contract for listening to BitLocker and volume storage events on the system.
/// </summary>
public interface IBitLockerStorageEventListener
{
    /// <summary>
    /// Starts listening to volume state modification events asynchronously.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Thrown when executed on a non-Windows platform.</exception>
    void StartListening();

    /// <summary>
    /// Stops listening to volume state modification events and releases allocated resources.
    /// </summary>
    void StopListening();
}