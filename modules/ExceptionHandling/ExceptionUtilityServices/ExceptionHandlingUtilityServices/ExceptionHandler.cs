using LoggerFactoryUtilityServices;
using Microsoft.Extensions.Logging;

namespace ExceptionHandlingUtilityServices
{
    public abstract class ExceptionHandler(
        ILoggerFactoryBaseUtilityService loggerFactoryService ,
        bool toLogWhenSuccess = true
    )
    {

        private ILoggerFactory? _loggerFactory;
        protected ILoggerFactory LoggerFactory => _loggerFactory ?? loggerFactoryService.LoggerFactory;

        protected ILogger Logger => loggerFactoryService.Logger;

        public bool ToLogWhenSuccess { get; init; } = toLogWhenSuccess;
        public T SafeExecute<T>(Func<T> func , string operationName , T defaultValue = default)
        {
            // 建立一個 Log 範圍，這會讓該區塊內所有的 Log 自動帶上 OperationName
            using(Logger.BeginScope(new Dictionary<string , object> { [ "Operation" ] = operationName }))
            {
                try
                {
                    var ret_val = func();
                    if(ToLogWhenSuccess)
                    {
                        Logger.LogInformation("執行成功");
                    }
                    return ret_val;
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex , "執行失敗");
                    return defaultValue;
                }
            }
        }
    } 
}
