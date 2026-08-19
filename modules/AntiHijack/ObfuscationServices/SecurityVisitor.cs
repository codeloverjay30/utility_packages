using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationServices;

public class SecurityVisitor : ExpressionVisitor
{
    /// <summary>
    /// When visiting a constant node(e.g. an expression `c=5;`), trying to generate a new expression
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected override Expression VisitConstant(ConstantExpression node)
    {
        // In .NET 7+, int,long,decimal etc type will implement INumber<TSelf> interface
        var isNumber = node.Type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumber<>));

        if (isNumber && node.Value != null)
        {
            // get _GenerateConstantExpression method by reflection
            var method = typeof(SecurityVisitor)
                .GetMethod(nameof(_GenerateConstantExpression), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // fill in the generic parameter and invoke it
            var genericMethod = method!.MakeGenericMethod(node.Type);
            var expression = (Expression)genericMethod.Invoke(this, [node]) ?? throw new InvalidCastException("Unknown error!!! The expression is casted to null");
            return expression;
        }

        return base.VisitConstant(node);
    }

    /// <summary>
    /// Obfuscates the provided expression and returns a compiled delegate. 
    /// </summary>
    /// <typeparam name="TInput">The input type of the expression.</typeparam>
    /// <typeparam name="TOutput">The output type of the expression.</typeparam>
    /// <param name="origin">The original expression to be protected.</param>
    /// <returns>A compiled delegate representing the obfuscated expression.</returns>
    public Func<TInput, TOutput> Protect<TInput, TOutput>(Expression<Func<TInput, TOutput>> origin)
    {
        var visitor = new SecurityVisitor();

        var obfuscatedBody = visitor.Visit(origin.Body);

        var finalExpression = Expression.Lambda<Func<TInput, TOutput>>(
            obfuscatedBody,
            origin.Parameters
        );

        // compile the expression
        return finalExpression.Compile();
    }

    private Expression _GenerateConstantExpression<T>(ConstantExpression node)
        where T : INumber<T>
    {
        if (node.Value is not null)
        {
            T value = (T)node.Value;
            T two = T.One + T.One; // 產生泛型的 2
            T part1 = value / two;
            T part2 = value - part1;
            // return a new expression `part1 + part2`
            return Expression.Add(
                Expression.Constant(part1),
                Expression.Constant(part2)
            );
        }

        throw new ArgumentException("Node value can't be null.");
    }
}
    
