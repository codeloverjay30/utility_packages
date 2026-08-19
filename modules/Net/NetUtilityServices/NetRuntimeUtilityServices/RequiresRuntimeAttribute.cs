using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace NetRuntimeUtilityServices
{
    public class RequiresRuntimeAttribute: ValidationAttribute
    {
        private readonly string[] _supportedPlatforms;
        private readonly Version _minimumVersion;

        public RequiresRuntimeAttribute(
            int major ,
            int minor,
            params string[] platforms
        )
        {
            _supportedPlatforms = platforms;
            _minimumVersion = new Version(major , minor);
        }

        protected override ValidationResult? IsValid(
            object? value ,
            ValidationContext validationContext
        )
        {
            // 從 Context 取得 Service，如果沒有則使用預設實作 (這是關鍵的解耦點)
            var provider = validationContext.GetService(typeof(IRuntimeEnvironmentProvider)) as IRuntimeEnvironmentProvider
                           ?? new DefaultRuntimeProvider();

            // 1. 使用 Provider 檢查作業系統
            bool isSupportedOS = false;
            foreach(var platform in _supportedPlatforms)
            {
                if(provider.IsOSPlatform(platform))
                {
                    isSupportedOS = true;
                    break;
                }
            }

            if(!isSupportedOS)
            {
                return new ValidationResult($"Current OS ({provider.GetOSDescription()}) is NOT supported on {string.Join(",",_supportedPlatforms)} for this API.");
            }

            // 2. 使用 Provider 檢查版本
            if(provider.GetVersion() < _minimumVersion)
            {
                return new ValidationResult($"current .NET runtime({provider.GetVersion()}) is less than required version {_minimumVersion} for this API.");
            }

            return ValidationResult.Success;
        }
    }
}
