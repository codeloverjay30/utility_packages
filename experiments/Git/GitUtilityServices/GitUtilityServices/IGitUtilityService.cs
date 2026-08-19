using CustomDataAnnotations.Maintenance;

namespace GitUtilityServices;

public interface IGitUtilityService
{
    [Obsolete("Use Async version instead")]
    [TechnicalDebt(CategoryType.MockingIssue | CategoryType.UnitTestIssue,"CheckModulesAsync(string rootPath)")]
    void CheckModules(string rootPath);

    Task CheckModulesAsync(string rootPath);

    [Obsolete("Use Async version instead")]
    [TechnicalDebt(CategoryType.MockingIssue | CategoryType.UnitTestIssue,"UpdateAndCommitAsync(string solutionPath,string message)")]
    void UpdateAndCommit(
        string solutionPath,
        string message
    );

    Task UpdateAndCommitAsync(
        string solutionPath,
        string message
    );
}
