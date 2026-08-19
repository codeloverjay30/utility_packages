using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoggerFactoryUtilityServices
{
    public interface ILoggerFactoryBaseUtilityService
    {
        ILoggerFactory LoggerFactory { get; init; }
        ILogger Logger { get; }
        bool IsLoggerCreated { get; }
    }
}
