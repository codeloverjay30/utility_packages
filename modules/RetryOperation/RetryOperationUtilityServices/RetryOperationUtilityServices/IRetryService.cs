using System;
using System.Collections.Generic;
using System.Text;

namespace RetryOperationUtilityServices
{
    public interface IRetryService
    {
        Task<T> ExecuteAsync<T>(
            Func<Task<T>> action ,
            Func<Exception , bool>? isTransient = null ,
            CancellationToken ct = default
        );
    }
}
