using System.IO.Abstractions;

namespace GlobPatternUtilityServices.Abstractions;

/// <summary>
/// Defines a strategy for executing an action on a matched file or directory.
/// </summary>
public interface IMatchActionStrategy
{
    /// <summary>
    /// Executes the action on the specified file system entry.
    /// </summary>
    /// <param name="fileInfo">The file information of the matched item.</param>
    void Execute(IFileInfo fileInfo);
}
    