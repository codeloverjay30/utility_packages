using System.Buffers;

namespace SharedUtilityServices;

/// <summary>
/// Implement the <see cref="global::SharedUtilityServices.IByteArrayPool"/> interface 
/// and have same behavior of <see cref="global::System.Buffers.ArrayPool{byte}.Shared"/>
/// </summary>
public class SharedByteArrayPool : IByteArrayPool
{
    public byte[] Rent(int minimumLength) => ArrayPool<byte>.Shared.Rent(minimumLength);
    public void Return(byte[] array, bool clearArray = false) => ArrayPool<byte>.Shared.Return(array, clearArray);
}
