using SharedUtilityServices;

namespace Microsoft.Extensions.DependencyInjection; // 使用標準命名空間，讓使用者不用額外 using

public static class BufferServiceCollectionExtensions
{
    /// <summary>
    /// Register `IByteArrayPool` to services.
    /// </summary>
    public static IServiceCollection AddSharedByteArrayPool(this IServiceCollection services)
    {
        // 註冊為 Singleton，因為 ArrayPool.Shared 本身就是執行緒安全且全域唯一的
        services.AddSingleton<IByteArrayPool, SharedByteArrayPool>();
        return services;
    }
}
