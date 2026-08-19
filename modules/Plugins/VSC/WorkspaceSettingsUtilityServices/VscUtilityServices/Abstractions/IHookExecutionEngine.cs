namespace VscUtilityServices.Core.Services;

public interface IHookExecutionEngine
{
    Task ProcessHookConfigurationAsync(
        string configFilePath,
        string workspaceRootPath,
        string triggeredEvent
    );
}
