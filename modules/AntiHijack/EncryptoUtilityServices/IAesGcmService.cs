using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncryptoUtilityServices;

public interface IAesGcmService
{
    bool IsEncryptionParametersValid(byte[] key);
    bool IsDecryptionParametersValid(byte[] encryptedData);
    byte[] Decrypt(byte[] encryptedData, byte[] key);
    byte[] Encrypt(byte[] dataToEncrypt, byte[] key);
    (byte[], byte[], byte[]) EncryptUnboxed(byte[] dataToEncrypt, byte[] key);

}

