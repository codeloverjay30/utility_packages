using System;
using System.Collections.Generic;
using System.Text;

namespace ExceptionHandlingUtilityServices
{
    public interface IExceptionUtilityService
    {
        void FlattenAndProcess(Action<Exception> action);
    }
}
