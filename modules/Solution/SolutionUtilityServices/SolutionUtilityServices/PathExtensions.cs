using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;

namespace SolutionUtilityServices
{
    public static class PathExtensions
    {
        private static readonly IFileSystem _defaultFileSystem = new FileSystem();
        public static string GetFileExtension(
            this string path,
            IFileSystem fileSystem = null
        )
        {
            var activeFileSystem = fileSystem ?? _defaultFileSystem;
            return activeFileSystem.Path.GetExtension(path);
        }
        public static bool IsOneOf(
            this string fileExt,
            HashSet<string> exts
        )
        {
            return exts.Contains(fileExt);
        }
    }
}
