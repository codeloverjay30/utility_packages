using System;

namespace ExceptionFactories
{
    /// <summary>
    /// a factory class of exception message
    /// 
    /// a convenience class that can easily to generate exception message.
    /// </summary>
    public class ExceptionFactory
    {
        public string DetailedMessage { get; set; } 
        public ExceptionFactory(Exception ex) 
        {
            string exceptionMessage = ex?.Message ?? string.Empty;
            string exceptionStackTrace = ex?.StackTrace ?? string.Empty;

            Exception innerException = ex?.InnerException;
            string innerExceptionMessage = innerException?.Message ?? string.Empty;
            string innerExceptionStackTrace = innerException?.StackTrace ?? string.Empty;
            string innerExceptionTitle = "Inner Exception";
            string innerExceptionStringWithTitle = innerException != null ? ($"{innerExceptionTitle} Message:\n{innerExceptionMessage}\n{innerExceptionTitle} Stack Trace:\n{innerExceptionStackTrace}\n") : string.Empty;

            this.DetailedMessage = $"Exception Message:\n{exceptionMessage}\nStack Trace:\n{exceptionStackTrace}\n{innerExceptionStringWithTitle}";
        }

        /// <summary>
        /// a factory method
        /// </summary>
        /// <returns>the detailed message</returns>
        public string Create()
        {
            return this.DetailedMessage;
        }
    }
}
