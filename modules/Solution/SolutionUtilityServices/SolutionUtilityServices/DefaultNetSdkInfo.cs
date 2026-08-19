using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Locator;

namespace SolutionUtilityServices
{
    /// <summary>
    /// NET SDK info helper
    /// </summary>
    public class DefaultNetSdkInfo : INetSdkInfo
    {
        public string DefaultLatestVersion => "8.0";

        public string GetInstalledLatestVersion()
        {
            try
            {
                var instances = MSBuildLocator.QueryVisualStudioInstances();
                var latestInstance = instances
                    .Where(inst => inst.DiscoveryType == DiscoveryType.DotNetSdk)
                    .OrderByDescending(inst => inst.Version)
                    .FirstOrDefault();

                if (latestInstance != null)
                {
                    return $"{latestInstance.Version.Major}.{latestInstance.Version.Minor}";
                }
            }
            catch
            {
                
            }

            return DefaultLatestVersion;
        }
    }
}
