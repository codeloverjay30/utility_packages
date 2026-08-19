using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeUtilityService.Utilities
{
    public class TimeOnAzureService : ITimeService
    {
        private const int _MillSeconds = 1000;

        public long GetCurrentStopWatch()
        {
            return Stopwatch.GetTimestamp() / Stopwatch.Frequency * _MillSeconds;
        }
    }
}
