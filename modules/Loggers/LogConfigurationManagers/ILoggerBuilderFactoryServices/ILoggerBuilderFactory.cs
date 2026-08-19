using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ILoggerBuilderFactoryServices
{
    public class ILoggerBuilderFactory<TConfig>
        where TConfig : class, ILogConfiguration, new()
    {

        public required ILoggingBuilder Builder { get; init; }
        public void AddConfiguration<T>(
            Action<TConfig>? configure
        )
        {
            ArgumentNullException.ThrowIfNull(Builder , nameof(Builder));
            ArgumentNullException.ThrowIfNull(configure , nameof(configure));

            Builder.Services.Configure(configure);
            Builder.Services.AddSingleton<ILoggerProvider , MyUniversalLoggerProvider<TConfig>>();
        }
        public ILoggingBuilder Create()
        {
            return Builder;
        }
    }
}
