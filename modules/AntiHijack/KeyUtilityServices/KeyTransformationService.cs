using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace KeyUtilityServices;

/// <summary>
/// Implements zero-allocation cryptographic data transformation with active validation.
/// </summary>
public sealed class KeyTransformationService : IKeyTransformationService
{
    /// <summary>
    /// Converts a read-only span of cryptographic secret bytes into a secure, readable character span using Base64 encoding.
    /// </summary>
    /// <param name="secretBytes">The read-only span containing the source cryptographic secret bytes.</param>
    /// <param name="charBuffer">The destination span where the resulting Base64 characters will be written.</param>
    /// <returns>A read-only span of characters representing the secure, readable transformation.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided <paramref name="charBuffer"/> is too small to hold the transformed string.</exception>
    /// <exception cref="CryptographicException">Thrown when the Base64 UTF-8 encoding operation fails due to cryptographic buffer anomalies.</exception>
    public ReadOnlySpan<char> ConvertToSecureReadableSpan(ReadOnlySpan<byte> secretBytes, Span<char> charBuffer)
    {
        if (secretBytes.IsEmpty)
        {
            return ReadOnlySpan<char>.Empty;
        }

        // Calculate required base64 length: Out = 4 * n / 3 padded to multiple of 4
        int requiredBase64Length = ((secretBytes.Length + 2) / 3) * 4;

        // if (charBuffer.Length < requiredBase64Length)
        // {
        //     throw new ArgumentException(
        //         $"The provided destination buffer is too small. Required: {requiredBase64Length}, Actual: {charBuffer.Length}.",
        //         nameof(charBuffer));
        // }

        ArgumentOutOfRangeException.ThrowIfLessThan(charBuffer.Length, requiredBase64Length);

        // Allocate byte buffer on stack for Base64 bytes transformation
        Span<byte> base64BytesBuffer = stackalloc byte[requiredBase64Length];

        if (Base64.EncodeToUtf8(secretBytes, base64BytesBuffer, out _, out int bytesWritten) != System.Buffers.OperationStatus.Done)
        {
            throw new CryptographicException("Failed to encode cryptographic bytes to Base64 UTF-8 sequence.");
        }

        // Convert the UTF-8 Base64 bytes into characters
        int charsWritten = Encoding.UTF8.GetChars(base64BytesBuffer[..bytesWritten], charBuffer);
        return charBuffer[..charsWritten];
    }
}
