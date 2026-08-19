using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryKeyUtilityServices
{
    public interface IRegistryWrapper
    {
        IEnumerable<string> GetSubKeyNames(string path);
        string? GetValue(string subKeyPath , string valueName);
        object? GetCurrentUserValue(
             string keyName ,
             string valueName
         );
    }
}
