using Polly;
using Polly.CircuitBreaker;
using Polly.Demo;
using Polly.Fallback;
using Polly.Hedging;
using Polly.Retry;
using Polly.Timeout;
using System.Diagnostics;
using System.Threading.RateLimiting;

var optionsOnHedging = new HedgingStrategyOptions<string>
{
    OnHedging = static args =>
    {
        Console.WriteLine($"OnHedging: Attempt number {args.AttemptNumber}");
        return default;
    }
};

// To add a timeout and listen for timeout events
var optionsOnTimeout = new TimeoutStrategyOptions
{
    TimeoutGenerator = static args =>
    {
        // Note: the timeout generator supports asynchronous operations
        return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(3));
    } ,
    OnTimeout = static args =>
    {
        Console.WriteLine($"{args.Context.OperationKey}: Execution timed out after {args.Timeout.TotalSeconds} seconds.");
        return default;
    }
};

// 1. 透過 Builder 組合 API 策略
var dataAccessPipeline = new ResiliencePipelineBuilder<string>()
    .AddFallback(new FallbackStrategyOptions<string>
    {
        ShouldHandle = new PredicateBuilder<string>().Handle<Exception>() ,
        FallbackAction = args => new ValueTask<Outcome<string>>(Outcome.FromResult("這裡是備份資料"))
    })
    .AddRetry(new RetryStrategyOptions<string>
    {
        MaxRetryAttempts = 1000 , // 最大重試次數
        BackoffType = DelayBackoffType.Constant
    })
    .AddTimeout(optionsOnTimeout) // 每次嘗試不能超過 3 秒
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions<string>
    {
        ShouldHandle = new PredicateBuilder<string>().Handle<Exception>() ,
        FailureRatio = 0.5 , // 如果失敗率超過 50%，則斷路器會觸發
        SamplingDuration = TimeSpan.FromSeconds(30) , // 在 30 秒內，如果失敗率超過 50%，則斷路器會觸發
        MinimumThroughput = 10 , // 在 30 秒內，至少有 10 次請求，才會考慮斷路器的觸發
        BreakDuration = TimeSpan.FromMinutes(1) , // 斷路器觸發後，會在 1 分鐘內阻止所有請求

        // 可選：定義被熔斷時呼叫的Callback
        OnOpened = args =>
        {
            Console.WriteLine("--- [Warning] The circuit breaker is opened! ---");
            Console.WriteLine($"reason：{args.Outcome.Exception?.Message}");
            Console.WriteLine($"熔斷持續時間：{args.BreakDuration}");
            return default; // 回傳 ValueTask
        } ,
        // 可選：定義恢復時呼叫的Callback
        OnClosed = args =>
        {
            Console.WriteLine("--- [Info] The service has returned to normal, the circuit breaker is closed. ---");
            return default;
        } ,
    }).AddFallback(new FallbackStrategyOptions<string>
    {
        ShouldHandle = new PredicateBuilder<string>().Handle<BrokenCircuitException>() , // 當斷路器被觸發時，會進入這個 Fallback
        FallbackAction = args =>
        {
            Console.WriteLine("The service is currently unavailable due to the circuit breaker being open.");
            return new ValueTask<Outcome<string>>(Outcome.FromResult("Service stop"));
        }
    })
    .AddRateLimiter(new SlidingWindowRateLimiter(
        new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100 ,
            SegmentsPerWindow = 4 ,
            Window = TimeSpan.FromMinutes(1)
        })).AddHedging(optionsOnHedging)
    .Build();
    
int counter = 0;
// 2. 執行並保護你的程式碼
var result = await dataAccessPipeline.ExecuteAsync(async token =>
{
    Console.WriteLine($"Attempt: {counter}");
    try {
        // 例如：呼叫政府專案的舊型 API 或資料庫操作
        var dataService = new DataService();
        
        var result = await dataService.GetDataAsync();
        counter++;
        return result;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error occurred: {ex.Message}");
        counter ++;
        throw; // 重新拋出異常以便 Polly 可以捕捉並進行重試
    }
});

Console.WriteLine($"Final Result: {result}");
Console.WriteLine($"All tasks completed with attempts: {counter}");
Console.ReadKey();
