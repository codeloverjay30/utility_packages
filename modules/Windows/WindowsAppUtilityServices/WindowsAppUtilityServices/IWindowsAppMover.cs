using CommonModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsAppUtilityServices;

public interface IWindowsAppMover
{
    /// <summary>
    /// Move one app with <paramref name="appSetting"/> settings according to <paramref name="options"/> option.
    /// </summary>
    /// <param name="appSetting">app settings</param>
    /// <param name="options"><see cref="global::WindowsAppUtilityServices.MoveDirectoryOptions"/></param>
    /// <returns></returns>
    public StatusJsonModel MoveOneApp(
        AppSettings appSetting,
        MoveDirectoryOptions option = MoveDirectoryOptions.Default
    );
}
    
