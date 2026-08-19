using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionUtilityServices
{
    public interface IExcludedEntriesUtilityService
    {
        public bool IsExcludedPath(string path);
        public bool IsExcludedFolderName(string folderName); 
    }
}
