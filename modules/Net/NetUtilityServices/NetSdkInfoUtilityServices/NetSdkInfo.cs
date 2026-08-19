namespace NetSdkInfoUtilityServices;

public record class NetSdkInfo
{
    public required string MSBuildSDKsPath { get; init; }
    public required string MSBuildExtensionsPath { get; init; }
}
