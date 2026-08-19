using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Identity;

namespace AzureUtilityServices
{
    /// <summary>
    /// wrapper class to more easily configure the options from Azure App Configuration
    /// </summary>
    public class AzureAppConfigurationUtilityService
    {
        public required AzureAppConfigurationOptions Options { get; init; }

        public DefaultAzureCredential Credential { get; init; } = new DefaultAzureCredential();
        public required Uri Uri { get; init; }
        public Action<AzureAppConfigurationKeyVaultOptions>? KeyVaultSetup { get; init; } = null;
        public Action<AzureAppConfigurationRefreshOptions>? RefreshSetup { get; init; } = null;

        public IEnumerable<string>? labels { get; init; } = null;

        public void Initialize()
        {
            this.Options.Connect(Uri, Credential)
                .ConfigurePullRefreshHook(RefreshSetup)
                .ConfigureKeyVaultHook(KeyVaultSetup)
                .SkipAppConfigurationWithLabels(labels);
        }
    }
}