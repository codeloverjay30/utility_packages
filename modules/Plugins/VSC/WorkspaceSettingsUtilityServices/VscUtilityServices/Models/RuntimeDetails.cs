using System.Text.Json.Serialization;

namespace VscUtilityServices.Core.Models;

/// <summary>
/// Holds exact SDK type and version parameters.
/// </summary>
public class RuntimeDetails
{
    [JsonPropertyName("sdk")]
    public string Sdk { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}
    