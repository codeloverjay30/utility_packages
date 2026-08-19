using System.Runtime.InteropServices;
using OsVersionUtilityServices;
using RegistryUtilityServices;

public class WindowsVersionResolver : IOSVersionResolver
{
    private readonly IRegistryService _registryService;
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
    private const string BuildValueName = "CurrentBuildNumber";

    public int Priority => (int)PlatformPriorityOptions.High;


    public WindowsVersionResolver(IRegistryService registryService) 
        => _registryService = registryService;

    public bool CanHandle(OSPlatform platform) => platform == OSPlatform.Windows;

    public Version Resolve(string osDescription)
    {
        var buildNumber = _registryService.GetValue(RegistryKeyPath, BuildValueName);

        if (int.TryParse(buildNumber, out int build))
        {
            var osVersion = Environment.OSVersion.Version;
            return new Version(osVersion.Major, osVersion.Minor, build);
        }

        return Environment.OSVersion.Version;
    }
}