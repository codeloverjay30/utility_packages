using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryKeyUtilityServices
{
    public class RegistryKeyManager : IRegistryKeyManager
    {
        public required string AppDisplayedName { get; init; }

        private readonly IRegistryWrapper _registry;

        public RegistryKeyManager(IRegistryWrapper? registry = null)
        {
            // 如果沒有傳入，則使用預設實作（正式環境）
            _registry = registry ?? new WindowsRegistryWrapper();
        }

        public string? GetRegistryKeyName()
        {
            string uninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            var subKeys = _registry.GetSubKeyNames(uninstallPath);

            foreach(var subKeyName in subKeys)
            {
                var displayName = _registry.GetValue($"{uninstallPath}\\{subKeyName}" , "DisplayName");
                if(displayName != null && displayName.Contains(this.AppDisplayedName , StringComparison.OrdinalIgnoreCase))
                {
                    return subKeyName;
                }
            }
            return null;
        }

        public string? GetAppSetting(string valueName)
        {
            string? keyName = this.GetRegistryKeyName();
            if(string.IsNullOrWhiteSpace(keyName))
            {
                return null;
            }
            // 透過 Wrapper 取得數值，這樣測試時就能 Mock 這個回傳值
            return _registry.GetCurrentUserValue(keyName , valueName)?.ToString();
        }
    }
}
