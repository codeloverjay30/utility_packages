using System;
using System.Collections.Generic;
using System.Text;

namespace ILoggerBuilderFactoryServices
{
    public interface IFileLogConfiguration : ILogConfiguration
    {
        string? LogFilePath { get; }
        bool EnableFileLogging => !string.IsNullOrEmpty(LogFilePath);
    }
}
