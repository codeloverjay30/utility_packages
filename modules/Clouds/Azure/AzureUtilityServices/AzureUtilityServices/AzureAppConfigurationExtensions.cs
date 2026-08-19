using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Identity;

namespace AzureUtilityServices
{
    public static class AzureAppConfigurationExtensions
    {
        /// <summary>
        /// utility extension method to configure pull refresh hook events on `Azure App Configuration`.
        /// </summary>
        /// <param name="options">options of Azure App Configuration</param>
        /// <param name="refreshSetup">actions of pull refresh hook</param>
        /// <returns>updated of <paramref name="options"/>, only used for chaining</returns>
        /// <remarks>
        /// + If the <paramref name="refreshSetup"/> is not specified, it will use default value to null
        /// 
        /// + If the <paramref name="refreshSetup"/> is specified to null (or not specified), no pull refresh hook will be configured.
        /// </remarks>
        public static AzureAppConfigurationOptions ConfigurePullRefreshHook(
            this AzureAppConfigurationOptions options,
            Action<AzureAppConfigurationRefreshOptions>? refreshSetup = null
        )
        {
            if (refreshSetup != null)
            {
                options.ConfigureRefresh(refresh=>refreshSetup(refresh));
            }
            return options;
        }

        /// <summary>
        /// utility extension method to configure key vault hook events on `Azure App Configuration`.
        /// </summary>
        /// <param name="options">options of Azure App Configuration</param>
        /// <param name="keyVaultSetup">actions of key vault hook</param>
        /// <returns>updated of <paramref name="options"/>, only used for chaining</returns>
        /// <remarks>
        /// + If the <paramref name="keyVaultSetup"/> is not specified, it will use default value to null
        /// 
        /// + If the <paramref name="keyVaultSetup"/> is specified to null (or not specified), no key vault hook will be configured.
        /// </remarks>
        public static AzureAppConfigurationOptions ConfigureKeyVaultHook(
            this AzureAppConfigurationOptions options,
            Action<AzureAppConfigurationKeyVaultOptions>? keyVaultSetup = null
        )
        {
            if (keyVaultSetup != null)
            {
                options.ConfigureKeyVault(keyVaultOptions=>keyVaultSetup(keyVaultOptions));
            }

            return options;
        }


        /// <summary>
        /// Filter out labels on `Azure App Configuration` to access.
        /// </summary>
        /// <param name="options">options of Azure App Configuration</param>
        /// <param name="labels">labels that will be filtered</param>
        /// <returns>updated of <paramref name="options"/>, only used for chaining</returns>
        /// <remarks>
        /// + If the <paramref name="labels"/> is not specified, it will use default value to null
        /// 
        /// + If the <paramref name="labels"/> is specified to null (or not specified), all labels will be fetched.
        /// </remarks>
        public static AzureAppConfigurationOptions SkipAppConfigurationWithLabels(
            this AzureAppConfigurationOptions options,
            IEnumerable<string>? labels = null
        )
        {
            // try to filter out app configuration from all available labels.
            options.Select(KeyFilter.Any, LabelFilter.Null);
            if (labels != null)
            {
                foreach (var label in labels.Where(l => !string.IsNullOrEmpty(l)))
                {
                    options.Select(KeyFilter.Any, label);
                }
            }

            return options;
        }
    }
}