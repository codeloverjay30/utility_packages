using FluentAssertions;
using LoggingCodeTemplateGenerators.LoggingCodeTemplateGenerators;

namespace LoggingCodeTemplateGenerators.Test
{
    public class LoggingCodeTemplateTests
    {
        [Fact]
        public void BuildPartialClass_WhenUseInterfaceIsTrue_ShouldUseInstanceCall()
        {
            // Arrange
            string serviceName = "_logger";
            string staticServiceName = "StaticLogger";
            string method = "LogEvent";
            string ns = "MyApp.Services";
            string className = "OrderService";
            string args = "\"UserLogin\", 101";
            bool useInterface = true;

            // Act
            string result = LoggingCodeTemplate.BuildPartialClass(
                serviceName , staticServiceName , method , ns , className , args , useInterface);

            // Assert
            result.Should().Contain($"namespace {ns}");
            result.Should().Contain($"public partial class {className}");
            result.Should().Contain($"this.{serviceName}.{method}({args});");
            result.Should().NotContain($"{staticServiceName}.{method}");
        }

        [Fact]
        public void BuildPartialClass_WhenUseInterfaceIsFalse_ShouldUseStaticCall()
        {
            // Arrange
            string serviceName = "_logger";
            string staticServiceName = "GlobalLogger";
            string method = "Notify";
            string ns = "Core.Utils";
            string className = "DataProcessor";
            string args = "404, \"Not Found\"";
            bool useInterface = false;

            // Act
            string result = LoggingCodeTemplate.BuildPartialClass(
                serviceName , staticServiceName , method , ns , className , args , useInterface);

            // Assert
            result.Should().Contain($"namespace {ns}");
            result.Should().Contain($"{staticServiceName}.{method}({args});");
            result.Should().NotContain($"this.{serviceName}");
        }

        [Theory]
        [InlineData("MyNamespace" , "MyClass")]
        [InlineData("System.Collections" , "GenericList")]
        public void BuildPartialClass_ShouldCorrectlyRenderStructure(string ns , string className)
        {
            // Act
            string result = LoggingCodeTemplate.BuildPartialClass(
                "s" , "S" , "M" , ns , className , "" , true);

            // Assert
            // 修正：移除 \n，因為原始字串字面量會忽略開頭的第一個換行
            result.TrimStart().Should().StartWith("namespace");
            result.Should().Contain($"public partial class {className}");
        }
    }
}
