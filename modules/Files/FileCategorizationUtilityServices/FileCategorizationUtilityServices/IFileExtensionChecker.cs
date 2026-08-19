using System;

namespace FileCategorizationUtilityServices;

/// <summary>
/// Provides methods to check file extensions for various categories.
/// </summary>
public interface IFileExtensionChecker
{
    /// <summary>
    /// Determines whether the specified file path corresponds to a configuration file.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns><c>true</c> if the file is a configuration file; otherwise, <c>false</c>.</returns>
    bool IsConfiguration(string filePath);

    /// <summary>
    /// Determines whether the specified file path corresponds to a document file.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns><c>true</c> if the file is a document file; otherwise, <c>false</c>.</returns>
    bool IsDocument(string filePath);

    /// <summary>
    /// Determines whether the specified file path corresponds to a solution file.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns><c>true</c> if the file is a solution file; otherwise, <c>false</c>.</returns>
    bool IsSolution(string filePath);

    /// <summary>
    /// Determines whether the specified file path corresponds to a project file.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns><c>true</c> if the file is a project file; otherwise, <c>false</c>.</returns>
    bool IsProject(string filePath);

    /// <summary>
    /// Determines whether the specified file path corresponds to a programming language source file.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns><c>true</c> if the file is a programming language file; otherwise, <c>false</c>.</returns>
    bool IsProgrammingLanguage(string filePath);

    /// <summary>
    /// Determines whether the specified file path corresponds to a text file.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns><c>true</c> if the file is a text file; otherwise, <c>false</c>.</returns>
    bool IsText(string filePath);

    /// <summary>
    /// Determines whether the specified file path needs to be replaced based on its extension.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns><c>true</c> if the file needs to be replaced; otherwise, <c>false</c>.</returns>
    bool NeedsToBeReplaced(string filePath);
}
