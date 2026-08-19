using System.Text;
using EncryptoUtilityServices;


/// <summary>
/// A defensive manual stub implementation to bypass runtime reflection and dynamic proxy failures on ref structs.
/// </summary>
public sealed class FakeEncryptoService : IEncryptoService
{
    private string _stubbedResult = string.Empty;

    /// <summary>
    /// Configures the text to return upon subsequent decryption attempts.
    /// </summary>
    public void SetupDecryptionResult(string result)
    {
        _stubbedResult = result;
    }

    /// <summary>
    /// Simulates structural encryption.
    /// </summary>
    public string EncryptText(ReadOnlySpan<char> plainText, ReadOnlySpan<byte> keyBytes)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText.ToString()));
    }

    /// <summary>
    /// Safely processes decryption without triggering IL verification errors.
    /// </summary>
    public string DecryptText(ReadOnlySpan<char> base64CipherText, ReadOnlySpan<byte> keyBytes)
    {
        return _stubbedResult;
    }
}
    