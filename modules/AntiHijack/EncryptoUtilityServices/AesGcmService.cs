using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EncryptoUtilityServices;

/// <summary>
/// 用在AES-GCM加密和解密的服務
/// </summary>
/// <remarks>
/// AesGcm 類別是在 .NET Core3.0 才加入的，且依賴底層作業系統支援。
/// 於是此類別只能在Windows 10或Windows Server 2016以上的系統上運行，且需要安裝相應的更新。
/// </remarks>
[Obsolete]
public class AesGcmService : IAesGcmService
{
    // AES-GCM 標準規格
    private const int NonceSize = 12; // 96 bits
    private const int TagSize = 16;   // 128 bits

    #region 加密和解密實作

    /// <summary>
    /// 加密位元組陣列
    /// </summary>
    /// <returns>回傳格式: [Nonce(12 bytes)][Tag(16 bytes)][Ciphertext]</returns>
    public byte[] Encrypt(byte[] dataToEncrypt, byte[] key)
    {
        var (c, n, t) = _Encrypt(dataToEncrypt, key);
        return _BoxKeyWithSpan(c, n, t);
    }

    /// <summary>
    /// 加密位元組陣列
    /// </summary>
    /// <returns>回傳格式: [Nonce(12 bytes)][Tag(16 bytes)][Ciphertext]</returns>
    public (byte[], byte[], byte[]) EncryptUnboxed(byte[] dataToEncrypt, byte[] key)
    {
        return _Encrypt(dataToEncrypt, key);
    }

    /// <summary>
    /// 解密封裝後的位元組陣列
    /// </summary>
    /// <param name="encryptedData">格式需為: [Nonce][Tag][Ciphertext]</param>
    public byte[] Decrypt(byte[] encryptedData, byte[] key)
    {
        var decryptedData = _Decrypt(encryptedData, key);
        return decryptedData;
    }

    /// <summary>
    /// 加密位元組陣列
    /// </summary>
    /// <returns>回傳格式: [Nonce(12 bytes)][Tag(16 bytes)][Ciphertext]</returns>

    private (byte[], byte[], byte[]) _Encrypt(byte[] dataToEncrypt, byte[] key)
    {
        _ValidateEncryptionParameters(key);

        byte[] nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        using var aesGcm = new AesGcm(key, TagSize);
        byte[] ciphertext = new byte[dataToEncrypt.Length];
        byte[] tag = new byte[TagSize];

        aesGcm.Encrypt(nonce, dataToEncrypt, ciphertext, tag);

        return (ciphertext, nonce, tag);
    }

    /// <summary>
    /// 解密封裝後的位元組陣列
    /// </summary>
    /// <param name="encryptedData">格式需為: [Nonce][Tag][Ciphertext]</param>
    private byte[] _Decrypt(byte[] encryptedData, byte[] key)
    {
        _ValidateDecryptionParameters(encryptedData);

        // 拆解資料
        byte[] nonce = new byte[NonceSize];
        byte[] tag = new byte[TagSize];
        int ciphertextLength = encryptedData.Length - NonceSize - TagSize;
        byte[] ciphertext = new byte[ciphertextLength];

        Buffer.BlockCopy(encryptedData, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(encryptedData, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(encryptedData, NonceSize + TagSize, ciphertext, 0, ciphertextLength);

        using var aesGcm = new AesGcm(key, TagSize);
        byte[] decryptedData = new byte[ciphertextLength];

        // 解密並驗證 Tag
        aesGcm.Decrypt(nonce, ciphertext, tag, decryptedData);

        return decryptedData;
    }
    #endregion

    #region 參數驗證

    /// <summary>
    /// 判斷要被加密的金鑰長度是否有效
    /// 此類別支援 128、192 和 256 位元的金鑰長度
    /// </summary>
    /// <param name="key">要被加密的金鑰</param>
    /// <returns></returns>
    /// <remarks>
    /// 只支援C# 11.0以上的編譯器，因為此方法實作用到List Pattern Matching語法
    /// </remarks>
    public bool IsEncryptionParametersValid(byte[] key)
    {
        return key is { Length: 16 or 24 or 32 };
    }
    /// <summary>
    /// 預先檢查要被加密的金鑰是否有效
    /// </summary>
    /// <param name="key">要被加密的金鑰</param>
    /// <exception cref="ArgumentException">當要被加密的金鑰長度不符時</exception>
    private void _ValidateEncryptionParameters(byte[] key)
    {
        if (!IsEncryptionParametersValid(key))
        {
            throw new ArgumentException("Key length must be 128, 192, or 256 bits.");
        }
    }
    /// <summary>
    /// 判斷要被解密的資料長度是否有效
    /// </summary>
    /// <param name="encryptedData">要被解密的資料</param>
    /// <returns></returns>
    public bool IsDecryptionParametersValid(byte[] encryptedData)
    {
        return encryptedData.Length > NonceSize + TagSize;
    }
    /// <summary>
    /// 預先檢查要被解密的資料是否有效
    /// </summary>
    /// <param name="encryptedData"></param>
    /// <exception cref="ArgumentException">當要被解密的資料長度不符時</exception>
    private void _ValidateDecryptionParameters(byte[] encryptedData)
    {
        if (!IsDecryptionParametersValid(encryptedData))
        {
            throw new ArgumentException("Invalid encrypted data length.");
        }
    }

    #endregion

    #region 包裝和拆解
    /// <summary>
    /// 將 Nonce、Tag 和 Ciphertext 盒裝成一個 Byte返回。
    /// </summary>
    /// <param name="ciphertext"></param>
    /// <param name="nonce"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <remarks>
    /// 雖然這個效能較佳(因為它使用了 Span<T> 來避免不必要的陣列複製)
    /// 但它要求 .NET 5.0或 .NET Core 3.1以上的環境
    /// </remarks>
    private byte[] _BoxKeyWithSpan(byte[] ciphertext, byte[] nonce, byte[] tag)
    {
        byte[] result = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Span<byte> resultSpan = result;

        nonce.CopyTo(resultSpan);
        tag.CopyTo(resultSpan.Slice(nonce.Length));
        ciphertext.CopyTo(resultSpan.Slice(nonce.Length + tag.Length));

        return result;
    }

    /// <summary>
    /// 將 Nonce、Tag 和 Ciphertext 盒裝成一個 Byte返回。
    /// </summary>
    /// <param name="ciphertext"></param>
    /// <param name="nonce"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <remarks>
    /// 優先使用<see cref='_BoxKeyWithSpan'>除非需要兼容舊版本的 .NET。因為效能較佳且沒有額外的陣列複製(確保了傳入引數不會短暫殘留在記憶體)。
    /// </remarks>
    private byte[] _BoxKeyToBytes(byte[] ciphertext, byte[] nonce, byte[] tag)
    {
        int totalLength = nonce.Length + tag.Length + ciphertext.Length;
        byte[] result = new byte[totalLength];

        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length);

        return result;
    }

    #endregion
}
    
