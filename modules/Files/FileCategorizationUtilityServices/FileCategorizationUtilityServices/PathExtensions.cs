using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace FileCategorizationUtilityServices;

/// <summary>
/// Provides extension methods for path and string manipulations.
/// </summary>
public static class PathExtensions
{
    /// <summary>
    /// Gets the extension of the specified path string using the provided <see cref="IFileSystem"/>.
    /// </summary>
    /// <param name="path">The path string from which to get the extension.</param>
    /// <param name="fileSystem">The file system abstraction to use. If null, a default <see cref="FileSystem"/> will be used.</param>
    /// <returns>The extension of the specified path (including the period "."), or null, or <see cref="string.Empty"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
    public static string GetFileExtension(
        this string path,
        IFileSystem fileSystem = null
    )
    {
        ArgumentNullException.ThrowIfNull(path);
        var activeFileSystem = fileSystem ?? new FileSystem();
        return activeFileSystem.Path.GetExtension(path);
    }

    /// <summary>
    /// Determines whether the specified string is contained within the <see cref="HashSet{T}"/>.
    /// </summary>
    /// <param name="value">The string to search for.</param>
    /// <param name="values">The collection of strings to search within.</param>
    /// <returns><c>true</c> if the value is found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    public static bool IsOneOf(
        this string value,
        HashSet<string> values
    )
    {
        ArgumentNullException.ThrowIfNull(values);
        return value != null && values.Contains(value);
    }
}
