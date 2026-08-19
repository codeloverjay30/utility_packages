using System.Text.Json.Serialization;

namespace VscUtilityServices.Core.Models;

/// <summary>
/// Dictates the infrastructure and SDK requirements for the targeted task environment.
/// </summary>
public class RuntimeSettings
{
    [JsonPropertyName("requires")]
    public RuntimeRequirement Requires { get; set; } = new();
}
    