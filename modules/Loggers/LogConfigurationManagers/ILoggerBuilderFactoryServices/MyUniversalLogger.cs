using ConsoleUtilityServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ILoggerBuilderFactoryServices
{
    public class MyUniversalLogger<TConfig> : ILogger
        where TConfig : class, ILogConfiguration, new()
    { 
        private readonly string _categoryName;
        private readonly TConfig _options;

        public MyUniversalLogger(string categoryName , TConfig options)
        {
            _categoryName = categoryName;
            _options = options;
        }

        public void Log<TState>(LogLevel logLevel , EventId eventId , TState state , Exception? exception , Func<TState , Exception? , string> formatter)
        {
            var message = formatter(state , exception);
            var prefix = _options.LogPrefix;
            var output = string.IsNullOrEmpty(prefix) ? message : $"{prefix}: {message}";

            var logMessage1 = $"[{_categoryName}] [{logLevel}] {output}";

            IConsoleService consoleService = new ConsoleService();
            bool canUseConsole = false;
            try
            {
                canUseConsole = consoleService.CanUseConsole();
            } catch(Exception)
            {
                canUseConsole = false;
            }

            // 透過 Singleton 管理器寫入檔案
            if(_options is IFileLogConfiguration fileConfig && !string.IsNullOrEmpty(fileConfig.LogFilePath))
            {
                // 第一次使用時初始化路徑
                FileLogManager.Instance.Initialize(fileConfig.LogFilePath);
                FileLogManager.Instance.Enqueue(logMessage1);
            }
            else if(_options is IExtendedLogConfiguration extended && extended.EnableDebug)
            {
                // 如果可以
                System.Diagnostics.Debug.WriteLine(logMessage1);
            }
            else if(canUseConsole)
            {
                // 如果只是普通的 ILogConfiguration，就試圖走預設路線
                // 若在執行環境中，可以將訊息導向至StdOut(stands for Standard Output Stream)，則將訊息導向至StdOut。
                Console.WriteLine(logMessage1);
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }
}
