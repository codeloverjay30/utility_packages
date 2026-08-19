using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionUtilityServices
{
    /// <summary>
    /// Utility service that determines which entries should be excluded.
    /// </summary>
    public class ExcludedEntriesUtilityService : IExcludedEntriesUtilityService
    {
        private static readonly HashSet<string> _excludedPath = new HashSet<string>
        {
            "\\bin\\" ,
            "\\obj\\" ,
            "\\.vs\\" ,
            "\\.vshistory\\",
            "\\.git\\",
        };
        private static readonly HashSet<string> _excludedFolderName = new HashSet<string>
        {
            "bin" ,
            "obj" ,
            ".vs" ,
            ".vshistory",
            ".git",
        };

        public bool IsExcludedPath(string path) => path.IsOneOf(_excludedPath);
        public bool IsExcludedFolderName(string folderName) => folderName.IsOneOf(_excludedFolderName);
    }
}
