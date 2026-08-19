using EnvironmentVariables.Core;

namespace NetSdkInfoUtilityServices;

public class NetSdkEnvironmentVariablesUtilityService : INetSdkEnvironmentVariablesUtilityService
{
    private readonly IEnvironmentVariableProvider _environmentVariableProvider;
    public NetSdkEnvironmentVariablesUtilityService(
        IEnvironmentVariableProvider environmentVariableProvider
    )
    {
        ArgumentNullException.ThrowIfNull(environmentVariableProvider, nameof(environmentVariableProvider));

        _environmentVariableProvider = environmentVariableProvider;
    }

    public string GetMSBuildSDKsPath()
    {
        return _environmentVariableProvider.GetEnvironmentVariable(EnvironmentVariablesConstants.MSBuildSDKsPath);
    }

    public string GetMSBuildExtensionsPath()
    {
        return _environmentVariableProvider.GetEnvironmentVariable(EnvironmentVariablesConstants.MSBuildExtensionsPath);
    }
}
