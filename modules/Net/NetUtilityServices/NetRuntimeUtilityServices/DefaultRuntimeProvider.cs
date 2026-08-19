using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NetRuntimeUtilityServices
{
    public class DefaultRuntimeProvider : IRuntimeEnvironmentProvider
    {
        public Version GetVersion() => Environment.Version;
        public bool IsOSPlatform(string platform) =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Create(platform.ToUpper()));
        public string GetOSDescription() => RuntimeInformation.OSDescription;
    }
}
