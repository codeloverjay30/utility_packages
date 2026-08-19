namespace SharedUtilityServices;

/// <summary>
/// Extract the logic of <see cref="global::System.Buffers.ArrayPool{byte}"/>
/// </summary>
public interface IByteArrayPool
{
    byte[] Rent(int minimumLength);
    void Return(byte[] array, bool clearArray = false);
}