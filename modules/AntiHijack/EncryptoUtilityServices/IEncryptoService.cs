namespace EncryptoUtilityServices;

public interface IEncryptoService
{
    /// <summary>
    /// Encrypts plain text characters to a Base64 string representation.
    /// </summary>
    string EncryptText(ReadOnlySpan<char> plainText, ReadOnlySpan<byte> keyBytes);

    /// <summary>
    /// Decrypts a Base64 encoded cipher text back into a plain text string safely.
    /// </summary>
    string DecryptText(ReadOnlySpan<char> base64CipherText, ReadOnlySpan<byte> keyBytes);
        
}
