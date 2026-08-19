using System;
using System.Collections.Generic;
using System.Text;

namespace ILoggerBuilderFactoryServices
{
    public interface ILogConfiguration
    {
        string? LogPrefix { get; }
    }
}
