using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeUtilityServices;

/// <summary>
/// Implements time tracking services integrated with the .NET modern TimeProvider architecture.
/// </summary>
public class TimeOnAzureService : ITimeService
{
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeOnAzureService"/> class.
    /// </summary>
    /// <param name="timeProvider">The internal system or mocked time provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when timeProvider is null.</exception>
    public TimeOnAzureService(
        TimeProvider timeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(timeProvider, nameof(timeProvider));
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Retrieves high-precision elapsed milliseconds derived from the standard TimeProvider timestamp mechanism.
    /// </summary>
    /// <returns>The accurate timestamp value in milliseconds.</returns>
    public long GetCurrentStopWatch()
    {
        long timestamp = _timeProvider.GetTimestamp();
        double elapsedSeconds = _timeProvider.GetElapsedTime(timestamp).TotalMilliseconds;
        return (long)elapsedSeconds;
    }

    /// <summary>
    /// Retrieves the current exact UTC date and time.
    /// </summary>
    /// <returns>The current UTC timestamp.</returns>
    public DateTimeOffset GetUtcNow()
    {
        return _timeProvider.GetUtcNow();
    }
}
    
    
    
