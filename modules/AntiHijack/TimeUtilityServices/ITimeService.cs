using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeUtilityServices;

/// <summary>
/// Defines high-precision and testable time tracking abstractions.
/// </summary>
public interface ITimeService
{
    /// <summary>
    /// Retrieves the current elapsed milliseconds since an arbitrary point in time, safe for benchmarking and performance tracking.
    /// </summary>
    /// <returns>The timestamp value in milliseconds.</returns>
    long GetCurrentStopWatch();

    /// <summary>
    /// Retrieves the current system time in coordinated universal time (UTC) via the underlying provider.
    /// </summary>
    /// <returns>A DateTimeOffset representing the current UTC time.</returns>
    DateTimeOffset GetUtcNow();
}
    
    


