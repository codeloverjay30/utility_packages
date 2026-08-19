namespace AntiHijackUtilityServices.Tests;

/// <summary>
/// A manual test stub that implements <see cref="EncryptoUtilityServices.IEncryptoService"/> 
/// without relying on dynamic proxy reflection, avoiding boxing of ref structs like <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public sealed class FaultInjectableEncryptoService : EncryptoUtilityServices.IEncryptoService
{
    private Exception? _exceptionToThrow;
        
    private string _stubbedResult = string.Empty;
        

    /// <summary>
    /// Configures the stub to systematically throw a specific exception when invocation occurs.
    /// </summary>
    /// <param name="exception">The exception instance to be thrown.</param>
    public void SetupExceptionToThrow(Exception exception)
    {
        _exceptionToThrow = exception;
    }
        

    /// <summary>
    /// Configures the stub to return a predetermined string if no exception is configured.
    /// </summary>
    /// <param name="result">The string payload to return.</param>
    public void SetupReturnValue(string result)
    {
        _stubbedResult = result;
    }
        

    /// <summary>
    /// Simulates or interrupts the decryption sequence under rigorous memory performance constraints.
    /// </summary>
    /// <param name="cipherText">The immutable slice of text data.</param>
    /// <param name="key">The cryptographic security key buffer.</param>
    /// <returns>The decrypted representation of the data payload.</returns>
    /// <exception cref="Exception">Throws the configured error to simulate infrastructure instability.</exception>
    public string DecryptText(ReadOnlySpan<char> cipherText, ReadOnlySpan<byte> key)
    {
        if (_exceptionToThrow != null)
        {
            throw _exceptionToThrow;
        }

        return _stubbedResult;
    }
        

    /// <summary>
    /// Implements standard string encryption routine for contract compliance.
    /// </summary>
    /// <param name="plainText">The source string to protect.</param>
    /// <param name="key">The encryption key.</param>
    /// <returns>The encrypted output text.</returns>
    public string EncryptText(string plainText, byte[] key)
    {
        return string.Empty;
    }
        

    /// <summary>
    /// Implements standard string decryption routine for contract compliance.
    /// </summary>
    /// <param name="cipherText">The encrypted string.</param>
    /// <param name="key">The decryption key.</param>
    /// <returns>The plain text string.</returns>
    public string DecryptText(string cipherText, byte[] key)
    {
        return string.Empty;
    }


    public string EncryptText(ReadOnlySpan<char> plainText, ReadOnlySpan<byte> keyBytes)
    {
        var keyByteArray = keyBytes.IsEmpty ? Array.Empty<byte>() : keyBytes.ToArray();
        return EncryptText(plainText.ToString(), keyByteArray);
    }
}
