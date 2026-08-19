using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NetRuntimeUtilityServices
{
    public interface IRuntimeEnvironmentProvider
    {
        Version GetVersion();
        bool IsOSPlatform(string platform);
        string GetOSDescription();
    }
}
