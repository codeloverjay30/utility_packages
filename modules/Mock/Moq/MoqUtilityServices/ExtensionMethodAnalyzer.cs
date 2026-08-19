using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MoqUtilityServices;

public class ExtensionMethodAnalyzer : IExtensionMethodAnalyzer
{
    private readonly SemanticModel _semanticModel;
    public ExtensionMethodAnalyzer(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    /// <inheritdoc cref="global::MoqUtilityServices.IExtensionMethodAnalyzer.TraceCallGraph(IMethodSymbol, HashSet{IMethodSymbol}, CallGraphNode)"/>
    public void TraceCallGraph(
        IMethodSymbol currentMethod,
        HashSet<IMethodSymbol> visited,
        CallGraphNode currentNode
    )
    {
        // 1. 防禦性設計：避免循環呼叫導致編譯期 Stack Overflow
        if (!visited.Add(currentMethod))
        {
            return;
        }
        // 2. 取得該方法的語法樹參考以掃描原始碼本體
        var syntaxReference = currentMethod.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference is null)
        {
            return; // 若無原始碼（如第三方DLL），則終止深挖
        }
        var methodDeclaration = (MethodDeclarationSyntax)syntaxReference.GetSyntax();
        // 3. 找出方法體內所有呼叫行為 (Invocations)
        var invocations =
       methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();
        foreach (var invocation in invocations)
        {
            // 4. 透過語意模型看穿呼叫本質
            var symbolInfo = _semanticModel.GetSymbolInfo(invocation);
            if (symbolInfo.Symbol is not IMethodSymbol symbol)
            {
                continue;
            }
            // 狀況 A：如果呼叫的是 Interface 的方法 -> 記錄為 Mock 目標
            if (symbol.ContainingType.TypeKind == TypeKind.Interface)
            {
                currentNode.RequiredMockInterfaces.Add(symbol);
            }
            // 狀況 B：如果呼叫的是另一個擴充方法或靜態方法 -> 建立子節點並遞迴深挖
            else if (symbol.IsExtensionMethod || symbol.IsStatic)
            {
                var childNode = new CallGraphNode(symbol);
                currentNode.ChildNodes.Add(childNode);
                // 【關鍵關鍵】：點對點遞迴鑽進去
                TraceCallGraph(symbol, visited, childNode);
            }
        }
    }
}
