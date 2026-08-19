namespace SymbolicLinkUtilityServices;

/// <summary>
/// Configures and builds <seealso cref="global::SymbolicLinkUtilityServices.SymbolicLinkOptions"/>
/// </summary>
public sealed class SymbolicLinkOptionsBuilder
{
    private readonly SymbolicLinkOptions _options = new();

    public SymbolicLinkOptionsBuilder(
        string linkPath,
        string targetPath
    )
    {
        _options.LinkPath = linkPath;
        _options.TargetPath = targetPath;
    }

    /// <summary>
    /// Configures <seealso cref="global::SymbolicLinkUtilityServices.SymbolicLinkOptions.EnsureTargetExists"/>
    /// </summary>
    /// <returns></returns>
    public SymbolicLinkOptionsBuilder CheckTargetExists()
    {
        _options.EnsureTargetExists = true;
        return this;
    }

    /// <summary>
    /// Whether enable <seealso cref="global::SymbolicLinkUtilityServices.SymbolicLinkOptions.EnsureSourceIsLink"/>, or not
    /// </summary>
    /// <param name="enable">true: enable, false: disable</param>
    /// <returns></returns>
    public SymbolicLinkOptionsBuilder EnsureSourceIsLink(bool enable = true)
    {
        _options.EnsureSourceIsLink = enable;
        return this;
    }

    /// <summary>
    /// Configures <seealso cref="global::SymbolicLinkUtilityServices.SymbolicLinkOptions.LockObject"/>
    /// </summary>
    /// <param name="lockObject"><seealso cref="global::SymbolicLinkUtilityServices.SymbolicLinkOptions.LockObject"/></param>
    /// <returns></returns>
    public SymbolicLinkOptionsBuilder WithLock(object lockObject)
    {
        _options.LockObject = lockObject;
        return this;
    }

    /// <summary>
    /// Build <seealso cref="global::SymbolicLinkUtilityServices.SymbolicLinkOptions"/>
    /// </summary>
    /// <returns></returns>
    public SymbolicLinkOptions Build() => _options;

    /// <summary>
    /// Static factory method to create a builder with strict mode
    /// </summary>
    /// <param name="linkPath"></param>
    /// <param name="targetPath"></param>
    /// <returns></returns>
    public static SymbolicLinkOptionsBuilder CreateStrict(string linkPath, string targetPath)
    {
        return new SymbolicLinkOptionsBuilder(linkPath, targetPath)
            .CheckTargetExists()
            .EnsureSourceIsLink(true);
    }

    /// <summary>
    /// Static factory method to create a builder without strict mode
    /// </summary>
    /// <param name="linkPath"></param>
    /// <param name="targetPath"></param>
    /// <returns></returns>
    public static SymbolicLinkOptionsBuilder CreateLax(string linkPath, string targetPath)
    {
        return new SymbolicLinkOptionsBuilder(linkPath, targetPath)
            .EnsureSourceIsLink(false);
    }
}
