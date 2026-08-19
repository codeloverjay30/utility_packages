using System;
using System.IO.Abstractions;

namespace EnvironmentUtilityServices;

/// <summary>
/// Implements defensive OS utility services backed by an abstract file system to eliminate unhandled IO failures.
/// </summary>
public class OsUtilityService : IOsUtilityService
{
    private readonly IFileSystem _fileSystem;
    private readonly IEnvironmentService _environmentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OsUtilityService"/> class with mandatory, non-null dependencies.
    /// </summary>
    /// <param name="fileSystem">The abstract file system instance.</param>
    /// <param name="environmentService">The environment detection service.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the dependencies are null.</exception>
    public OsUtilityService(
        IFileSystem fileSystem,
        IEnvironmentService environmentService
    )
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(environmentService);

        _fileSystem = fileSystem;
        _environmentService = environmentService;
    }

    /// <inheritdoc />
    public StringComparison GetComparison()
    {
        return _environmentService.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when the provided path is null, empty, or consists only of white-space characters.</exception>
    public string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        // Defensive Action: Utilize the abstracted IFileSystem to extract path formatting, avoiding direct hardcoded IO operations.
        return _fileSystem.Path.GetFullPath(path).Trim();
    }
}