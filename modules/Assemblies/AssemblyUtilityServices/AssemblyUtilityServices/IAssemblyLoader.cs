using System.Reflection;

namespace AssemblyUtilityServices;

/// <summary>
/// Defines path-based assembly loading.
/// </summary>
public interface IAssemblyLoader
{
    /// <summary>
    /// Loads an assembly from the specified file path.
    /// </summary>
    /// <param name="assemblyPath">The assembly file path.</param>
    /// <returns>The loaded assembly.</returns>
    Assembly LoadFromPath(string assemblyPath);
}
