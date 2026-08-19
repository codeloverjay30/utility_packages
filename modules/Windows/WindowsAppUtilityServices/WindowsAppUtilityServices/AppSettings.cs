using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsAppUtilityServices;

/// <summary>
/// App Settings
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Process name of the app (or directory) (can be seen in Task Manager)
    /// </summary>
    public required string ProcessName { get; init; }

    /// <summary>
    /// The path of the app or the origin directory
    /// </summary>
    public required string SourcePath { get; init; }

    /// <summary>
    /// The target path of the app (or directory <see cref="global::WindowsAppUtilityServices.AppSettings.SourcePath") will be moved to.
    /// </summary>
    public required string TargetPath { get; init; }
}

