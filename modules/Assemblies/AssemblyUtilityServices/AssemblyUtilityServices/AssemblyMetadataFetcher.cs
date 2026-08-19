using System.Reflection;

namespace AssemblyUtilityServices;

/// <summary>
/// Provides strongly defined access to assembly metadata.
/// </summary>
public static class AssemblyMetadataFetcher
{
    /// <summary>
    /// Gets the informational version declared by the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <returns>The informational version.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the assembly does not declare an informational version.
    /// </exception>
    public static string GetInformationalVersion(this Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        string? informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informationalVersion))
        {
            throw new InvalidOperationException(
                $"Assembly '{assembly.FullName}' does not declare an informational version.");
        }

        return informationalVersion;
    }

    /// <summary>
    /// Attempts to get the informational version declared by the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <param name="informationalVersion">
    /// The informational version when available; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a non-empty informational version is available;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryGetInformationalVersion(
        this Assembly assembly,
        out string? informationalVersion)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informationalVersion))
        {
            informationalVersion = null;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the assembly identity version.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <returns>The assembly identity version.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the assembly identity version is unavailable.
    /// </exception>
    public static Version GetAssemblyVersion(this Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return assembly.GetName().Version
            ?? throw new InvalidOperationException(
                $"Assembly '{assembly.FullName}' does not expose an assembly identity version.");
    }

    /// <summary>
    /// Gets the informational version of the entry assembly.
    /// </summary>
    /// <returns>The entry assembly informational version.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no entry assembly is available or it has no informational version.
    /// </exception>
    public static string GetInformationalVersionOfEntryAssembly()
    {
        Assembly assembly = GetEntryAssembly();
        return assembly.GetInformationalVersion();
    }

    /// <summary>
    /// Gets the assembly that contains the process entry point.
    /// </summary>
    /// <returns>The entry assembly.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the current execution context has no entry assembly.
    /// </exception>
    internal static Assembly GetEntryAssembly()
    {
        return Assembly.GetEntryAssembly()
            ?? throw new InvalidOperationException(
                "The current execution context does not expose an entry assembly.");
    }

    /// <summary>
    /// Gets the full display name of the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <returns>The full assembly display name.</returns>
    public static string GetStrongName(this Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return assembly.GetName().FullName
            ?? throw new InvalidOperationException(
                $"Assembly '{assembly}' does not expose a full display name.");
    }

    /// <summary>
    /// Gets the simple name of the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <returns>The simple assembly name.</returns>
    public static string GetShortName(this Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return assembly.GetName().Name
            ?? throw new InvalidOperationException(
                $"Assembly '{assembly.FullName}' does not expose a simple name.");
    }
}
