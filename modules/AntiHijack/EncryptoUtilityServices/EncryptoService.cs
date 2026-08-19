using System;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using SharedUtilityServices;

namespace EncryptoUtilityServices;

/// <summary>
/// Cryptographic service fully optimized for Zero-Allocation scenarios and high-throughput memory buffers.
/// </summary>
public class EncryptoService : IEncryptoService
{
    private const int BytesPerBits = 8;
    private const int MaxStackAllocBytes = 1024;
    private const int AesBlockSizeInBytes = 16;
    private const int KeyBufferInBits = 256;
    private const int KeyBufferInBytes = (int)((double)KeyBufferInBits / BytesPerBits);

    private readonly IByteArrayPool _byteArrayPool;
    public EncryptoService(
        IByteArrayPool byteArrayPool
    )
    {
        ArgumentNullException.ThrowIfNull(byteArrayPool);

        this._byteArrayPool = byteArrayPool;
    }

    /// <summary>
    /// Encrypts plain text characters using AES-256-CBC without managing high-level stream abstractions or heap-allocated structures.
    /// </summary>
    /// <param name="plainText">The source text buffer treated as an immutable character span.</param>
    /// <param name="keyBytes">The 256-bit cryptographic key span.</param>
    /// <returns>A string representation containing the Base64-encoded ciphertext.</returns>
    /// <exception cref="ArgumentException">Thrown if the cryptographic key length does not conform to 32 bytes.</exception>
    public string EncryptText(
        ReadOnlySpan<char> plainText,
        ReadOnlySpan<byte> keyBytes
    )
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(keyBytes.Length, KeyBufferInBytes, nameof(keyBytes));

        if (plainText.IsEmpty)
        {
            return string.Empty;
        }

        // 1. 動態計算密文最大可能位元組，防禦編碼緩衝區溢位
        int maxByteCount = Encoding.UTF8.GetByteCount(plainText);

        // 2. 配置明文 Byte 緩衝區（小於閾值則走 Stack 空間，完全避免 GC 壓力）
        byte[]? rentedPlainArray = null;
        Span<byte> plainBytes = maxByteCount <= MaxStackAllocBytes
            ? stackalloc byte[MaxStackAllocBytes]
            : (rentedPlainArray = _byteArrayPool.Rent(maxByteCount));

        try
        {
            int actualPlainBytesWritten = Encoding.UTF8.GetBytes(plainText, plainBytes);
            Span<byte> validPlainBytes = plainBytes[..actualPlainBytesWritten];

            // 3. 資安安全：使用現成 Span 產生動態隨機初始化向量 (IV) 防止密碼劫持與重放攻擊
            Span<byte> iv = stackalloc byte[AesBlockSizeInBytes];
            RandomNumberGenerator.Fill(iv); // 鐵律2：呼叫安全且高效的 ReadOnlySpan 填充機制

            using var aes = Aes.Create();
            aes.Key = keyBytes.ToArray(); // 由於 AES 加密引擎底層要求，金鑰轉換為內部陣列

            // 4. 計算 CBC Padding 後的總加密空間
            int cipherLength = aes.GetCiphertextLengthCbc(validPlainBytes.Length);

            // 5. 加上裝載 IV 的總輸出長度 (IV 置於密文最前段，便於未來解密解包)
            int totalOutputLength = AesBlockSizeInBytes + cipherLength;

            byte[]? rentedCipherArray = null;
            Span<byte> totalOutputBuffer = totalOutputLength <= MaxStackAllocBytes
                ? stackalloc byte[MaxStackAllocBytes]
                : (rentedCipherArray = _byteArrayPool.Rent(totalOutputLength));

            try
            {
                // 將隨機生成的 IV 複製到緩衝區頭部
                iv.CopyTo(totalOutputBuffer[..AesBlockSizeInBytes]);

                // .NET 10 高效 Span 加密：直接寫入 IV 後面的剩餘空間
                int bytesEncrypted = aes.EncryptCbc(
                    validPlainBytes,
                    iv,
                    totalOutputBuffer[AesBlockSizeInBytes..],
                    PaddingMode.PKCS7);

                Span<byte> finalizedCipherSpan = totalOutputBuffer[..(AesBlockSizeInBytes + bytesEncrypted)];

                // 轉為 Base64 字串輸出
                return Convert.ToBase64String(finalizedCipherSpan);
            }
            finally
            {
                if (rentedCipherArray != null)
                {
                    _byteArrayPool.Return(rentedCipherArray);
                }
            }
        }
        finally
        {
            if (rentedPlainArray != null)
            {
                _byteArrayPool.Return(rentedPlainArray);
            }
        }
    }

    /// <summary>
    /// Decrypts high-performance Span-based Base64 payloads securely without heap contamination.
    /// </summary>
    public string DecryptText(
        ReadOnlySpan<char> base64CipherText,
        ReadOnlySpan<byte> keyBytes
    )
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(keyBytes.Length, KeyBufferInBytes, nameof(keyBytes));

        if (base64CipherText.IsEmpty)
        {
            return string.Empty;
        }
        // 1. 計算解碼 Base64 所需的 Byte 長度
        int maxCipherByteCount = Base64.GetMaxDecodedFromUtf8Length(base64CipherText.Length); // 或者是簡化估算
        int approximateBytes = (base64CipherText.Length * 3 / 4) + 2;

        byte[]? rentedCipherArray = null;
        Span<byte> cipherBuffer = approximateBytes <= MaxStackAllocBytes 
                ? stackalloc byte[MaxStackAllocBytes] 
                : (rentedCipherArray = _byteArrayPool.Rent(approximateBytes));

        try
        {
            // 將字串解碼回原始 Byte 陣列 (包含頭部的 IV)
            if (!Convert.TryFromBase64String(new string(base64CipherText), cipherBuffer, out int bytesDecoded))
            {
                throw new CryptographicException("The provided cipher text is not a valid Base64 string.");
            }

            Span<byte> actualCipherPayload = cipherBuffer[..bytesDecoded];

            if (actualCipherPayload.Length <= AesBlockSizeInBytes)
            {
                throw new CryptographicException("Cipher payload is truncated or severely corrupted.");
            }

            // 2. 抽取頭部 16 碼作為 IV，其餘為密文核心
            ReadOnlySpan<byte> iv = actualCipherPayload[..AesBlockSizeInBytes];
            ReadOnlySpan<byte> pureCipherBytes = actualCipherPayload[AesBlockSizeInBytes..];

            using var aes = Aes.Create();
            aes.Key = keyBytes.ToArray();

            // 3. 配置明文輸出 Buffer
            byte[]? rentedPlainArray = null;
            Span<byte> plainBuffer = pureCipherBytes.Length <= MaxStackAllocBytes 
                ? stackalloc byte[MaxStackAllocBytes]
                : (rentedPlainArray = _byteArrayPool.Rent(pureCipherBytes.Length));

            try
            {
                // .NET 10 高效能無配置解密
                int bytesDecrypted = aes.DecryptCbc(pureCipherBytes, iv, plainBuffer, PaddingMode.PKCS7);

                return Encoding.UTF8.GetString(plainBuffer[..bytesDecrypted]);
            }
            finally
            {
                if (rentedPlainArray != null)
                {
                    _byteArrayPool.Return(rentedPlainArray);
                }
            }
        }
        finally
        {
            if (rentedCipherArray != null)
            {
                _byteArrayPool.Return(rentedCipherArray);
            }
        }
    }
}