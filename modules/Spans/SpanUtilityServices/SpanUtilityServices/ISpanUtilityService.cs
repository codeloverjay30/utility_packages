namespace SpanUtilityServices;

/// <summary>
/// Defines advanced utility operations for validating and inspecting <see cref="global::System.Span{T}"/>, 
/// memory managers, pipelines, segmentations, and high-performance toolkit types.
/// </summary>
public partial interface ISpanUtilityService
{
    /// <summary>
    /// Checks if the type is a 1D continuous memory span (<see cref="global::System.Span{T}"/> or <see cref="global::System.ReadOnlySpan{T}"/>).
    /// </summary>
    bool IsContinuousSpan(Type type);

    /// <summary>
    /// Checks if the type is a 1D heap-allocatable memory block (<see cref="global::System.Memory{T}"/> or <see cref="global::System.ReadOnlyMemory{T}"/>).
    /// </summary>
    bool IsMemoryBlock(Type type);

    /// <summary>
    /// Checks if the type is a multi-dimensional continuous memory from CommunityToolkit (<c>Memory2D&lt;T&gt;</c> or <c>ReadOnlyMemory2D&lt;T&gt;</c>).
    /// </summary>
    bool IsMultiDimensionalMemory(Type type);

    /// <summary>
    /// Checks if the type is a memory manager or sequence segment (<see cref="global::System.Buffers.MemoryManager{T}"/>, <c>BufferSegment</c>, or <see cref="global::System.Buffers.ReadOnlySequenceSegment{T}"/>).
    /// </summary>
    bool IsMemoryManagerOrSegment(Type type);

    /// <summary>
    /// Checks if the type represents a discontinuous memory structure (<see cref="global::System.Buffers.ReadOnlySequence{T}"/>).
    /// </summary>
    bool IsDiscontinuousSequence(Type type);

    /// <summary>
    /// Checks if the type is a sequence reader for discontinuous memory (<see cref="global::System.Buffers.SequenceReader{T}"/> or <c>ReadOnlySequenceReader</c>).
    /// </summary>
    bool IsSequenceReader(Type type);

    /// <summary>
    /// Checks if the type implements buffer writing or memory ownership interfaces (<see cref="global::System.Buffers.IBufferWriter{T}"/> or <see cref="global::System.Buffers.IMemoryOwner{T}"/>).
    /// </summary>
    bool IsBufferControlInterface(Type type);

    /// <summary>
    /// Checks if the type belongs to standard or high-performance pooling infrastructure (<see cref="global::System.Buffers.ArrayPool{T}"/> or <c>StringPool</c>).
    /// </summary>
    bool IsPoolInfrastructure(Type type);

    /// <summary>
    /// Checks if the type matches open-source high-performance string defense buffers (<c>StringRentedBuffer</c> or <c>ValueStringBuilder</c>).
    /// </summary>
    bool IsHighPerformanceStringDefense(Type type);

    /// <summary>
    /// Checks if the specified type is a ref struct (stack-only structure) using runtime metadata.
    /// </summary>
    bool IsRefStruct(Type type);
}