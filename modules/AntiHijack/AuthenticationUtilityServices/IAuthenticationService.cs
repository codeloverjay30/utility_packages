namespace AuthenticationUtilityServices;

/// <summary>
/// Provides verification mechanics to protect structural integrity against spoofing or payload modification.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Validates that the payload was signed correctly and matches the given verification signature token.
    /// </summary>
    bool VerifySignature(ReadOnlySpan<char> rawPayload, ReadOnlySpan<char> expectedSignature);
}
    
