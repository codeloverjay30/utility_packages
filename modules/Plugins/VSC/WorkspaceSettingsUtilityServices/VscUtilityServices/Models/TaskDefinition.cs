using System.Text.Json.Serialization;

namespace VscUtilityServices.Core.Models;

/// <summary>
/// Encapsulates metadata for an individual execution task inside a target hook.
/// </summary>
public class TaskDefinition
{
    [JsonPropertyName("task-name")]
    public string TaskName { get; set; } = string.Empty;

    [JsonPropertyName("runtime-settings")]
    public RuntimeSettings RuntimeSettings { get; set; } = new();

// Architectural Defense: JSON5 maps both forms in wild production profiles.
    [JsonPropertyName("programming-languages")]
    public string ProgrammingLanguageFallback { get; set; } = string.Empty;

    [JsonPropertyName("programming-language")]
    public string ProgrammingLanguageDirect { get; set; } = string.Empty;

    /// <summary>
    /// Gets the verified programming language token normalized across inconsistent config formats.
    /// </summary>
    [JsonIgnore]
    public string ProgrammingLanguage => !string.IsNullOrWhiteSpace(ProgrammingLanguageDirect) 
        ? ProgrammingLanguageDirect 
        : ProgrammingLanguageFallback;
}
    