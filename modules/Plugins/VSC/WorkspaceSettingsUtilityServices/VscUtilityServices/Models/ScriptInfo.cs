namespace VscUtilityServices.Core.Models;

public record class ScriptInfo
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }

    public required VersionInfo VersionInfo { get; init; }
}
