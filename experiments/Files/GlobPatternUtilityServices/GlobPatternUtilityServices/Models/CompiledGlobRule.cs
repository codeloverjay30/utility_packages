namespace GlobPatternUtilityServices.Models;

/// <summary>
/// Encapsulates the compiled matcher and its behavior properties.
/// This POCO ensures type safety and clean architecture without heap allocation.
/// </summary>
public readonly struct CompiledGlobRule
{
    /// <summary>
    /// Gets the pre-compiled Microsoft FileSystemGlobbing Matcher instance.
    /// </summary>
    public Microsoft.Extensions.FileSystemGlobbing.Matcher Matcher { get; }

    /// <summary>
    /// Gets a value indicating whether this rule is an inverse rule.
    /// </summary>
    public bool IsInverse { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompiledGlobRule"/> struct.
    /// </summary>
    /// <param name="matcher">The compiled matcher.</param>
    /// <param name="isInverse">If set to <c>true</c>, it is an inverse rule.</param>
    public CompiledGlobRule(Microsoft.Extensions.FileSystemGlobbing.Matcher matcher, bool isInverse)
    {
        // 預防性檢查 (符合鐵律)
        ArgumentNullException.ThrowIfNull(matcher);
        
        Matcher = matcher;
        IsInverse = isInverse;
    }
}