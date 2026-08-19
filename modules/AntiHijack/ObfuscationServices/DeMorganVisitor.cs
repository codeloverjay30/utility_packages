using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationServices;

/// <summary>
/// Use DeMorgan principle to convert binary expression
/// </summary>
public class DeMorganVisitor : ExpressionVisitor
{
    protected override Expression VisitBinary(BinaryExpression node)
    {
        // fetch all nodes containing `&&`
        if (node.NodeType == ExpressionType.AndAlso)
        {
            // `Left && Right` => `!(!Left || !Right)`      
            var notLeft = Expression.Not(node.Left);
            var notRight = Expression.Not(node.Right);
            var combinedOr = Expression.OrElse(notLeft, notRight);

            return Expression.Not(combinedOr);
        }

        // fetch all nodes containing `||`
        if (node.NodeType == ExpressionType.OrElse)
        {
            // `Left || Right` => `!(!Left && !Right)`
            var notLeft = Expression.Not(node.Left);
            var notRight = Expression.Not(node.Right);
            var combinedAnd = Expression.AndAlso(notLeft, notRight);

            return Expression.Not(combinedAnd);
        }

        return base.VisitBinary(node);
    }
}
    
