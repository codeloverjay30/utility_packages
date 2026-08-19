using System.IO.Abstractions;

namespace GlobPatternUtilityServices.Abstractions;

/// <summary>
/// Defines the contract for the .gitignore-like glob pattern matcher.
/// </summary>
public interface IGlobPatternMatcher
{
    /// <summary>
    /// Processes the directory and applies actions based on the registered rules.
    /// </summary>
    /// <param name="rootDirectory">The root directory to start scanning.</param>
    void ProcessDirectory(IDirectoryInfo rootDirectory);
}
    
