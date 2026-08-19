using System.Linq.Expressions;

namespace ObfuscationServices;

public interface IOptimizedPolymorphicEngine
{
    Func<T, bool> GetPolymorphicPredicate<T>(Expression<Func<T, bool>> expression, string cacheKey);
}
