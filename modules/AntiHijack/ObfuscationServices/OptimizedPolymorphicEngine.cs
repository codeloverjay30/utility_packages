using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace ObfuscationServices;

public class OptimizedPolymorphicEngine
{
    // 快取編譯後的表達式防線，避免高頻調用時因 Expression.Compile() 導致動態組譯件塞滿記憶體
    private static readonly ConcurrentDictionary<string, Delegate> _compiledLogicCache = new();

    public Func<T, bool> GetPolymorphicPredicate<T>(Expression<Func<T, bool>> expression, string cacheKey)
    {
        return (Func<T, bool>)_compiledLogicCache.GetOrAdd(cacheKey, _ =>
        {
            // 在此處套用 DeMorganVisitor 或 SecurityVisitor 的代碼邏輯變換
            var securityVisitor = new SecurityVisitor(); 
            var transformed = (Expression<Func<T, bool>>)securityVisitor.Visit(expression);
            return transformed.Compile();
        });
    }
}