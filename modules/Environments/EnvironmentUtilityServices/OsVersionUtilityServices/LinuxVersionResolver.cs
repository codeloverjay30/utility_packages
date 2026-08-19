using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;

namespace OsVersionUtilityServices;

/// <summary>
/// Resolves Linux-based OS versions by parsing standard /etc/os-release files.
/// Uses IFileSystem to ensure testability and defensive IO handling.
/// </summary>
public class LinuxVersionResolver : IOSVersionResolver
{
    private readonly IFileSystem _fileSystem;

    public int Priority => (int)PlatformPriorityOptions.High;


    public LinuxVersionResolver(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <inheritdoc/>
    public bool CanHandle(OSPlatform platform) => 
        platform == OSPlatform.Linux || platform == OSPlatform.Create("ANDROID");

    /// <summary>
    /// Parses the /etc/os-release file to extract version information.
    /// </summary>
    public Version Resolve(string osDescription)
    {
        const string path = "/etc/os-release";
        
        if (!_fileSystem.File.Exists(path))
        {
            // Fallback to basic Environment info if file is missing
            return Environment.OSVersion.Version;
        }

        var lines = _fileSystem.File.ReadAllLines(path);
        foreach (var line in lines)
        {
            if (line.StartsWith("VERSION_ID=", StringComparison.Ordinal))
            {
                var versionPart = line.Replace("VERSION_ID=", "").Trim('"');
                return TryParseVersion(versionPart);
            }
        }

        return Environment.OSVersion.Version;
    }

    private static Version TryParseVersion(string versionPart)
    {
        // Handle common formats like "22.04" or "10"
        return Version.TryParse(versionPart, out var version) 
            ? version 
            : Environment.OSVersion.Version;
    }
}