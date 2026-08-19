using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationServices;

public class ConditionalExpressionVisitor : ExpressionVisitor
{
    /// <summary>
    /// Obfuscates the trinary expression (format: `<expression> ? <executed-when-true> : <executed-when-false>`)
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected override Expression VisitConditional(ConditionalExpression node)
    {
        // create a new conditional expression node you want, but adding TickCount > 0 condition check.
        var tickCountProperty = typeof(Environment).GetProperty("TickCount");
        var condition = Expression.Condition(
            Expression.GreaterThan(Expression.Property(null, tickCountProperty), Expression.Constant(0)),
            node,    // real node
            Expression.Constant(false) // false logic (never been executed)
        );

        return condition;
    }
}
    

