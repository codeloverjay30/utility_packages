using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OsVersionUtilityServices;

/// <summary>
/// Handles version parsing for macOS by extracting version numbers from the OS description string.
/// Implements defensive parsing to handle potential variations in OS string formats.
/// </summary>
public class MacOSVersionResolver : IOSVersionResolver
{
    public int Priority => (int)PlatformPriorityOptions.High;


    /// <inheritdoc/>
    public bool CanHandle(OSPlatform platform) => platform == OSPlatform.OSX;

    /// <summary>
    /// Resolves the macOS version using high-performance Span-based parsing of the OS description.
    /// </summary>
    /// <param name="osDescription">The raw OS description string provided by RuntimeInformation.</param>
    /// <returns>A Version object representing the extracted OS version.</returns>
    /// <exception cref="FormatException">Thrown when the OS version format is unrecognizable.</exception>
    public Version Resolve(string osDescription)
    {
        if (string.IsNullOrWhiteSpace(osDescription))
        {
            throw new ArgumentException("OS description cannot be null or empty.", nameof(osDescription));
        }

        // Example input: "Darwin 23.4.0 Darwin Kernel Version 23.4.0: ..."
        ReadOnlySpan<char> span = osDescription.AsSpan();

        // Locate the start of the version string (e.g., after "Darwin ")
        int startIndex = span.IndexOf("Darwin ") + 7;
        if (startIndex < 7) 
        {
            return Environment.OSVersion.Version;
        }

        ReadOnlySpan<char> versionPart = span.Slice(startIndex);
        int endIndex = versionPart.IndexOf(' ');
        
        if (endIndex != -1)
        {
            versionPart = versionPart.Slice(0, endIndex);
        }

        // Parse Major.Minor.Build
        return TryParseVersion(versionPart, out var version) 
            ? version 
            : Environment.OSVersion.Version;
    }

    private static bool TryParseVersion(ReadOnlySpan<char> versionSpan, out Version version)
    {
        version = new Version(0, 0);
        try
        {
            string versionStr = versionSpan.ToString();
            string[] parts = versionStr.Split('.');
            
            int major = parts.Length > 0 ? int.Parse(parts[0]) : 0;
            int minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
            int build = parts.Length > 2 ? int.Parse(parts[2]) : 0;

            version = new Version(major, minor, build);
            return true;
        }
        catch
        {
            return false;
        }
    }
}