using NuGet.Versioning;
using System.Reflection;

namespace AssemblyUtilityServices;

/// <summary>
/// Provides matching operations for assembly informational versions and identity versions.
/// </summary>
public static class AssemblyVersionMatcher
{
    /// <summary>
    /// Determines whether an informational version is a valid NuGet-compatible version.
    /// </summary>
    /// <param name="version">The informational version to validate.</param>
    /// <returns>
    /// <see langword="true"/> when the value can be parsed as a NuGet version;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValidInformationalVersion(string? version)
    {
        return NuGetVersion.TryParse(version, out _);
    }

    /// <summary>
    /// Determines whether an informational version is a valid NuGet-compatible version.
    /// </summary>
    /// <param name="version">The informational version to validate.</param>
    /// <returns>
    /// <see langword="true"/> when the value can be parsed as a NuGet version;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [Obsolete(
        "Use IsValidInformationalVersion instead. This compatibility member will be removed in a future major version.")]
    public static bool _IsValidVersion(this string version)
    {
        return IsValidInformationalVersion(version);
    }

    /// <summary>
    /// Determines whether the informational-version major component matches the expected value.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <param name="expectedVersionLevel">The expected major version.</param>
    /// <returns>
    /// <see langword="true"/> when the informational version is valid and matches;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsMajorVersionMatched(
        this Assembly assembly,
        int expectedVersionLevel)
    {
        ValidateExpectedVersionLevel(expectedVersionLevel);

        return TryGetNuGetVersion(assembly, out NuGetVersion? version)
            && version.Major == expectedVersionLevel;
    }

    /// <summary>
    /// Determines whether the informational-version minor component matches the expected value.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <param name="expectedVersionLevel">The expected minor version.</param>
    /// <returns>
    /// <see langword="true"/> when the informational version is valid and matches;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsMinorVersionMatched(
        this Assembly assembly,
        int expectedVersionLevel)
    {
        ValidateExpectedVersionLevel(expectedVersionLevel);

        return TryGetNuGetVersion(assembly, out NuGetVersion? version)
            && version.Minor == expectedVersionLevel;
    }

    /// <summary>
    /// Determines whether the informational-version patch component matches the expected value.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <param name="expectedVersionLevel">The expected patch version.</param>
    /// <returns>
    /// <see langword="true"/> when the informational version is valid and matches;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsPatchVersionMatched(
        this Assembly assembly,
        int expectedVersionLevel)
    {
        ValidateExpectedVersionLevel(expectedVersionLevel);

        return TryGetNuGetVersion(assembly, out NuGetVersion? version)
            && version.Patch == expectedVersionLevel;
    }

    /// <summary>
    /// Determines whether the assembly identity major component matches the expected value.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <param name="expectedVersionLevel">The expected major version.</param>
    /// <returns><see langword="true"/> when the major component matches.</returns>
    public static bool IsAssemblyMajorVersionMatched(
        this Assembly assembly,
        int expectedVersionLevel)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ValidateExpectedVersionLevel(expectedVersionLevel);

        return assembly.GetAssemblyVersion().Major == expectedVersionLevel;
    }

    /// <summary>
    /// Determines whether the assembly identity minor component matches the expected value.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <param name="expectedVersionLevel">The expected minor version.</param>
    /// <returns><see langword="true"/> when the minor component matches.</returns>
    public static bool IsAssemblyMinorVersionMatched(
        this Assembly assembly,
        int expectedVersionLevel)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ValidateExpectedVersionLevel(expectedVersionLevel);

        return assembly.GetAssemblyVersion().Minor == expectedVersionLevel;
    }

    /// <summary>
    /// Determines whether the assembly identity build component matches the expected value.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <param name="expectedVersionLevel">The expected build version.</param>
    /// <returns><see langword="true"/> when the build component matches.</returns>
    public static bool IsAssemblyBuildVersionMatched(
        this Assembly assembly,
        int expectedVersionLevel)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ValidateExpectedVersionLevel(expectedVersionLevel);

        return assembly.GetAssemblyVersion().Build == expectedVersionLevel;
    }

    /// <summary>
    /// Attempts to parse the assembly informational version as a NuGet version.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <param name="version">The parsed NuGet version when successful.</param>
    /// <returns>
    /// <see langword="true"/> when the informational version exists and is valid;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private static bool TryGetNuGetVersion(
        Assembly assembly,
        out NuGetVersion? version)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        if (!assembly.TryGetInformationalVersion(out string? informationalVersion))
        {
            version = null;
            return false;
        }

        return NuGetVersion.TryParse(informationalVersion, out version);
    }

    /// <summary>
    /// Validates an expected version component.
    /// </summary>
    /// <param name="expectedVersionLevel">The expected version component.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is negative.
    /// </exception>
    private static void ValidateExpectedVersionLevel(int expectedVersionLevel)
    {
        if (expectedVersionLevel < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expectedVersionLevel),
                expectedVersionLevel,
                "Expected version level must be zero or greater.");
        }
    }
}
