namespace FileExplorerUtilityServices;

/// <summary>
/// Represents a memory-efficient collection of drive paths allocated on the stack.
/// </summary>
public readonly ref struct DrivesInfo
{
    /// <summary>
    /// Gets the collection of parsed drive segments as a span of memory ranges.
    /// </summary>
    public ReadOnlySpan<Range> DriveRanges { get; }

    /// <summary>
    /// Gets the original source text containing the drive information.
    /// </summary>
    public ReadOnlySpan<char> SourceText { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DrivesInfo"/> struct defensively.
    /// </summary>
    public DrivesInfo(ReadOnlySpan<char> sourceText, ReadOnlySpan<Range> driveRanges)
    {
        SourceText = sourceText;
        DriveRanges = driveRanges;
    }
}
    
