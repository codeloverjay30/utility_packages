using Microsoft.Win32;

namespace RegistryUtilityServices;

public class RegistryService : IRegistryService
{
    /// <inheritdoc/>
    public string? GetValue(string keyPath, string valueName)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath, writable: false);
            return key?.GetValue(valueName)?.ToString();
        }
        catch (System.Security.SecurityException)
        {
            return null; // Handle according to your security policy
        }
    }
}