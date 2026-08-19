using System;

namespace EnvironmentUtilityServices;

/// <summary>
/// Provides utility functions optimized for safe, cross-platform file system interactions and path comparisons.
/// </summary>
public interface IOsUtilityService
{
    /// <summary>
    /// Retrieves the appropriate string comparison rule for the current file system to prevent path hijacking or casing mismatches.
    /// </summary>
    /// <returns>The <see cref="StringComparison"/> suitable for path checking on the host OS.</returns>
    StringComparison GetComparison();

    /// <summary>
    /// Defensively normalizes a path string based on the current operating system's constraints.
    /// </summary>
    /// <param name="path">The raw path to look at.</param>
    /// <returns>A normalized, secure path representation.</returns>
    string NormalizePath(string path);
}