using System.Linq.Expressions;

namespace ObfuscationServices;

public interface IPolymorphicEngine<TInput, TOutput>
{
    void RegisterVariant(Expression<Func<TInput, TOutput>> variant);

    TOutput Execute(TInput input);
}
