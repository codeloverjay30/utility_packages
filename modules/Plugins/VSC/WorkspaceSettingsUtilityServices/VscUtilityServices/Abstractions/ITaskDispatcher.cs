namespace WorkspaceUtility.Core.Services;

public interface ITaskDispatcher
{
    Task ExecuteTaskDefensivelyAsync(
            string targetPath,
            string taskName,
            string language,
            string version
    );
}
