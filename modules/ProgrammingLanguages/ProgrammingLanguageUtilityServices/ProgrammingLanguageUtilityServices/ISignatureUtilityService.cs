namespace ProgrammingLanguageUtilityServices;

public interface ISignatureUtilityService
{
    bool IsSignatureMatched(
        string content,
        string programmingLanguage,
        SignatureInfo signatureInfo
    );
}
