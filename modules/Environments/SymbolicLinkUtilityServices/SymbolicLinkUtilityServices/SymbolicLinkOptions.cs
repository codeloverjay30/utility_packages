namespace SymbolicLinkUtilityServices;

/// <summary>
/// Options used when updating symbolic links
/// </summary>
public sealed class SymbolicLinkOptions
{
    /// <summary>
    /// The path of symbolic link
    /// </summary>
    public string LinkPath { get; set; } = string.Empty;

    /// <summary>
    /// The symbolic link reparses to
    /// </summary>
    public string TargetPath { get; set; } = string.Empty;

    /// <summary>
    /// To ensure the <see cref="global::SymbolicLinkUtilityServices.SymbolicLinkOptions.TargetPath"/> exists, or not
    /// </summary>
    public bool EnsureTargetExists { get; set; } = false;

    /// <summary>
    /// To ensure the <see cref="global::SymbolicLinkUtilityServices.SymbolicLinkOptions.LinkPath"/> is a symbolic link, or not 
    /// </summary>
    public bool EnsureSourceIsLink { get; set; } = true; // 預設開啟，防誤刪安全第一

    /// <summary>
    /// A lock.
    /// When it is not specified, it uses default value null.
    /// When it is set to null, no lock lockes during updating the symbolic link.
    /// </summary>
    public object? LockObject { get; set; } = null;
}
