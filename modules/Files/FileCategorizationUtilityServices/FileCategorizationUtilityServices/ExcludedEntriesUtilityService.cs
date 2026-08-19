using System;
using System.Collections.Generic;

namespace FileCategorizationUtilityServices;

/// <summary>
/// Implementation of <see cref="IExcludedEntriesUtilityService"/> that provides default excluded paths and folder names.
/// </summary>
public class ExcludedEntriesUtilityService : IExcludedEntriesUtilityService
{
    private static readonly HashSet<string> _excludedPath = new(StringComparer.OrdinalIgnoreCase)
    {
        @"\bin\",
        @"\obj\",
        @"\.vs\",
        @"\.vshistory\",
        @"\.git\",
        @"\.github\",
    };

    private static readonly HashSet<string> _excludedFolderName = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin",
        "obj",
        ".vs",
        ".vshistory",
        ".git",
        ".github",
    };

    /// <inheritdoc/>
    public bool IsExcludedPath(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        return path.IsOneOf(_excludedPath);
    }

    /// <inheritdoc/>
    public bool IsExcludedFolderName(string folderName)
    {
        ArgumentException.ThrowIfNullOrEmpty(folderName);
        return folderName.IsOneOf(_excludedFolderName);
    }
}
