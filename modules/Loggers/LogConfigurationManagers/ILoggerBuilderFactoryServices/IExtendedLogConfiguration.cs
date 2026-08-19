using System;
using System.Collections.Generic;
using System.Text;

namespace ILoggerBuilderFactoryServices
{
    public interface IExtendedLogConfiguration : ILogConfiguration
    {
        bool EnableConsole { get; }
        bool EnableDebug { get; }
    }
}
