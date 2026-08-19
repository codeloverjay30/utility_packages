
using Microsoft.Extensions.Logging;

namespace LoggerFactoryUtilityServices
{
    public class LoggerFactoryBaseUtilityService(
        ILoggerFactory loggerFactory ,
        string unknownAppName = "UnknownApp"
    ) : ILoggerFactoryBaseUtilityService
    {
        /// <summary>
        /// Use primary constructor to DI inject `ILoggerFactory`,
        /// avoiding the order of initialization using `init`.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; init; } = loggerFactory;
        private readonly Lazy<ILogger> _logger = new(() =>
        loggerFactory.CreateLogger(unknownAppName));

        public virtual ILogger Logger => _logger.Value;

        /// <summary>
        /// Check Logger is initialize
        /// </summary>
        public bool IsLoggerCreated => _logger.IsValueCreated;
    }
}
