using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace ProgrammingLanguageUtilityServices;

public class SignatureUtilityService : ISignatureUtilityService
{
    private static readonly ConcurrentDictionary<string, ProgammingLanguageInfo> _defaultProgammingLanguageInfoPatterns = ProgrammingLanguageHelper.GetDefaultProgrammingLanguagePatterns();
    private readonly ConcurrentDictionary<string, ProgammingLanguageInfo> _progammingLanguageInfoPatterns;

    public SignatureUtilityService(
        ConcurrentDictionary<string, ProgammingLanguageInfo> progammingLanguageInfoPatterns
    )
    {
        _progammingLanguageInfoPatterns = progammingLanguageInfoPatterns ?? _defaultProgammingLanguageInfoPatterns;
    }
    public bool IsSignatureMatched(
        string content,
        string programmingLanguage,
        SignatureInfo signatureInfo
    )
    {
        ArgumentNullException.ThrowIfNull(programmingLanguage);
        programmingLanguage = programmingLanguage.ToLowerInvariant();
        if (
            _progammingLanguageInfoPatterns.TryGetValue(programmingLanguage, out var progammingLanguageInfo)        
        )
        {
            var functionDefinitionSymbolOptions = progammingLanguageInfo.SignatureTemplate.FunctionDefinitionSymbol;
            string functionKeyword = progammingLanguageInfo.SignatureTemplate.FunctionDefintionKeyword;
            string pattern = $@".*{functionKeyword}\s+{signatureInfo.MethodName}{GetRegexOfFunctionKeyword(functionDefinitionSymbolOptions)}";
            return Regex.IsMatch(content, pattern, RegexOptions.Compiled);
        }


        throw new NotSupportedException($"The {programmingLanguage} is NOT supported at present");
    }
    
    private string GetRegexOfFunctionKeyword(
        FunctionDefinitionSymbolOptions functionDefinitionSymbolOptions
    )
    {
        string functionSymbol;
        switch (functionDefinitionSymbolOptions)
        {
            case FunctionDefinitionSymbolOptions.Whitespace:
                functionSymbol = @"\s+";
                break;
            case FunctionDefinitionSymbolOptions.WrappedByParentheses:
                functionSymbol = @"\s*\(";
                break;
            default:
                functionSymbol = @"\s+";
                break;
        }

        return functionSymbol;
    }
}
