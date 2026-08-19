namespace VscUtilityServices.Core.Models;

public record class Script
{
    public required string Path { get; init; }
    public required ScriptInfo ScriptInfo { get; init; }
    public required ProgrammingLanguageInfo LanguageInfo { get; init; }
}
