using LoggerFactoryUtilityServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetryOperationUtilityServices
{
    public class RetryBaseUtilityService(
        LoggerFactoryBaseUtilityService loggerFactoryService
    ):RetryAbstractBaseUtilityService(loggerFactoryService)
    {
    }
}
