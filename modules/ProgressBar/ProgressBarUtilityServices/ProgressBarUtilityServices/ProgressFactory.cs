using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressBarUtilityServices
{
    public class ProgressFactory(ILogger<ProgressFactory> logger) : IProgressFactory
    {
        public ITaskProgressTracker CreateTracker(string description)
        {
            // 這裡可以根據環境判斷要回傳 Spectre 還是 Logger
            // 如果是 CI 環境或沒有互動式終端
            if(Console.IsOutputRedirected)
            {
                return new LoggerTracker(description , logger);
            }

            return new SpectreTracker(description);
        }
    }
}
