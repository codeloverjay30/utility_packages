using System.Text.Json.Serialization;

namespace VscUtilityServices.Core.Models;

/// <summary>
/// Specific runtime environment metrics required for execution validation.
/// </summary>
public class RuntimeRequirement
{
    [JsonPropertyName("runtime")]
    public RuntimeDetails Runtime { get; set; } = new();
}
    