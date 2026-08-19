using System.Linq.Expressions;

namespace ObfuscationServices;

public interface ISecurityVisitor
{
    Func<TInput, TOutput> Protect<TInput, TOutput>(Expression<Func<TInput, TOutput>> origin);
}
