using System.Reflection;
using System.Text.RegularExpressions;
using RegexUtilityServices;

namespace AssemblyUtilityServices
{
    public static class AssemblyVersionMatcher
    {
        internal static bool _IsInformationalVersionMatched(
            this Assembly assembly,
            int versionLevel,
            int expectedVersionLevel
        )
        {
            string version = AssemblyMetadataFetcher.GetInformationalVersion(assembly);
            ArgumentException.ThrowIfNullOrWhiteSpace(version, nameof(version));
            return _IsValidVersion(version) && NumberParser.CheckNthOccurrence(input:version,n:versionLevel,a:expectedVersionLevel);
        }

        /// <summary>
        /// check the version format is valid or not
        /// </summary>
        /// <param name="version"></param>
        /// <returns>
        /// + return true iff the version is valid.
        /// 
        /// + return false, otherwise.
        /// </returns>
        /// <remarks>
        /// A common valid version format:
        /// 
        /// start with `<majorVersion>.<minorVersion>.<patchVersion>`
        /// 
        /// or
        /// 
        /// start with `<majorVersion>.<minorVersion>.<patchVersion>.<littlePatchVersion>`
        /// </remarks>

        public static bool _IsValidVersion(
            this string version
        )
        {            
            bool isValidVersion = Regex.IsMatch(version, @"\d.\d.\d");
            return isValidVersion;
        }

        /// <summary>
        /// check the major version number of assembly exactly matches specific number `expectedVersionLevel`
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="expectedVersionLevel"></param>
        /// <returns>
        /// + return true iff the version is valid and the major version number of assembly exactly matches specific number `expectedVersionLevel`
        /// 
        /// + return false, otherwise.
        /// </returns>
        /// </returns>
        /// <remarks>
        /// About the definition of version and valid version format, see remark section of <seealso cref="_IsValidVersion"/> method.
        /// </remarks>
        public static bool IsMajorVersionMatched(
            this Assembly assembly,
            int expectedVersionLevel            
        )
        {
            return _IsInformationalVersionMatched(assembly, 1, expectedVersionLevel);
        }

        /// <summary>
        /// check the minor version number of assembly exactly matches specific number `expectedVersionLevel`
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="expectedVersionLevel"></param>
        /// <returns>
        /// + return true iff the version is valid and the minor version number of assembly exactly matches specific number `expectedVersionLevel`
        /// 
        /// + return false, otherwise.
        /// </returns>
        /// <remarks>
        /// About the definition of version and valid version format, see remark section of <seealso cref="_IsValidVersion"/> method.
        /// </remarks>
        public static bool IsMinorVersionMatched(
            this Assembly assembly,
            int expectedVersionLevel            
        )
        {
            return _IsInformationalVersionMatched(assembly, 2, expectedVersionLevel);
        }

        /// <summary>
        /// check the patch version number of assembly exactly matches specific number `expectedVersionLevel`
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="expectedVersionLevel"></param>
        /// <returns>
        /// + return true iff the version is valid and the minor version number of assembly exactly matches specific number `expectedVersionLevel`
        /// 
        /// + return false, otherwise.
        /// </returns>
        /// <remarks>
        /// About the definition of version and valid version format, see remark section of <seealso cref="_IsValidVersion"/> method.
        /// </remarks>
        public static bool IsPatchVersionMatched(
            this Assembly assembly,
            int expectedVersionLevel            
        )
        {
            return _IsInformationalVersionMatched(assembly, 3, expectedVersionLevel);
        }
    }
}