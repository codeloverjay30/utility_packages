using System.Collections.Concurrent;

namespace ProgrammingLanguageUtilityServices;

public static class ProgrammingLanguageHelper
{
    public static ConcurrentDictionary<string,ProgammingLanguageInfo> GetDefaultProgrammingLanguagePatterns()
    {
        var defaultProgrammingLanguagePatterns = new ConcurrentDictionary<string, ProgammingLanguageInfo>()
        {
            ["csharp"] = new ProgammingLanguageInfo
            {
                LowercasedName = "csharp",
                DisplayedName = "C#",
                SignatureTemplate = new SignatureTemplateInfo
                {
                    FunctionDefintionKeyword = @"void",
                    FunctionDefinitionSymbol = FunctionDefinitionSymbolOptions.WrappedByParentheses,
                },
                FileExtension = "*.cs",
            },

            ["fsharp"] = new ProgammingLanguageInfo
            {
                LowercasedName = "fsharp",
                DisplayedName = "F#",
                SignatureTemplate = new SignatureTemplateInfo
                {
                    FunctionDefintionKeyword = @"let",
                    FunctionDefinitionSymbol = FunctionDefinitionSymbolOptions.Whitespace,
                },
                FileExtension = "*.fs",
            },
            ["python"] = new ProgammingLanguageInfo
            {
                LowercasedName = "python",
                DisplayedName = "Python",
                SignatureTemplate = new SignatureTemplateInfo
                {
                    FunctionDefintionKeyword = @"def",
                    FunctionDefinitionSymbol = FunctionDefinitionSymbolOptions.WrappedByParentheses,
                },
                FileExtension = "*.py",
            },
            ["go"] = new ProgammingLanguageInfo
            {
                LowercasedName = "go",
                DisplayedName = "Go",
                SignatureTemplate = new SignatureTemplateInfo
                {
                    FunctionDefintionKeyword = @"func",
                    FunctionDefinitionSymbol = FunctionDefinitionSymbolOptions.WrappedByParentheses,
                },
                FileExtension = "*.go",
            },
        };

        return defaultProgrammingLanguagePatterns;
    }
}
