namespace EnvironmentUtilityServices;

/// <summary>
/// Defensive implementation of the unified platform facade.
/// </summary>
public class PlatformService : IPlatformService
{
    private readonly IEnvironmentService _env;
    private readonly IOsUtilityService _util;

    public PlatformService(
        IEnvironmentService env,
        IOsUtilityService util
    )
    {
        ArgumentNullException.ThrowIfNull(env);
        ArgumentNullException.ThrowIfNull(util);
        _env = env;
        _util = util;
    }

    // 顯式或隱式轉發實作，完美的防禦性包裝（Forwarding / Delegation）
    public bool IsWindows() => _env.IsWindows();
    public bool IsLinux() => _env.IsLinux();
    public bool IsMacOS() => _env.IsMacOS();

    public bool IsUncPath(string targetPath) => _env.IsUncPath(targetPath);

    public StringComparison GetComparison() => _util.GetComparison();
    public string NormalizePath(string path) => _util.NormalizePath(path);
}