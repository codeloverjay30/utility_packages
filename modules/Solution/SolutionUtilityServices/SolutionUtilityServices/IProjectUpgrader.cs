using CommonModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionUtilityServices
{
    /// <summary>
    /// interface of project upgrader
    /// </summary>
    [Obsolete()]
    public interface IProjectUpgrader
    {
        StatusJsonModels FullUpgrade();
        StatusJsonModel UpgradeNuGetPackages();

        StatusJsonModel UpgradeFrameworkInFile(
            string filePath ,
            string targetFramework
        );

        string GetLatestSdkVersion();
    }
}
