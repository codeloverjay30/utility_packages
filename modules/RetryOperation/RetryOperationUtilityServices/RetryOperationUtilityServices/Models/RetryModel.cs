using System;
using System.Collections.Generic;
using System.Text;

namespace RetryOperationUtilityServices.Models
{
    public class RetryModel
    {
        /// <summary>
        /// Maximum (continous) retry attempts
        /// </summary>
        public int MaxRetryAttempts { get; init; } = 3;

        /// <summary>
        /// Delay after the first attempt failed.
        /// </summary>
        public TimeSpan InitialDelay { get; init; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The multiplier that is relative to the delay of current attempt failed to determine the delay when the next attempt failed.
        /// </summary>
        public double BackoffMultiplier { get; init; } = 2.0;

        /// <summary>
        /// Maximum delay after the attempts failed.
        /// </summary>
        public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(30);
    }
}
