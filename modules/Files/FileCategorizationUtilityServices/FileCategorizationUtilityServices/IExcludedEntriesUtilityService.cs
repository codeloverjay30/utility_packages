using System;

namespace FileCategorizationUtilityServices;

/// <summary>
/// Provides methods to determine which paths or folder names should be excluded.
/// </summary>
public interface IExcludedEntriesUtilityService
{
    /// <summary>
    /// Determines whether the specified path is in the excluded list.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><c>true</c> if the path is excluded; otherwise, <c>false</c>.</returns>
    bool IsExcludedPath(string path);

    /// <summary>
    /// Determines whether the specified folder name is in the excluded list.
    /// </summary>
    /// <param name="folderName">The folder name to check.</param>
    /// <returns><c>true</c> if the folder name is excluded; otherwise, <c>false</c>.</returns>
    bool IsExcludedFolderName(string folderName);
}
