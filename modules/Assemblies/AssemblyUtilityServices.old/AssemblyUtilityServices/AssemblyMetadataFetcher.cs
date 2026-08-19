using System.Reflection;

namespace AssemblyUtilityServices
{
    public static class AssemblyMetadataFetcher
    {
        /// <summary>
        /// get assembly version
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns>
        /// + returns assembly information version (like `1.0.0-preview-1.0.0` in `<InformationalVersion>` or `<Version>` tag in .csproj file) if one of them exists.
        /// 
        /// + returns the assembly version (like `[1.0.0.0]`), otherwise.
        /// </returns>
        public static string GetInformationalVersion(
            this Assembly assembly
        )
        {
            var versionAttribute = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion 
                      ?? assembly?.GetName().Version?.ToString();
            
            ArgumentException.ThrowIfNullOrWhiteSpace(versionAttribute, nameof(versionAttribute));
            return versionAttribute;
        }

        /// <summary>
        /// get the version of assembly who executes the entry point. See <seealso cref="GetInformationalVersion"/> for more detais.
        /// </summary>
        /// <returns>See <seealso cref="GetInformationalVersion"/> for more detais.</returns>

        public static string GetInformationalVersionOfEntryAssembly()
        {
            var assembly = GetEntryAssembly();
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));
            return GetInformationalVersion(assembly);
        }

        /// <summary>
        /// get the assembly who executes the entry point.
        /// </summary>
        /// <returns></returns>
        internal static Assembly GetEntryAssembly()
        {
            var assembly = Assembly.GetEntryAssembly();
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));
            return assembly;
        }

        /// <summary>
        /// get the strong name of a specific assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string? GetStrongName(
            this Assembly assembly
        )
        {
            return assembly?.GetName().FullName;
        }

        /// <summary>
        /// get the short name of a specific assembly
        /// </summary>
        public static string? GetShortName(
            this Assembly assembly
        )
        {
            return assembly?.GetName().Name;
        }
    }
}
