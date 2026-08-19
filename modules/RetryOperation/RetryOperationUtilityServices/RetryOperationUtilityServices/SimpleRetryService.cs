using LoggerFactoryUtilityServices;
using Microsoft.Extensions.Logging;
using RetryOperationUtilityServices.Models;

namespace RetryOperationUtilityServices
{
    public partial class SimpleRetryService(
        LoggerFactoryBaseUtilityService loggerFactoryService ,
        RetryModel retryModel
    ): RetryBaseUtilityService(loggerFactoryService), IRetryService
    {
        private readonly Random _random = new();

        private readonly ILogger _logger = loggerFactoryService.Logger;

        [LoggerMessage(Level = LogLevel.Warning , Message = "{Attempts}th attempts failed. It will retry after {DelayTime}ms")]
        static partial void LogForWaiting(ILogger logger , int Attempts , double DelayTime);

        [LoggerMessage(Level = LogLevel.Error , Message = "Reached maximum retry attempts: {MaxAttempts}")]
        static partial void LogForReachingMaxLimit(ILogger logger, int MaxAttempts);


        /// <summary>
        /// Execute the action (<paramref name="action"/>) with retry technique.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action to be executed</param>
        /// <param name="isTransient">Is the execption worth to retry when it occurs.</param>
        /// <param name="ct">cancellation token</param>
        /// <returns></returns>
        public async Task<T> ExecuteAsync<T>(
            Func<Task<T>> action ,
            Func<Exception , bool>? isTransient = null ,
            CancellationToken ct = default
        )
        {
            int attempts = 0;
            TimeSpan delay = retryModel.InitialDelay;

            while(true)
            {
                ct.ThrowIfCancellationRequested(); // 確保進入迴圈前檢查取消狀態

                try
                {
                    attempts++;
                    return await action();
                }
                catch(Exception ex) when(attempts < retryModel.MaxRetryAttempts && (isTransient?.Invoke(ex) ?? true))
                {
                    LogForWaiting(_logger , attempts , delay.TotalMilliseconds);

                    // 使用 Task.Delay 並傳入 ct 是正確的
                    await Task.Delay(delay , ct);

                    // 改進的退避計算 (包含 MaxDelay 檢查)
                    var factor = retryModel.BackoffMultiplier;
                    // 選擇性加入 Jitter: factor += Random.Shared.NextDouble() * 0.1;

                    long nextTicks = (long)(delay.Ticks * factor);
                    delay = nextTicks > retryModel.MaxDelay.Ticks
                            ? retryModel.MaxDelay
                            : TimeSpan.FromTicks(nextTicks);
                }
                catch(Exception ex)
                {
                    // 這裡可以區分是因為「取消」還是「重試次數耗盡」而失敗
                    if(ct.IsCancellationRequested)
                    {
                        _logger.LogInformation("Retry operation was cancelled.");
                    }
                    else
                    {
                        LogForReachingMaxLimit(_logger ,retryModel.MaxRetryAttempts);
                    }
                    throw;
                }
            }
        }
    }
}
