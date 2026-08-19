using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressBarUtilityServices
{
    public class LoggerTracker(string taskName , ILogger logger) : ITaskProgressTracker
    {
        private int _lastReportedPercent = -1;

        public void Update(double percentage , string? message = null)
        {
            int percent = (int)(percentage * 100);
            if(percent % 10 == 0 && percent != _lastReportedPercent)
            {
                // 使用結構化日誌注入變數
                logger.LogInformation("[{TaskName}] Progress: {Percent}% - {Message}" ,
                    taskName , percent , message ?? "Processing");
                _lastReportedPercent = percent;
            }
        }

        public void Complete(string? message = null) =>
            logger.LogInformation("[{TaskName}] Completed. {Message}" , taskName , message ?? "");

        public void Dispose() { }
    }
}
