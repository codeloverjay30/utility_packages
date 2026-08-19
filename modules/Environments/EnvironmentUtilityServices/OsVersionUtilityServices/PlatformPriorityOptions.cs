using System.ComponentModel;

namespace OsVersionUtilityServices;

/// <summary>
/// Represents the priority options for different platforms when resolving the operating system version.
/// </summary>
public enum PlatformPriorityOptions : int
{
    [Description("High priority for platform-specific resolvers. Such as Linux, Windows, and MacOS.")]

    High = 3,
    [Description("Medium priority for platform-specific resolvers. Such as Android and iOS.")]
    Medium = 2,
    [Description("Low priority for platform-specific resolvers. Such as other platforms.")]
    Low = 1
}
