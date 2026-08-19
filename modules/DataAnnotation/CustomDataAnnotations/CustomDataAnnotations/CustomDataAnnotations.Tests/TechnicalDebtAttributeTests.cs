using Xunit;
using FluentAssertions;
using CustomDataAnnotations.Maintenance;
using System.Reflection;

namespace CustomDataAnnotations.Tests
{
    public class TechnicalDebtAttributeTests
    {
        [Fact]
        public void Constructor_ShouldSetCategory_WhenOnlyCategoryIsProvided()
        {
            // Arrange
            var category = CategoryType.CodeSmell;

            // Act
            var attribute = new TechnicalDebtAttribute(category);

            // Assert
            attribute.Category.Should().Be(category);
            attribute.BetterAlternative.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_ShouldSetAllProperties_WhenCategoryAndAlternativeAreProvided()
        {
            // Arrange
            var category = CategoryType.PrimitiveObsessionIssue;
            var alternative = "Use CustomerModel instead of multiple strings";

            // Act
            var attribute = new TechnicalDebtAttribute(category , alternative);

            // Assert
            attribute.Category.Should().Be(category);
            attribute.BetterAlternative.Should().Be(alternative);
        }

        [Theory]
        [InlineData(CategoryType.SecurityVulnerability , "Update to OAuth 2.0")]
        [InlineData(CategoryType.DeprecatedApiOfOutdatedFrameworkIssue , "")]
        public void Attribute_AppliedToMethod_ShouldBeRetrievableViaReflection(CategoryType category , string alternative)
        {
            // Arrange & Act
            // 這裡模擬一個使用該 Attribute 的類別
            var method = typeof(TestSubject).GetMethod(nameof(TestSubject.LegacyMethod));
            var attribute = method?.GetCustomAttribute<TechnicalDebtAttribute>();

            // Assert
            attribute.Should().NotBeNull();
            // 注意：這裡我們測試 TestSubject 上標記的實際值 (固定為 CodeSmell)
            // 如果要測試多個案例，建議建立多個測試類別或動態檢查
            attribute.Category.Should().Be(CategoryType.CodeSmell);
        }

        // 用於測試反射的輔助類別
        private class TestSubject
        {
            [TechnicalDebt(CategoryType.CodeSmell , "Use RefactoredMethod instead")]
            public void LegacyMethod() { }
        }
    }
}
