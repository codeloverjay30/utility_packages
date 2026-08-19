namespace MoqUtilityServices.Tests;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using Xunit;

public class ExtensionMethodAnalyzerTests
{
    /// <summary>
    /// Verifies that TraceCallGraph successfully identifies interface calls and adds them to RequiredMockInterfaces.
    /// </summary>
    [Fact]
    public void TraceCallGraph_WhenInvokingInterfaceMethod_ShouldAddInterfaceToRequiredMockInterfaces()
    {
        // Arrange
        var sourceCode = @"
            using System;
            namespace DemoNamespace
            {
                public interface IService
                {
                    void Execute();
                }

                public static class TargetClass
                {
                    public static void TargetMethod(IService service)
                    {
                        service.Execute();
                    }
                }
            }";

        var (methodSymbol, semanticModel) = GetMethodSymbolAndSemanticModel(sourceCode, "DemoNamespace.TargetClass", "TargetMethod");
        var rootNode = new CallGraphNode(methodSymbol);
        var visited = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var analyzer = new ExtensionMethodAnalyzer(semanticModel);

        // Act
        Action act = () => analyzer.TraceCallGraph(methodSymbol, visited, rootNode);

        // Assert
        act.Should().NotThrow();
        rootNode.RequiredMockInterfaces.Should().ContainSingle(s => s.Name == "Execute");
    }

    /// <summary>
    /// Verifies that TraceCallGraph safely handles methods without source code (e.g., external or metadata symbols) without throwing exceptions.
    /// </summary>
    [Fact]
    public void TraceCallGraph_WhenSyntaxReferenceIsNull_ShouldReturnGracefully()
    {
        // Arrange
        var mockMethodSymbol = new Mock<IMethodSymbol>();
        // Fix: Use ImmutableArray<SyntaxReference> to match modern Roslyn IMethodSymbol signature
        mockMethodSymbol.Setup(m => m.DeclaringSyntaxReferences).Returns(ImmutableArray<SyntaxReference>.Empty);
        
        var mockSemanticModel = new Mock<SemanticModel>(MockBehavior.Strict);
        
        var rootNode = new CallGraphNode(mockMethodSymbol.Object);
        var visited = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var analyzer = new ExtensionMethodAnalyzer(mockSemanticModel.Object);

        // Act
        Action act = () => analyzer.TraceCallGraph(mockMethodSymbol.Object, visited, rootNode);

        // Assert
        act.Should().NotThrow();
        rootNode.ChildNodes.Should().BeEmpty();
        rootNode.RequiredMockInterfaces.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that circular references are properly handled and prevented using the visited set.
    /// </summary>
    [Fact]
    public void TraceCallGraph_WhenCircularReferenceOccurs_ShouldPreventInfiniteRecursion()
    {
        // Arrange
        var sourceCode = @"
            namespace DemoNamespace
            {
                public static class RecursiveClass
                {
                    public static void RecursiveMethod()
                    {
                        RecursiveMethod();
                    }
                }
            }";

        var (methodSymbol, semanticModel) = GetMethodSymbolAndSemanticModel(sourceCode, "DemoNamespace.RecursiveClass", "RecursiveMethod");
        var rootNode = new CallGraphNode(methodSymbol);
        var visited = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var analyzer = new ExtensionMethodAnalyzer(semanticModel);

        // Act
        Action act = () => analyzer.TraceCallGraph(methodSymbol, visited, rootNode);

        // Assert
        act.Should().NotThrow();
        visited.Should().Contain(methodSymbol);
    }

    /// <summary>
    /// Helper method to compile code in-memory and extract the target IMethodSymbol and SemanticModel.
    /// </summary>
    private static (IMethodSymbol MethodSymbol, SemanticModel SemanticModel) GetMethodSymbolAndSemanticModel(string sourceCode, string typeName, string methodName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var model = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();
        
        var methodDecl = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First(m => m.Identifier.Text == methodName);

        var symbol = model.GetDeclaredSymbol(methodDecl) as IMethodSymbol;
        return (symbol!, model);
    }
}