namespace NetSdkInfoUtilityServices;

public interface INetSdkEnvironmentVariablesUtilityService
{
    string GetMSBuildSDKsPath();
    string GetMSBuildExtensionsPath();
}
