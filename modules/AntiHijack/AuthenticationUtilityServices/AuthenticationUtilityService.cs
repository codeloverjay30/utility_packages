using System;
using System.Security.Cryptography;
using System.Text;

namespace AuthenticationUtilityServices;

/// <summary>
/// Standard implementation of structural authentication with constant-time defensive matching.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    /// <summary>
    /// Compares signatures using Cryptographic Constant-Time comparison to thwart side-channel timing attacks.
    /// </summary>
    public bool VerifySignature(
        ReadOnlySpan<char> rawPayload,
        ReadOnlySpan<char> expectedSignature
    )
    {
        if (rawPayload.IsEmpty || expectedSignature.IsEmpty)
        {
            return false;
        }

        // 防禦性設計：預估 UTF-8 緩衝區最大所需位元組數（1 char 最大可能佔 3~4 bytes）
        int maxPayloadBytes = Encoding.UTF8.GetMaxByteCount(rawPayload.Length);
        int maxSignatureBytes = Encoding.UTF8.GetMaxByteCount(expectedSignature.Length);

        // 使用 stackalloc 於執行緒棧（Thread Stack）上配置記憶體，完全避免託管堆（Heap）配置與 GC 壓力
        Span<byte> payloadBuffer = stackalloc byte[maxPayloadBytes];
        Span<byte> signatureBuffer = stackalloc byte[maxSignatureBytes];

        // 使用高興能 Span 多載，將字元安全地編碼至棧空間緩衝區，並取得真實寫入長度
        int actualPayloadBytes = Encoding.UTF8.GetBytes(rawPayload, payloadBuffer);
        int actualSignatureBytes = Encoding.UTF8.GetBytes(expectedSignature, signatureBuffer);

        // 依據實際寫入長度切割出精確的唯讀資料檢視區
        ReadOnlySpan<byte> finalPayload = payloadBuffer[..actualPayloadBytes];
        ReadOnlySpan<byte> finalSignature = signatureBuffer[..actualSignatureBytes];

        // 使用常數時間比對（Fixed-Time Comparison）防範計時攻擊（Timing Attack）
        return CryptographicOperations.FixedTimeEquals(finalPayload, finalSignature);
    }
}