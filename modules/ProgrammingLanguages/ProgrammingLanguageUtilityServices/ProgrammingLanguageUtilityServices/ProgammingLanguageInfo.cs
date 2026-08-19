namespace ProgrammingLanguageUtilityServices;

public class ProgammingLanguageInfo
{
    public required SignatureTemplateInfo SignatureTemplate { get; init; }
    public required string DisplayedName { get; init; }
    public required string LowercasedName { get; init; }
    public required string FileExtension { get; init; }
}
