using Microsoft.CodeAnalysis;

namespace MoqUtilityServices;

public interface IExtensionMethodAnalyzer
{
    /// <summary>
    /// Track the stack trace of <paramref name="currentMethod"/>
    /// Then added all nodes under <paramref name="currentNode"/> into the child list <see cref="global::MoqUtilityServices.CallGraphNode.ChildNodes"/> of <paramref name="currentNode"/>
    /// <summary>
    /// <param name="currentMethod">current method for analysis</param>
    /// <param name="visited">all visited nodes</param>
    /// <param name="currentNode">current node that is analyzed</param>
    void TraceCallGraph(
        IMethodSymbol currentMethod,
        HashSet<IMethodSymbol> visited,
        CallGraphNode currentNode
    );
}
