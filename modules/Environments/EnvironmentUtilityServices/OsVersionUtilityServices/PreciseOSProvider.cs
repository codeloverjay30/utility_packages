using System.Runtime.InteropServices;

namespace OsVersionUtilityServices;

/// <summary>
/// Central service to provide high-precision OS version data.
/// </summary>
public class PreciseOSProvider
{
    private readonly IEnumerable<IOSVersionResolver> _resolvers;

    public PreciseOSProvider(IEnumerable<IOSVersionResolver> resolvers)
    {
        // Sort by Priority descending to ensure the most specific resolver runs first
        _resolvers = resolvers.OrderByDescending(r => r.Priority).ToList();
    }

    public Version GetPreciseVersion()
    {
        var description = RuntimeInformation.OSDescription;
        var platform = GetCurrentPlatform();

        var resolver = _resolvers.FirstOrDefault(r => r.CanHandle(platform))
            ?? throw new NotSupportedException("No resolver found for current OS.");

        return resolver.Resolve(description);
    }

    private static OSPlatform GetCurrentPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
        {
            return OSPlatform.Windows;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) 
        {
            return OSPlatform.OSX;
        }
        return OSPlatform.Linux;
    }
}
