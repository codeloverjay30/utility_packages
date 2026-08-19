using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryKeyUtilityServices
{
    public class WindowsRegistryWrapper : IRegistryWrapper
    {
        public IEnumerable<string> GetSubKeyNames(string path)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            using var key = Registry.LocalMachine.OpenSubKey(path);
            return key?.GetSubKeyNames() ?? Enumerable.Empty<string>();
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public string? GetValue(
            string subKeyPath ,
            string valueName
        )
        {
#pragma warning disable CA1416 // Validate platform compatibility
            using var key = Registry.LocalMachine.OpenSubKey(subKeyPath);
            return key?.GetValue(valueName)?.ToString();
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public object? GetCurrentUserValue(
            string keyName ,
            string valueName
        )
        {
#pragma warning disable CA1416
            using var key = Registry.CurrentUser.OpenSubKey(keyName);
            return key?.GetValue(valueName);
#pragma warning restore CA1416
        }
    }
}
