namespace VscUtilityServices.Core.Models;

public record class ProgrammingLanguageInfo
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
}
