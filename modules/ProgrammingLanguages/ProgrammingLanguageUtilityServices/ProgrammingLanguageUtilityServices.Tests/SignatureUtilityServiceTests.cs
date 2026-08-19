using System.Collections.Concurrent;
using FluentAssertions;
using Moq;

namespace ProgrammingLanguageUtilityServices.Tests;

/// <summary>
/// Contains defensive and comprehensive unit tests for the <see cref="SignatureUtilityService"/> class.
/// </summary>
public class SignatureUtilityServiceTests
{
    private readonly SignatureUtilityService _sutWithDefaultPatterns;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignatureUtilityServiceTests"/> class with default dependencies.
    /// </summary>
    public SignatureUtilityServiceTests()
    {
        // System Under Test (SUT) initialized with null to force fallback to default patterns safely
        _sutWithDefaultPatterns = new SignatureUtilityService(null!);
    }

    /// <summary>
    /// Verifies that <see cref="SignatureUtilityService.IsSignatureMatched"/> correctly matches 
    /// valid function signatures for various built-in languages using default patterns.
    /// </summary>
    /// <param name="language">The programming language identifier.</param>
    /// <param name="content">The code snippet containing the function definition.</param>
    /// <param name="methodName">The expected name of the method/function.</param>
    [Theory]
    [InlineData("csharp", "public void CalculateRoute(int id)", "CalculateRoute")]
    [InlineData("CSHARP", "void ProcessData()", "ProcessData")] // Verifies case insensitivity
    [InlineData("fsharp", "let aggregateResult x y =", "aggregateResult")]
    [InlineData("python", "def fetch_user_profile(user_id):", "fetch_user_profile")]
    public void IsSignatureMatched_ValidDefaultLanguagePatterns_ReturnsTrue(
        string language, 
        string content, 
        string methodName)
    {
        // Arrange
        var signatureInfo = new SignatureInfo { MethodName = methodName };

        // Act
        bool result = _sutWithDefaultPatterns.IsSignatureMatched(content, language, signatureInfo);

        // Assert
        result.Should().BeTrue($"because the content '{content}' conforms to valid {language} function signature syntax.");
    }

    /// <summary>
    /// Verifies that <see cref="SignatureUtilityService.IsSignatureMatched"/> returns false 
    /// when the code snippet does not match the specified function signature layout.
    /// </summary>
    /// <param name="language">The programming language identifier.</param>
    /// <param name="content">The malformed or mismatched code snippet.</param>
    /// <param name="methodName">The expected name of the method/function.</param>
    [Theory]
    [InlineData("csharp", "public int CalculateRoute(int id)", "CalculateRoute")] // Wrong keyword (int instead of void)
    [InlineData("fsharp", "letCalculateRoute x =", "CalculateRoute")] // Missing spacing after keyword
    [InlineData("python", "defExecuteTask:", "ExecuteTask")] // Missing parentheses/spacing for Python layout
    public void IsSignatureMatched_MismatchedDefaultLanguagePatterns_ReturnsFalse(
        string language, 
        string content, 
        string methodName)
    {
        // Arrange
        var signatureInfo = new SignatureInfo { MethodName = methodName };

        // Act
        bool result = _sutWithDefaultPatterns.IsSignatureMatched(content, language, signatureInfo);

        // Assert
        result.Should().BeFalse($"because the content '{content}' does not follow the signature rules defined for {language}.");
    }

    /// <summary>
    /// Ensures that passing a null reference for the programming language parameter 
    /// throws an <see cref="ArgumentNullException"/> defensively.
    /// </summary>
    [Fact]
    public void IsSignatureMatched_NullProgrammingLanguage_ThrowsArgumentNullException()
    {
        // Arrange
        var signatureInfo = new SignatureInfo { MethodName = "AnyMethod" };
        string nullLanguage = null!;

        // Act
        Action act = () => _sutWithDefaultPatterns.IsSignatureMatched("void AnyMethod()", nullLanguage, signatureInfo);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("programmingLanguage");
    }

    /// <summary>
    /// Ensures that querying an unsupported language throws a <see cref="NotSupportedException"/> 
    /// with an explicit and detailed exception message.
    /// </summary>
    [Fact]
    public void IsSignatureMatched_UnsupportedLanguage_ThrowsNotSupportedExceptionWithExpectedMessage()
    {
        // Arrange
        var signatureInfo = new SignatureInfo { MethodName = "SomeMethod" };
        string unsupportedLanguage = "typescript";

        // Act
        Action act = () => _sutWithDefaultPatterns.IsSignatureMatched("function SomeMethod()", unsupportedLanguage, signatureInfo);

        // Assert
        act.Should().Throw<NotSupportedException>()
           .WithMessage($"The {unsupportedLanguage} is NOT supported at present");
    }

    /// <summary>
    /// Validates that <see cref="SignatureUtilityService"/> behaves correctly when injected with 
    /// a custom, isolated dictionary of language definitions, preventing pollution of global states.
    /// </summary>
    [Fact]
    public void IsSignatureMatched_CustomInjectedPatterns_SuccessfullyMatchesCustomRules()
    {
        // Arrange
        var customPatterns = new ConcurrentDictionary<string, ProgammingLanguageInfo>();
        customPatterns.TryAdd("go", new ProgammingLanguageInfo
        {
            LowercasedName = "go",
            DisplayedName = "Go",
            SignatureTemplate = new SignatureTemplateInfo
            {
                FunctionDefintionKeyword = "func",
                FunctionDefinitionSymbol = FunctionDefinitionSymbolOptions.WrappedByParentheses
            },
            FileExtension = "*.go"
        });

        var sutWithCustomPatterns = new SignatureUtilityService(customPatterns);
        var signatureInfo = new SignatureInfo { MethodName = "main" };
        string content = "func main() { }";

        // Act
        bool result = sutWithCustomPatterns.IsSignatureMatched(content, "go", signatureInfo);

        // Assert
        result.Should().BeTrue("because the custom injected language pattern 'go' matches the code structure.");
    }

    /// <summary>
    /// Guarantees that when custom patterns are injected, standard default patterns (e.g., C#) 
    /// are no longer recognized, enforcing strict containment and avoiding unexpected side effects.
    /// </summary>
    [Fact]
    public void IsSignatureMatched_CustomInjectedPatterns_DoesNotFallbackToDefaultPatterns()
    {
        // Arrange
        var customPatterns = new ConcurrentDictionary<string, ProgammingLanguageInfo>();
        // Only inject Go, leaving CSharp out of this instance completely
        customPatterns.TryAdd("go", new ProgammingLanguageInfo
        {
            LowercasedName = "go",
            DisplayedName = "Go",
            SignatureTemplate = new SignatureTemplateInfo
            {
                FunctionDefintionKeyword = "func",
                FunctionDefinitionSymbol = FunctionDefinitionSymbolOptions.WrappedByParentheses
            },
            FileExtension = "*.go"
        });

        var sutWithCustomPatterns = new SignatureUtilityService(customPatterns);
        var signatureInfo = new SignatureInfo { MethodName = "Calculate" };

        // Act
        Action act = () => sutWithCustomPatterns.IsSignatureMatched("void Calculate()", "csharp", signatureInfo);

        // Assert
        act.Should().Throw<NotSupportedException>()
           .WithMessage("The csharp is NOT supported at present");
    }
}