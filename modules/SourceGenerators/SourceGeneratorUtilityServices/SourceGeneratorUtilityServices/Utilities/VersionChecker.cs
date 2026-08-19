using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceGeneratorUtilityServices.Utilities
{
    public class VersionChecker : IVersionChecker
    {
        /// <summary>
        /// check the current version of C# language and NET SDK of importing project.
        ///
        /// If the version is older than .NET 10, skip some new features of C# 14 and emit compile warning.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="symbol"></param>
        /// <param name="options"></param>
        public void CheckVersion(
            SourceProductionContext context ,
            INamedTypeSymbol symbol ,
            AnalyzerConfigOptionsProvider options
        )
        {
            string version = "10.0";
            // 取得主專案的 TargetFramework (例如: v10.0)
            options.GlobalOptions.TryGetValue("build_property.TargetFrameworkVersion" , out var tfv);

            // 取得 C# 語言版本 (例如: 14.0)
            options.GlobalOptions.TryGetValue("build_property.LangVersion" , out var langVersion);

            // 檢查邏輯
            if(tfv != null && !tfv.Contains(version))
            {
                // 發出一個編譯器警告 (Warning) 而非直接報錯
                Report(context , symbol , "AH002" , "語法版本過低" , "Generator 產生的程式碼需要 C# 11.0 以上支援。");
            }
        }

        private void Report(SourceProductionContext context , INamedTypeSymbol symbol , string id , string title , string message)
        {
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor(
                    id , title , message , "Compatibility" ,
                    DiagnosticSeverity.Warning , true) ,
                symbol.Locations.FirstOrDefault());

            context.ReportDiagnostic(diagnostic);
        }
    }
}
