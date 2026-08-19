using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGeneratorUtilityServices.Utilities
{
    public interface IVersionChecker
    {
        void CheckVersion(
           SourceProductionContext context ,
           INamedTypeSymbol symbol ,
           AnalyzerConfigOptionsProvider options
        );
    }
}
