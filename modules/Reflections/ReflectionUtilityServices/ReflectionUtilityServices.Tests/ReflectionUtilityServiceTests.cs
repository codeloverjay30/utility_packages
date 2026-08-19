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
        private readonly Mock<IExpressionTreeUtilityService> _mockExpressionTreeService;
        private readonly ReflectionUtilityService _service;

        public ReflectionUtilityServiceTests()
        {
            _mockExpressionTreeService = new Mock<IExpressionTreeUtilityService>();
            _service = new ReflectionUtilityService(_mockExpressionTreeService.Object);
        }

        [Fact]
        public void AddFastDelegate_StaticMethod_ShouldCompileSuccessfully()
        {
            // Arrange
            var methodInfo = typeof(TestTarget).GetMethod(nameof(TestTarget.StaticMethod));
            _mockExpressionTreeService
                .Setup(s => s.CreateParameterExpressions(It.IsAny<MethodInfo>() , It.IsAny<ParameterExpression>()))
                .Returns(new Expression [ 0 ]);

            // Act
            var exception = Record.Exception(() => _service.AddFastDelegate(methodInfo!));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public void AddFastDelegate_InstanceMethod_ShouldCompileSuccessfully()
        {
            // Arrange
            var methodInfo = typeof(TestTarget).GetMethod(nameof(TestTarget.InstanceMethod));
            _mockExpressionTreeService
                .Setup(s => s.CreateParameterExpressions(It.IsAny<MethodInfo>() , It.IsAny<ParameterExpression>()))
                .Returns(new Expression [ 0 ]);

            // Act
            var exception = Record.Exception(() => _service.AddFastDelegate(methodInfo!));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public void AddFastDelegate_VoidMethod_ShouldHandleNullReturn()
        {
            // Arrange
            var methodInfo = typeof(TestTarget).GetMethod(nameof(TestTarget.VoidMethod));
            _mockExpressionTreeService
                .Setup(s => s.CreateParameterExpressions(It.IsAny<MethodInfo>() , It.IsAny<ParameterExpression>()))
                .Returns(new Expression [ 0 ]);

            // Act & Assert
            var exception = Record.Exception(() => _service.AddFastDelegate(methodInfo!));
            exception.Should().BeNull();
        }

        [Fact]
        public void AddFastDelegates_MultipleMethods_ShouldProcessAll()
        {
            // Arrange
            var methods = typeof(TestTarget).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.DeclaringType == typeof(TestTarget)).ToList();

            // 修正點：根據不同方法的參數數量動態回傳 Expression 陣列，避免 ArgumentException
            _mockExpressionTreeService
                .Setup(s => s.CreateParameterExpressions(It.IsAny<MethodInfo>() , It.IsAny<ParameterExpression>()))
                .Returns((MethodInfo mi , ParameterExpression args) =>
                {
                    var parameters = mi.GetParameters();
                    return parameters.Select((p , i) =>
                        Expression.Convert(Expression.ArrayIndex(args , Expression.Constant(i)) , p.ParameterType)
                    ).ToArray();
                });

            // Act
            var exception = Record.Exception(() => _service.AddFastDelegates(methods));

            // Assert
            exception.Should().BeNull();
            _service.FastDelegates.Count.Should().Be(methods.Count);
        }
    }

    // 用於測試的目標類別
    public class TestTarget
    {
        public static void StaticMethod() { }
        public string InstanceMethod() => "Hello";
        public void VoidMethod() { }

        public bool IsUpdated { get; private set; }

        public static int StaticAdd(int a , int b) => a + b;

        public void UpdateStatus() => IsUpdated = true;

        public string SayHello() => "Hello World";
    }
}
