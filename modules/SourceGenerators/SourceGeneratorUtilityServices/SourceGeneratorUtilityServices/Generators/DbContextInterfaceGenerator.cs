using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SourceGeneratorUtilityServices.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGeneratorUtilityServices.Generators
{
    [Generator]
    public class DbContextInterfaceGenerator : IIncrementalGenerator
    {
        // 預先實體化版本檢查器(而不是在Execute方法中才實體化)因為每編譯一次就會呼叫Execute多次，這樣可以避免重複實體化造成的效能問題。
        private static readonly IVersionChecker _versionChecker = new VersionChecker();

        /// <summary>
        /// Initializes the incremental source generator by configuring syntax and semantic analysis pipelines.
        /// </summary>
        /// <remarks>
        /// Call this method from the generator's entry point to set up syntax filtering,
        /// semantic symbol extraction, and source output registration. This method should be invoked once during
        /// generator initialization.
        /// </remarks>
        /// <param name="context">The context used to register source outputs and configure the generator's initialization logic.</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 篩選：尋找所有類別，並提取我們感興趣的「語義符號 (Symbol)」
            var beanSymbols = context.SyntaxProvider
                        .CreateSyntaxProvider(
                            predicate: static (s , _) => s is ClassDeclarationSyntax ,
                            transform: static (ctx , _) => GetSemanticTarget(ctx))
                        .Where(static m => m is not null);

            var configOptions = context.AnalyzerConfigOptionsProvider;
            var combinedSource = beanSymbols.Combine(configOptions);

            context.RegisterSourceOutput(combinedSource , static (spc , source) =>
            {
                // 利用靜態欄位呼叫解耦後的檢查器
                _versionChecker.CheckVersion(spc , source.Left! , source.Right);

                // 執行生成
                Execute(spc , source.Left! , source.Right);
            });
        }

        /// <summary>
        /// Central method responsible for generating source code based on the provided semantic symbol and configuration options.
        /// This method is called for each class that meets the criteria defined in the <seealso cref="Initialize"/> method.
        /// It performs version checks, reads configuration values, and
        /// generates the appropriate source code for the interface based on the class's namespace and name.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="symbol"></param>
        /// <param name="options"></param>
        private static void Execute(SourceProductionContext context , INamedTypeSymbol symbol , AnalyzerConfigOptionsProvider options)
        {
            _versionChecker.CheckVersion(context , symbol , options);

            // 讀取 .csproj 配置，若無則使用預設值
            options.GlobalOptions.TryGetValue("build_property.AutoInterface_TargetNamespace" , out var targetNS);
            options.GlobalOptions.TryGetValue("build_property.AutoInterface_OutputSuffix" , out var outputSuffix);

            targetNS = targetNS ?? ".Beans";
            outputSuffix = outputSuffix ?? ".Interfaces";

            string beanName = symbol.Name;
            string beanFullName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string interfaceNamespace = symbol.ContainingNamespace.ToDisplayString().Replace(targetNS , outputSuffix);

            var code = _GenerateSourceText(interfaceNamespace, beanName,beanFullName);

            context.AddSource($"I{beanName}Context.g.cs" , SourceText.From(code , Encoding.UTF8));
        }

        /// <summary>
        /// Utility method used at <seealso cref="Initialize"/> method to extract the semantic symbol from the syntax context. This is where you can add additional checks to ensure that the class meets certain criteria (e.g., has specific attributes, inherits from a base class, etc.) before generating code for it.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static INamedTypeSymbol? GetSemanticTarget(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

            // 隱式標記檢查：判斷命名空間是否符合規範
            if(symbol != null && symbol.ContainingNamespace.ToDisplayString().EndsWith(".Beans"))
            {
                return symbol;
            }
            return null;
        }

        /// <summary>
        /// Utility method used at <seealso cref="Execute"/> method to extract the semantic symbol from the syntax context. This is where you can add additional checks to ensure that the class meets certain criteria (e.g., has specific attributes, inherits from a base class, etc.) before generating code for it.
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="beanName"></param>
        /// <param name="beanFullName"></param>
        /// <returns></returns>
        private static string _GenerateSourceText(string ns , string beanName , string beanFullName)
        {
            // 使用 C# 11 原始字串 (Raw String Literals)，雙大括號 {{ }} 用於轉義
            return $$"""
// <auto-generated/>
#nullable enable
using Microsoft.EntityFrameworkCore;

namespace {{ns}}
{
    /// <summary>
    /// 自動生成的 {{beanName}} 資料庫上下文介面
    /// </summary>
    public partial interface I{{beanName}}Context
    {
        DbSet<{{beanFullName}}> {{beanName}}s { get; set; }
    }
}
""";
        }
    }
}
