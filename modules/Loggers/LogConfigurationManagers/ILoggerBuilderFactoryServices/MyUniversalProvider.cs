using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ILoggerBuilderFactoryServices
{
    public class MyUniversalLoggerProvider<TConfig> : ILoggerProvider
        where TConfig : class, ILogConfiguration, new()
    {
        private readonly TConfig _options;

        public MyUniversalLoggerProvider(IOptions<TConfig> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// 根據分類名稱產生 Logger
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new MyUniversalLogger<TConfig>(categoryName , _options);
        }

        /// <summary>
        /// 清理資源
        /// </summary>
        public void Dispose()
        {

        }
    }
}
