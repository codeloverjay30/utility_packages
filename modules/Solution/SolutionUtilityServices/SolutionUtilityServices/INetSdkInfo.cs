using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionUtilityServices
{
    public interface INetSdkInfo
    {
        string DefaultLatestVersion { get; }
        string GetInstalledLatestVersion();
    }
}
