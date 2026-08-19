using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ExpressionTreeUtilityServices
{
    public interface IExpressionTreeUtilityService
    {
        Expression [ ] CreateParameterExpressions(MethodInfo methodInfo , ParameterExpression argumentsParam);
    }
}
