using System;
using System.Runtime.InteropServices;

namespace EnvironmentUtilityServices;

/// <summary>
/// Implements the environment inspection service with state freezing at initialization to prevent runtime tampering.
/// </summary>
public class EnvironmentService : IEnvironmentService
{
    private readonly bool _isWindows;
    private readonly bool _isLinux;
    private readonly bool _isMacOS;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentService"/> class using standard runtime information.
    /// </summary>
    public EnvironmentService() : this(RuntimeInformation.IsOSPlatform)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentService"/> class with a custom detector, primarily used for defensive isolation in unit tests.
    /// </summary>
    /// <param name="osCheck">The delegate to evaluate the target <see cref="OSPlatform"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="osCheck"/> is null.</exception>
    public EnvironmentService(Func<OSPlatform, bool> osCheck)
    {
        ArgumentNullException.ThrowIfNull(osCheck);

        // Defensive Guard: Snapshots are taken immediately at initialization time to ensure immutability and high performance.
        _isWindows = osCheck(OSPlatform.Windows);
        _isLinux = osCheck(OSPlatform.Linux);
        _isMacOS = osCheck(OSPlatform.OSX);
    }

    /// <summary>
    /// Check it is in Windows
    /// </summary>
    /// <returns></returns>
    public bool IsWindows() => _isWindows;

    /// <summary>
    /// Check it is in Linux
    /// </summary>
    /// <returns></returns>
    public bool IsLinux() => _isLinux;

    /// <summary>
    /// Check it is in Mac Os.
    /// </summary>
    /// <returns></returns>
    public bool IsMacOS() => _isMacOS;

    /// <summary>
    /// Defensively checks if a path matches Windows Uniform Naming Convention (UNC) formats,
    /// supporting URI schemas, standard prefixes, and long-path device namespaces.
    /// </summary>
    /// <param name="path">The target path string to evaluate.</param>
    /// <returns>True if the path represents a Windows UNC network location; otherwise, false.</returns>
    public bool IsUncPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        // 防禦性快取閘道：捕捉 Win32 Device Namespace 長路徑格式 (e.g., \\?\UNC\server\share)
        if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // 使用 Uri 精準提取 Scheme 與 IsUnc 特徵
        if (Uri.TryCreate(path, UriKind.Absolute, out Uri? uri))
        {
            return uri.IsUnc;
        }

        // 最終低消耗字串降級防線
        return path.StartsWith(@"\\", StringComparison.Ordinal) || path.StartsWith(@"//", StringComparison.Ordinal);
    }
        
}