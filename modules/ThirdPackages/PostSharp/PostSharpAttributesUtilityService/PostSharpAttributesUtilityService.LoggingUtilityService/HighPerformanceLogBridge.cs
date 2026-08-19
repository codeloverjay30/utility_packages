using Microsoft.Extensions.Logging;

namespace PostSharpAttributesUtilityService.LoggingUtilityService
{
    public static partial class HighPerfLogBridge
    {
        // 核心：定義一個結構化的通用範本
        // [{ns}] [{className}] {methodName} {eventStatus} | Args: {detail}
        [LoggerMessage(
            EventId = 1001 ,
            Level = LogLevel.Information ,
            Message = "[{Namespace}] [{ClassName}] {MethodName} {EventStatus} | Args: {Detail}")]
        public static partial void LogBoundary(
            ILogger logger ,
            string @namespace ,
            string className ,
            string methodName ,
            string eventStatus ,
            string detail);
    }
}
