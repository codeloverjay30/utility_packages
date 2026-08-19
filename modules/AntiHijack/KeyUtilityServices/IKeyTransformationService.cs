namespace KeyUtilityServices;

/// <summary>
/// Defines defensive cryptographic data transformation and encoding utilities.
/// </summary>
public interface IKeyTransformationService
{
    /// <summary>
    /// Safely converts a cryptographic byte span into a readable Base64 character span using stack-allocated memory.
    /// </summary>
    /// <param name="secretBytes">The source cryptographic raw bytes.</param>
    /// <param name="charBuffer">The destination span buffer to hold the encoded characters.</param>
    /// <returns>A read-only span containing the Base64 representation of the secret bytes.</returns>
    ReadOnlySpan<char> ConvertToSecureReadableSpan(ReadOnlySpan<byte> secretBytes, Span<char> charBuffer);
}
