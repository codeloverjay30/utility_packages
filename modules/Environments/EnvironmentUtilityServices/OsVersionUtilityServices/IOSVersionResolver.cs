using System.Runtime.InteropServices;

namespace OsVersionUtilityServices;

/// <summary>
/// Defines the capability to resolve platform-specific version strings into semantic versions.
/// </summary>
public interface IOSVersionResolver
{
    /// <summary>
    /// Gets the priority of the resolver. Higher values indicate higher priority when multiple resolvers can handle the same platform.
    /// </summary>
    int Priority { get; }
    /// <summary>
    /// Determines if the resolver can handle the specified platform.
    /// </summary>
    /// <param name="platform">platform</param>
    /// <returns></returns>
    bool CanHandle(OSPlatform platform);
    /// <summary>
    /// Resolves the OS version from the provided OS description string.
    /// </summary>
    /// <param name="osDescription">OS description string</param>
    /// <returns></returns>
    Version Resolve(string osDescription);
}