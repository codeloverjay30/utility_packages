using System.IO.Abstractions;
using System.Reflection;

namespace AssemblyUtilityServices;

/// <summary>
/// Discovers and loads assemblies from a configured directory.
/// </summary>
public sealed class AssembliesUtilityService : IAssembliesUtilityService
{
    private readonly string _solutionPath;
    private readonly string _globFilter;
    private readonly IFileSystem _fileSystem;
    private readonly IAssemblyLoader _assemblyLoader;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssembliesUtilityService"/> class
    /// using the real file system and the default path-based assembly loader.
    /// </summary>
    /// <param name="solutionPath">The directory to search.</param>
    /// <param name="globFilter">The file search pattern, such as <c>*.dll</c>.</param>
    public AssembliesUtilityService(
        string solutionPath,
        string globFilter)
        : this(
            solutionPath,
            globFilter,
            new FileSystem(),
            assemblyLoader: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssembliesUtilityService"/> class
    /// with explicit dependencies for deterministic testing.
    /// </summary>
    /// <param name="solutionPath">The directory to search.</param>
    /// <param name="globFilter">The file search pattern, such as <c>*.dll</c>.</param>
    /// <param name="fileSystem">The file-system abstraction.</param>
    /// <param name="assemblyLoader">
    /// The assembly loader. When <see langword="null"/>, an <see cref="AssemblyLoader"/>
    /// backed by <paramref name="fileSystem"/> is used.
    /// </param>
    public AssembliesUtilityService(
        string solutionPath,
        string globFilter,
        IFileSystem fileSystem,
        IAssemblyLoader? assemblyLoader = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(solutionPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(globFilter);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _fileSystem = fileSystem;
        _solutionPath = _fileSystem.Path.GetFullPath(solutionPath);
        _globFilter = globFilter;
        _assemblyLoader = assemblyLoader ?? new AssemblyLoader(fileSystem);
    }

    /// <summary>
    /// Lists assembly file paths that match the configured glob filter.
    /// </summary>
    /// <returns>The matching assembly paths in deterministic order.</returns>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when the configured directory does not exist.
    /// </exception>
    public IEnumerable<string> ListAllAssemblies()
    {
        if (!_fileSystem.Directory.Exists(_solutionPath))
        {
            throw new DirectoryNotFoundException(
                $"Assembly search directory '{_solutionPath}' does not exist.");
        }

        return _fileSystem.Directory
            .GetFiles(_solutionPath, _globFilter)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Loads assemblies from the specified file paths.
    /// </summary>
    /// <param name="dllFiles">The assembly file paths to load.</param>
    /// <returns>The loaded assemblies in the same order as the supplied paths.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="dllFiles"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when an item in <paramref name="dllFiles"/> is null, empty, or whitespace.
    /// </exception>
    public List<Assembly> LoadAllAssemblies(IEnumerable<string> dllFiles)
    {
        ArgumentNullException.ThrowIfNull(dllFiles);

        var assemblies = new List<Assembly>();

        foreach (string dllFile in dllFiles)
        {
            if (string.IsNullOrWhiteSpace(dllFile))
            {
                throw new ArgumentException(
                    "Assembly file paths cannot contain null, empty, or whitespace values.",
                    nameof(dllFiles));
            }

            assemblies.Add(_assemblyLoader.LoadFromPath(dllFile));
        }

        return assemblies;
    }
}
