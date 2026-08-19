using System.IO.Abstractions;
using System.Reflection;

namespace AssemblyUtilityServices;

/// <summary>
/// Loads assemblies from file-system paths.
/// </summary>
public sealed class AssemblyLoader : IAssemblyLoader
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyLoader"/> class
    /// using the real file system.
/// </summary>
    public AssemblyLoader()
        : this(new FileSystem())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyLoader"/> class.
/// </summary>
    /// <param name="fileSystem">The file-system abstraction.</param>
    public AssemblyLoader(IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Loads an assembly from the specified file path.
    /// </summary>
    /// <param name="assemblyPath">The assembly file path.</param>
    /// <returns>The loaded assembly.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="assemblyPath"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the specified assembly file does not exist.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the file exists but cannot be loaded as a managed assembly.
    /// </exception>
    public Assembly LoadFromPath(string assemblyPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);

        string fullPath = _fileSystem.Path.GetFullPath(assemblyPath);

        if (!_fileSystem.File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"Assembly file '{fullPath}' does not exist.",
                fullPath);
        }

        try
        {
            return Assembly.LoadFrom(fullPath);
        }
        catch (BadImageFormatException exception)
        {
            throw new InvalidOperationException(
                $"File '{fullPath}' is not a valid managed assembly.",
                exception);
        }
        catch (FileLoadException exception)
        {
            throw new InvalidOperationException(
                $"Assembly '{fullPath}' could not be loaded.",
                exception);
        }
    }
}
