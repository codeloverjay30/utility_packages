using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using static SerilogHelperServices.SerilogExtensions;

namespace LogConfigurationUtilityService
{
    public static class LogConfigurationManager
    {
        public static void SetupLogConfiguration(
            this HostBuilderContext context,
            string baseDirectory,
            bool enableDemystifyStackTrace = true,
            Action<Serilog.LoggerConfiguration, IConfiguration>? extraLogging = null
        )
        {
            var loggerConfig = new Serilog.LoggerConfiguration();
            if(context!=null)
            {
                loggerConfig.SetSerilogConfig(context);
            }
            else
            {
                loggerConfig.SetSerilogConfig();
            }

            if (enableDemystifyStackTrace)
            {
                loggerConfig.EnableDemystifyStackTrace();
            }

            loggerConfig.SetLogFileName(baseDirectory,null);
            extraLogging?.Invoke(loggerConfig, context.Configuration);
        }
    }
}