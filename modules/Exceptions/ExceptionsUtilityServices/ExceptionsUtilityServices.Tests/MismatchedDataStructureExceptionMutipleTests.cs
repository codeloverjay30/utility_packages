using System.Exceptions;
using System.Reflection;
using FluentAssertions;

namespace ExceptionsUtilityServices.Tests;

public class MismatchedDataStructureExceptionMutipleTests
{
    // 建立測試資料源：克服 InlineData 無法傳入非編譯期常數的限制
    public static IEnumerable<object[]> MismatchedTypeTestData =>
        new List<object[]>
        {
            new object[] { typeof(string), typeof(int), "Invalid type, expected {0} but was {1}", "String", "Int32", "expected" },
            new object[] { typeof(decimal), typeof(bool), "Failed: {0} vs {1}", "Decimal", "Boolean", "expected" },
            new object[] { typeof(DateTime), typeof(Guid), "Mismatched {0} and {1}", "DateTime", "Guid", "expected" }
        };

    [Theory]
    [MemberData(nameof(MismatchedTypeTestData))]
    public void Create_WithTypeArguments_ShouldConstructCorrectException_MultipleTestCases(
        Type expectedType,
        Type actualType,
        string format,
        string shouldContainExceptionMessage1,
        string shouldContainExceptionMessage2,
        string shouldBeParamName)
    {
        // Arrange
        // 核心防禦：由於型別是動態傳入的，必須透過反射 (Reflection) 來動態建構泛型方法
        Type openGenericType = typeof(MismatchedDataStructureException<,>);
        Type closedGenericType = openGenericType.MakeGenericType(expectedType, actualType);
        
        MethodInfo createMethod = closedGenericType.GetMethod(
            nameof(MismatchedDataStructureException<object, object>.Create), 
            new[] { typeof(Type), typeof(Type), typeof(string) }) 
            ?? throw new InvalidOperationException($"Method 'Create' not found on technical type {closedGenericType.FullName}");

        // Act
        // 動態執行：MismatchedDataStructureException<T1, T2>.Create(expectedType, actualType, format)
        var exception = createMethod.Invoke(null, new object[] { expectedType, actualType, format }) as Exception;

        // Assert
        // 嚴格遵守防禦性與 FluentAssertions 規範，避免 Null 參考副作用
        exception.Should().NotBeNull();
        
        // 動態驗證 Exception 的 Message 與 ParamName
        exception!.Message.Should().Contain(shouldContainExceptionMessage1)
                 .And.Contain(shouldContainExceptionMessage2);
                 
        // 透過反射取得 ParamName 屬性進行驗證
        PropertyInfo? paramNameProp = exception.GetType().GetProperty("ParamName");
        paramNameProp.Should().NotBeNull();
        
        string? paramNameValue = paramNameProp!.GetValue(exception) as string;
        paramNameValue.Should().Be(shouldBeParamName);
    }
}
