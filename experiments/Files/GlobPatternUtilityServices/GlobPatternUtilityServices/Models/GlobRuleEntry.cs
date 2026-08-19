using GlobPatternUtilityServices.Abstractions;

namespace GlobPatternUtilityServices.Models;

/// <summary>
/// Represents a single rule entry mimicking .gitignore behavior.
/// </summary>
public record class GlobRuleEntry
{
    /// <summary>
    /// Gets or sets the glob pattern string.
    /// </summary>
    public required string Pattern { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this is an inverse rule (starts with '!').
    /// </summary>
    public bool IsInverse { get; set; } = false; // Default to false for safety, will be set correctly during pre-compilation

    /// <summary>
    /// Gets or sets the action strategy to execute when a file matches this rule.
    /// </summary>
    public required IMatchActionStrategy ActionStrategy { get; init; }
}
    