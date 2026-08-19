using Moq;
using System.Reflection;
using System.Linq.Expressions;
using Xunit;
using FluentAssertions;
using ReflectionUtilityServices;
using ExpressionTreeUtilityServices;

namespace ReflectionUtilityServices.Tests
{
    public partial class ReflectionUtilityServiceTests
    {
        [Fact]
        public void AddFastDelegate_StaticMethod_ShouldExecuteAndReturnCorrectValue()
        {
            // Arrange
            var methodInfo = typeof(TestTarget).GetMethod(nameof(TestTarget.StaticAdd));

            // 使用 Mock 的 Callback 或 Returns 邏輯，確保使用 Service 傳來的 argumentsParam
            _mockExpressionTreeService
                .Setup(s => s.CreateParameterExpressions(methodInfo! , It.IsAny<ParameterExpression>()))
                .Returns((MethodInfo mi , ParameterExpression args) => new Expression [ ]
                {
                    // 使用 Service 傳過來的 args 進行 ArrayIndex
                    Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(0)), typeof(int)),
                    Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(1)), typeof(int))
                });

            // Act
            _service.AddFastDelegate(methodInfo!);
            var fastDelegate = _service.FastDelegates.Last();
            var result = fastDelegate!(null! , new object [ ] { 10 , 20 });

            // Assert
            result.Should().Be(30);
        }

        [Fact]
        public void AddFastDelegate_InstanceMethod_ShouldChangeTargetState()
        {
            // Arrange
            var target = new TestTarget();
            var methodInfo = typeof(TestTarget).GetMethod(nameof(TestTarget.UpdateStatus));

            _mockExpressionTreeService
                .Setup(s => s.CreateParameterExpressions(methodInfo! , It.IsAny<ParameterExpression>()))
                .Returns(Array.Empty<Expression>());

            // Act
            _service.AddFastDelegate(methodInfo!);
            var fastDelegate = _service.FastDelegates.Last();
            var result = fastDelegate!(target , null);

            // Assert
            result.Should().BeNull(); // Void 方法應回傳 null
            target.IsUpdated.Should().BeTrue(); // 驗證副作用是否發生
        }

        [Fact]
        public void AddFastDelegate_WithReturnType_ShouldReturnExpectedString()
        {
            // Arrange
            var target = new TestTarget();
            var methodInfo = typeof(TestTarget).GetMethod(nameof(TestTarget.SayHello));

            _mockExpressionTreeService
                .Setup(s => s.CreateParameterExpressions(methodInfo! , It.IsAny<ParameterExpression>()))
                .Returns(Array.Empty<Expression>());

            // Act
            _service.AddFastDelegate(methodInfo!);
            var fastDelegate = _service.FastDelegates.Last();
            var result = fastDelegate!(target , null);

            // Assert
            result.Should().Be("Hello World");
        }

        [Fact]
        public void AddFastDelegates_MultipleMethods_ShouldAllBeInList()
        {
            // Arrange
            var methods = typeof(TestTarget).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.DeclaringType == typeof(TestTarget)).ToList();

            // 根據 MethodInfo 的參數數量動態回傳 Expression 陣列
            _mockExpressionTreeService
                .Setup(s => s.CreateParameterExpressions(It.IsAny<MethodInfo>() , It.IsAny<ParameterExpression>()))
                .Returns((MethodInfo mi , ParameterExpression args) =>
                {
                    var parameters = mi.GetParameters();
                    // 為每個參數建立一個轉換表達式 (從 object[] args 中取出)
                    return parameters.Select((p , i) =>
                        Expression.Convert(Expression.ArrayIndex(args , Expression.Constant(i)) , p.ParameterType)
                    ).ToArray();
                });

            // Act
            _service.AddFastDelegates(methods);

            // Assert
            _service.FastDelegates.Should().HaveCount(methods.Count);
            _service.FastDelegates.Should().NotContainNulls();
        }
    }
}
