using System.Text.Json.Serialization;

namespace VscUtilityServices.Core.Models;

/// <summary>
/// Represents the root structural model of the settings.json5 configuration file.
/// </summary>
public class HookConfiguration
{
    [JsonPropertyName("hooks")]
    public List<HookTargetWrapper> Hooks { get; set; } = new();
}
    
