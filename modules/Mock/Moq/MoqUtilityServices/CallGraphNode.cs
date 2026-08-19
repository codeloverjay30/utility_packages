namespace MoqUtilityServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a node in the method call graph used for analyzing method dependencies and mock requirements.
/// </summary>
public class CallGraphNode
{
    /// <summary>
    /// Gets the method symbol associated with this node.
    /// </summary>
    public IMethodSymbol MethodSymbol { get; }

    /// <summary>
    /// Gets the list of child nodes representing invoked static or extension methods.
    /// </summary>
    public List<CallGraphNode> ChildNodes { get; } = new();

    /// <summary>
    /// Gets the set of interface methods required to be mocked during testing.
    /// </summary>
    public HashSet<IMethodSymbol> RequiredMockInterfaces { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CallGraphNode"/> class.
    /// </summary>
    /// <param name="symbol">The method symbol for this node.</param>
    public CallGraphNode(IMethodSymbol symbol) => MethodSymbol = symbol;
}