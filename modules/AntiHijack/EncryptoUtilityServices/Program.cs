using EncryptoUtilityServices;
using System.Collections;
using System.Security.Cryptography;

AesGcmService aesGcmService = new AesGcmService();
// 1. 準備 Key (例如從密碼派生或隨機產生)
byte [ ] key = new byte [ 32 ];
RandomNumberGenerator.Fill(key);

// 2. 準備原始數據 (例如讀取檔案)
byte [ ] originalFileBytes = File.ReadAllBytes("Desumi-Magahara.jpg");

// 3. 加密
byte [ ] encryptedPackage = aesGcmService.Encrypt(originalFileBytes , key);

// 4. 解密
byte [ ] decryptedFileBytes = aesGcmService.Decrypt(encryptedPackage , key);

// 驗證內容是否一致
bool isEqual = StructuralComparisons.StructuralEqualityComparer.Equals(originalFileBytes , decryptedFileBytes);
Console.WriteLine($"還原是否成功: {isEqual}");
