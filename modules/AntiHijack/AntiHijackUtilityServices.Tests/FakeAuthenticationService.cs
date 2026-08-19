using AuthenticationUtilityServices;

namespace AntiHijackUtilityServices.Tests;

/// <summary>
/// Fake class of <see cref="global::AuthenticationUtilityServices.AuthenticationService"/>
/// </summary>
public class FakeAuthenticationService : IAuthenticationService
{
    private ReadOnlyMemory<char> _stubbedResult;

    /// <summary>
    /// Configures the text to return upon subsequent decryption attempts.
    /// </summary>
    public void SetupDecryptionResult(string result)
    {
        _stubbedResult = result.AsMemory();
    }

    /// <summary>
    /// Configures the text to return upon subsequent decryption attempts.
    /// </summary>
    public void SetupDecryptionResult(ReadOnlyMemory<char> result)
    {
        _stubbedResult = result;
    }

    public bool VerifySignature(ReadOnlySpan<char> rawPayload, ReadOnlySpan<char> expectedSignature)
    {
        return _stubbedResult.Span.SequenceEqual(expectedSignature);
    }
}
