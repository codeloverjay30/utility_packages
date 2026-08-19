using System.Reflection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Enrichers.Demystifier;
using LogNameUtilityFactories;
using Microsoft.Extensions.Logging;
using static AssemblyUtilityServices.AssemblyMetadataFetcher;
using Serilog.Debugging;
using System.Diagnostics;
using FileNameUtilityFactories;

namespace SerilogHelperServices
{
    public static class SerilogExtensions
    {
        public static void SetSerilogConfig(
            this Serilog.LoggerConfiguration loggerConfig,
            HostBuilderContext context
        )
        {
            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            loggerConfig
                .ReadFrom.Configuration(context.Configuration) // 從配置系統讀取 "Serilog" 區段
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("AppVersion", assembly?.GetInformationalVersion() ?? "Unknown Version")
                ;
        }

        public static void SetSerilogConfig(
            this Serilog.LoggerConfiguration loggerConfig
        )
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            loggerConfig
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Environment",assembly?.GetShortName() ?? "Unknown Environment")
                .Enrich.WithProperty("AppVersion", assembly?.GetInformationalVersion() ?? "Unknown Version")
                ;
        }

        public static void EnableDemystifyStackTrace(
            this Serilog.LoggerConfiguration loggerConfig
        )
        {
            loggerConfig.Enrich.When(
                logEvent => logEvent.Level >= LogEventLevel.Error,
                configuration => configuration.WithDemystifiedStackTraces()
            );
        }

        public static void SetLogFileName(
            this Serilog.LoggerConfiguration loggerConfig,
            string baseDirectory,
            string? logFileName = null
        )
        {
            string logFullPath = string.Empty;
            ArgumentException.ThrowIfNullOrEmpty(baseDirectory);

            if (string.IsNullOrEmpty(logFileName))
            {
                var projectNameFactory = new ProjectNameFactory();
                var logNameFactory = new LogNameFactory(projectNameFactory);
                var logFullPathFactory = new LogFullPathFactory(baseDirectory,logNameFactory);
                logFullPath = logFullPathFactory.Create();
            }
            else
            {
                logFullPath = Path.Combine(baseDirectory, logFileName);
            }

            loggerConfig.WriteTo.File(
                path: logFullPath,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                shared: true,
                rollingInterval: RollingInterval.Infinite,
                fileSizeLimitBytes: 10*1024*1024,
                flushToDiskInterval: TimeSpan.FromSeconds(2),
                rollOnFileSizeLimit: true
            );
        }
    }
}