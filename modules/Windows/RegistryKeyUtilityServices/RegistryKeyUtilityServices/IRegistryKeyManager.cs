using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryKeyUtilityServices
{
    public interface IRegistryKeyManager
    {
        string AppDisplayedName { get; init; }
        string? GetRegistryKeyName();
        string? GetAppSetting(string valueName);
    }
}
