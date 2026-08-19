using System.Reflection;

namespace AssemblyUtilityServices;

/// <summary>
/// Defines operations for discovering and loading assemblies.
/// </summary>
public interface IAssembliesUtilityService
{
    /// <summary>
    /// Lists assembly file paths that match the configured glob filter.
/// </summary>
    /// <returns>The matching assembly file paths in deterministic order.</returns>
    IEnumerable<string> ListAllAssemblies();

    /// <summary>
    /// Loads assemblies from the specified file paths.
    /// </summary>
    /// <param name="dllFiles">The assembly file paths to load.</param>
    /// <returns>The loaded assemblies.</returns>
    List<Assembly> LoadAllAssemblies(IEnumerable<string> dllFiles);
}
