using System.IO.Abstractions;
using CliUtilityServices;
using EnvironmentUtilityServices;
using EnvironmentVariables.Core;

namespace NetSdkInfoUtilityServices;

public class NetSdkInfoUtilityService : INetSdkInfoUtilityService
{
    private readonly IFileSystem _fileSystem;

    private readonly INetSdkEnvironmentVariablesUtilityService _netSdkEnvironmentVariablesUtilityService;

    private readonly IEnvironmentService _environmentService;
    private readonly ICliCommandExecutor _commandLineExecutor;
    public NetSdkInfoUtilityService(
        IFileSystem fileSystem,
        IEnvironmentService environmentService,
        ICliCommandExecutor commandLineExecutor,
        INetSdkEnvironmentVariablesUtilityService netSdkEnvironmentVariablesUtilityService
    )
    {
        ArgumentNullException.ThrowIfNull(fileSystem, nameof(fileSystem));
        ArgumentNullException.ThrowIfNull(environmentService, nameof(environmentService));
        ArgumentNullException.ThrowIfNull(commandLineExecutor, nameof(commandLineExecutor));
        ArgumentNullException.ThrowIfNull(netSdkEnvironmentVariablesUtilityService, nameof(netSdkEnvironmentVariablesUtilityService));

        _fileSystem = fileSystem;
        _environmentService = environmentService;
        _commandLineExecutor = commandLineExecutor;
        _netSdkEnvironmentVariablesUtilityService = netSdkEnvironmentVariablesUtilityService;
    }
    
    public NetSdkInfo GetNetSdkInfo()
    {
        var msBuildSDKsPath = _netSdkEnvironmentVariablesUtilityService.GetMSBuildSDKsPath();
        if (string.IsNullOrWhiteSpace(msBuildSDKsPath))
        {
            CommandLineInput commandLineInput = new CommandLineInput
            {
                Command = "dotnet",
                Arguments = new[] { "msbuild", $"-getProperty:{EnvironmentVariablesConstants.MSBuildSDKsPath}" },
                WorkingDirectory = _fileSystem.Directory.GetCurrentDirectory(),
                EnvironmentService = _environmentService
            };
            msBuildSDKsPath = _commandLineExecutor.ExecuteAutoDetectedAsync(commandLineInput).GetAwaiter().GetResult().StandardOutput;
        }
        return new NetSdkInfo
        {
            MSBuildSDKsPath = msBuildSDKsPath,
            MSBuildExtensionsPath = _netSdkEnvironmentVariablesUtilityService.GetMSBuildExtensionsPath()
        };
    }
}
