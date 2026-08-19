using System.Text.Json.Serialization;

namespace VscUtilityServices.Core.Models;

/// <summary>
/// Wrapper to match the nested target structure defined in the requirement specifications.
/// </summary>
public class HookTargetWrapper
{
    [JsonPropertyName("target")]
    public TargetDefinition Target { get; set; } = new();
}
    