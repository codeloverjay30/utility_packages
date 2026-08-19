using CommonModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionUtilityServices
{
    public interface ICSharpProjectUpgrader
    {
        Task<StatusJsonModels> FullUpgradeAsync();
        Task<StatusJsonModel> UpgradeNuGetPackagesAsync(string filePath);
        StatusJsonModel UpgradeFrameworkInFile(
            string filePath ,
            string targetFramework
        );

        string GetLatestSdkVersion();
    }
}
