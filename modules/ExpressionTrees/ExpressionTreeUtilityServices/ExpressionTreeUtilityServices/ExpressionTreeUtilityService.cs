using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionTreeUtilityServices
{
    public class ExpressionTreeUtilityService : IExpressionTreeUtilityService
    {
        public Expression [ ] CreateParameterExpressions(
            MethodInfo methodInfo ,
            ParameterExpression argumentsParam
        )
        {
            return methodInfo.GetParameters().Select((p , i) =>
            {
                var index = Expression.Constant(i);
                var accessor = Expression.ArrayIndex(argumentsParam , index);
                return Expression.Convert(accessor , p.ParameterType);
            }).ToArray();
        }
    }
}
