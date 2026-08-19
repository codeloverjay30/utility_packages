using System.Buffers;

namespace ExceptionsUtilityServices;

public partial class ArgumentEmptyOrWhitespaceException
{
    internal static class TypeCache<TSpan, TValue> 
        where TSpan : allows ref struct
    {
        public static readonly bool IsReadOnlySpan =
            typeof(TSpan) == typeof(ReadOnlySpan<TValue>);
        public static readonly bool IsSpan =
            typeof(TSpan) == typeof(Span<TValue>);
        public static readonly bool IsReadOnlyMemory =
            typeof(TSpan) == typeof(ReadOnlyMemory<TValue>);
        public static readonly bool IsMemory =
            typeof(TSpan) == typeof(Memory<TValue>);
        public static readonly bool IsReadOnlySequence =
            typeof(TSpan) == typeof(ReadOnlySequence<TValue>);
        public static readonly bool IsArraySegment =
            typeof(TSpan) == typeof(ArraySegment<TValue>);
        public static readonly bool IsArray =
            typeof(TSpan) == typeof(TValue[]);
        public static readonly bool IsString =
            typeof(TValue) == typeof(char) && typeof(TSpan) == typeof(string);
    }
}
    