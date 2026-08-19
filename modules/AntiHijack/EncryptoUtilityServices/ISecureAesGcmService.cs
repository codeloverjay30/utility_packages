namespace EncryptoUtilityServices;

public interface ISecureAesGcmService
{
    (byte[] CipherText, byte[] Tag, byte[] Nonce) Encrypt(byte[] plainText, byte[] key, byte[] associatedData);
}
