using System.Text.Json.Serialization;

namespace VscUtilityServices.Core.Models;

/// <summary>
/// Defines a lifecycle hook target triggered by specific VS Code workspace events.
/// </summary>
public class TargetDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("on")]
    public List<string> OnEvents { get; set; } = new();

    [JsonPropertyName("tasks")]
    public List<TaskDefinition> Tasks { get; set; } = new();
}
    