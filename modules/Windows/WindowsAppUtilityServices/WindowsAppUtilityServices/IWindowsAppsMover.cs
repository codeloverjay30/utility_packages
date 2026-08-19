using CommonModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsAppUtilityServices;

public interface IWindowsAppsMover
{
    /// <summary>
    /// Move lots of apps with <see cref="global::WindowsAppsMover._settings"/> settings according to <paramref name="options"/> option.
    /// </summary>
    StatusJsonModels MoveManyApps(
        MoveDirectoryOptions options = MoveDirectoryOptions.Ren
    );
}
    

