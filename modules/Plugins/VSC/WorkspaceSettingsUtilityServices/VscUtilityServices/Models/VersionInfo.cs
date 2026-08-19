namespace VscUtilityServices.Core.Models;

public record class VersionInfo
{
    public required string Version { get; init; } = string.Empty;
}
