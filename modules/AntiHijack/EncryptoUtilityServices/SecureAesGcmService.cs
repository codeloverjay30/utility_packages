using System.Security.Cryptography;

namespace EncryptoUtilityServices;

public class SecureAesGcmService : ISecureAesGcmService
{
    /// <summary>
    /// 加密敏感數據並同時驗證附加身分資訊（如目前執行緒ID或系統環境雜湊），防止重放與篡改劫持
    /// </summary>
    public (byte[] CipherText, byte[] Tag, byte[] Nonce) Encrypt(byte[] plainText, byte[] key, byte[] associatedData)
    {
        ArgumentNullException.ThrowIfNull(plainText, nameof(plainText));
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        byte[] nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 Bytes
        byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize];     // 16 Bytes
        byte[] cipherText = new byte[plainText.Length];

        RandomNumberGenerator.Fill(nonce);

        using var aesGcm = new AesGcm(key, tag.Length);
        aesGcm.Encrypt(nonce, plainText, cipherText, tag, associatedData);

        return (cipherText, tag, nonce);
    }
}