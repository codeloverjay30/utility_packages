using ILoggerBuilderFactoryServices;
using LoggerFactoryUtilityServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostSharpAttributesUtilityService
{
    public static class LoggerBridge
        <TConfig>
        where TConfig : class, ILogConfiguration, new()
    {
        private static LoggerFactoryBaseUtilityService? _loggerFactoryService;
        public static string? CategoryName { get; private set; }
        public static IOptions<TConfig>? Options { get; private set; } 
        public static void Initialize(LoggerFactoryBaseUtilityService loggerFactoryService)
        {
            _loggerFactoryService = loggerFactoryService;
        }

        public static void SetOptions(IOptions<TConfig> options)
        {
            Options = options;
        }

        public static void SetCategoryName(string categoryName)
        {
            CategoryName = categoryName;
        }

        public static ILogger GetLogger(
            Type type,
            bool useNullLogger = true
        )
        {
            // check it has been instantiated
            if(_loggerFactoryService == null)
            {
                if(useNullLogger)
                {
                    return Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
                }

                var loggerFactory = CreateDefaultILoggerFactory();
                _loggerFactoryService = new LoggerFactoryBaseUtilityService { LoggerFactory = loggerFactory };
                return _loggerFactoryService.Logger;
            }

            if(!_loggerFactoryService.IsLoggerCreated)
            {
                var loggerFactory = CreateDefaultILoggerFactory();
                _loggerFactoryService = new LoggerFactoryBaseUtilityService { LoggerFactory = loggerFactory };
                // Will instantiate a create a ILogger instance and thus _loggerFactoryService.IsLoggerCreated will be set to true.
                return _loggerFactoryService.Logger;
            }

            // Will use the cached ILogger instance.
            return _loggerFactoryService.Logger;
        }

        private static ILoggerFactory CreateDefaultILoggerFactory()
        {
            ArgumentNullException.ThrowIfNull(Options,nameof(Options));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(CategoryName,nameof(CategoryName));

            MyUniversalLoggerProvider<TConfig> provider = new MyUniversalLoggerProvider<TConfig>(Options);

            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(provider);
            });

            return factory;
        }
    }
}
