using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;
using ExpressionTreeUtilityServices;

namespace ExpressionTreeUtilityServices.Tests
{
    public class ExpressionTreeUtilityServiceTests
    {
        private readonly ExpressionTreeUtilityService _service;

        public ExpressionTreeUtilityServiceTests()
        {
            _service = new ExpressionTreeUtilityService();
        }

        // 定義一個用於測試的虛擬方法
        public void MockMethod(int id , string name , DateTime date) { }

        [Fact]
        public void CreateParameterExpressions_ShouldReturnCorrectExpressions()
        {
            // Arrange
            MethodInfo methodInfo = typeof(ExpressionTreeUtilityServiceTests).GetMethod(nameof(MockMethod));
            ParameterExpression argumentsParam = Expression.Parameter(typeof(object [ ]) , "args");
            ParameterInfo [ ] parameters = methodInfo.GetParameters();

            // Act
            Expression [ ] result = _service.CreateParameterExpressions(methodInfo , argumentsParam);

            // Assert
            Assert.Equal(parameters.Length , result.Length);

            for(int i = 0; i < parameters.Length; i++)
            {
                // 驗證是否為轉換表達式 (Expression.Convert)
                var convertExpr = Assert.IsType<UnaryExpression>(result [ i ]);
                Assert.Equal(ExpressionType.Convert , convertExpr.NodeType);
                Assert.Equal(parameters [ i ].ParameterType , convertExpr.Type);

                // 驗證內部是否為陣列索引存取 (Expression.ArrayIndex)
                var binaryExpr = Assert.IsAssignableFrom<BinaryExpression>(convertExpr.Operand);
                Assert.Equal(ExpressionType.ArrayIndex , binaryExpr.NodeType);

                // 驗證索引值是否正確
                var indexExpr = Assert.IsType<ConstantExpression>(binaryExpr.Right);
                Assert.Equal(i , indexExpr.Value);
            }
        }

        [Fact]
        public void CreateParameterExpressions_WithNoParameters_ShouldReturnEmptyArray()
        {
            // Arrange
            MethodInfo methodInfo = typeof(object).GetMethod(nameof(object.ToString));
            ParameterExpression argumentsParam = Expression.Parameter(typeof(object [ ]) , "args");

            // Act
            Expression [ ] result = _service.CreateParameterExpressions(methodInfo , argumentsParam);

            // Assert
            Assert.Empty(result);
        }
    }
}
