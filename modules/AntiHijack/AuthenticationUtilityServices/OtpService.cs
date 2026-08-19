using OtpNet;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationUtilityServices;

/// <summary>
/// Service about OTP
/// </summary>
public class OtpService
{
    /// <summary>
    /// MFA verification
    /// </summary>
    /// <param name="issuer"></param>
    /// <param name="userEmail"></param>
    /// <returns></returns>
    public (string secret, string qrCodeBase64) EnableMfa(
            string issuer,
            string userEmail
    )
    {
        // 1. 生成隨機密鑰 (160 bits 是標準做法)
        byte[] key = KeyGeneration.GenerateRandomKey(20);
        string base32Secret = Base32Encoding.ToString(key);

        // 2. 建立 otpauth 連結
        // 格式：otpauth://totp/{Issuer}:{Account}?secret={Secret}&issuer={Issuer}
        string provisionUrl = $"otpauth://totp/{issuer}:{userEmail}?secret={base32Secret}&issuer={issuer}";

        // 3. 轉為 QR Code 圖片 (Base64 格式供前端顯示)
        using QRCodeGenerator qrGenerator = new QRCodeGenerator();
        using QRCodeData qrCodeData = qrGenerator.CreateQrCode(provisionUrl, QRCodeGenerator.ECCLevel.Q);
        using PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        string qrCodeBase64 = Convert.ToBase64String(qrCode.GetGraphic(20));

        return (base32Secret, qrCodeBase64);
    }
}
    

