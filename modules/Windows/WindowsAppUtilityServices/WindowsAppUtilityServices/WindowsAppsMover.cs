using CommonModels;
using Microsoft.Extensions.Options;
using WindowsAppUtilityServices;

namespace WindowsAppUtilityServices;

public class WindowsAppsMover : IWindowsAppsMover
{
    private readonly IWindowsAppMover _mover;
    private readonly List<AppSettings> _settings;
    public WindowsAppsMover(
        IWindowsAppMover mover,
        IOptions<List<AppSettings>> options
    )
    {
        _mover = mover;
        _settings = options.Value;
    }

    /// <inheritdoc cref="global::WindowsAppUtilityServices.IWindowsAppsMover.MoveManyApps(MoveDirectoryOptions)"/>    
    public StatusJsonModels MoveManyApps(
        MoveDirectoryOptions options = MoveDirectoryOptions.Ren
    )
    {
        var statusJsonModels = new StatusJsonModels();
        foreach (var appSetting in _settings)
        {
            // 直接呼叫注入的服務，不再手動 new 
            statusJsonModels.StatusList.Add(_mover.MoveOneApp(appSetting, options));
        }
        return statusJsonModels;
    }
}