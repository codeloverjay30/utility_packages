using System;
using System.Buffers;

namespace SpanUtilityServices.Tests;

/// <summary>
/// Fake memory manager implementation for testing type identification.
/// </summary>
internal sealed class FakeMemoryManager : MemoryManager<byte>
{
    public override Span<byte> GetSpan() => Span<byte>.Empty;
    public override MemoryHandle Pin(int elementIndex = 0) => default;
    public override void Unpin() { }
    protected override void Dispose(bool disposing) { }
}

/// <summary>
/// Custom implementation of IBufferWriter for type defense verification.
/// </summary>
internal sealed class CustomBufferWriter : IBufferWriter<byte>
{
    public void Advance(int count) { }
    public Memory<byte> GetMemory(int sizeHint = 0) => Memory<byte>.Empty;
    public Span<byte> GetSpan(int sizeHint = 0) => Span<byte>.Empty;
}

/// <summary>
/// A dummy class used to simulate non-generic, standard system components.
/// </summary>
internal sealed class StandardClass { }

/// <summary>
/// A copy-pasted string rented buffer structure often found in high-performance legacy codebases.
/// </summary>
internal struct StringRentedBuffer { }

/// <summary>
/// A copy-pasted value string builder structure often found in high-performance legacy codebases.
/// </summary>
internal struct ValueStringBuilder { }

/// <summary>
/// A dummy custom implementation mirroring the name of BufferSegment.
/// </summary>
internal struct BufferSegment { }

#if NET9_0_OR_GREATER
/// <summary>
/// A custom ref struct used to test memory layout length extraction under .NET 9+.
/// </summary>
internal ref struct CustomRefStruct
{
    private readonly IntPtr _pointer;
    private readonly int _length;

    public CustomRefStruct(int length)
    {
        _pointer = IntPtr.Zero;
        _length = length;
    }
}
#endif