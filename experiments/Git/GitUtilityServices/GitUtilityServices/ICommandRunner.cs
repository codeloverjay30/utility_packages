using System.ComponentModel;
using CustomDataAnnotations.Maintenance;

namespace GitUtilityServices;

public interface ICommandRunner
{
    [Obsolete("Use Async version instead")]
    [TechnicalDebt(CategoryType.MockingIssue | CategoryType.UnitTestIssue,"ExecuteGitCommandAsync(string workingDir, string command)")]
    void ExecuteGitCommand(string workingDir, string command);
    Task ExecuteGitCommandAsync(string workingDir, string command);
}
