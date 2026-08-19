namespace ExceptionsUtilityServices;

using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using SpanUtilityServices;


/// <summary>
/// An exception is thrown once the argument is null, empty, or only contains whitespace.
/// </summary>
public partial class ArgumentEmptyOrWhitespaceException : ArgumentException
{
    private static readonly ISpanUtilityService _defaultSpanUtilityService = new SpanUtilityService();

    #region ----------- Constructors -----------

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentEmptyOrWhitespaceException"/> class with a default message.
    /// </summary>
    public ArgumentEmptyOrWhitespaceException() : base("Value cannot be empty or whitespace.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentEmptyOrWhitespaceException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ArgumentEmptyOrWhitespaceException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentEmptyOrWhitespaceException"/> class with a specified error message and the name of the parameter that causes this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    public ArgumentEmptyOrWhitespaceException(string? message, string? paramName) : base(message, paramName) { }

    #endregion

#if NET9_0_OR_GREATER

#if PASS_BY_VALUE

    /// Defines the method using passing by value.
    /// Although Span<T> or ReadOnlySpan<T> etc are used as parameters
    /// for zero-allocation, less GC (for most cases), 
    /// (and thus better (runtime) performance),
    /// it will still copy the structure (in rare case) in one of following occurs: 
    /// + One of parameter is a `ref struct` the holds lots of memory space.
    /// + One of parameter is some class that defined in namespace <see cref="global.System.Buffers"/> 
    /// (e.g. <see cref="global.System.Buffers.ReadOnlySequence{T}"/>)
    /// 
    /// Thus, for extremely best performance, 
    /// invoke the method defined in the `#if PASS_BY_REFERENCE` block.
    
    #region ----------- .NET 9.0+ / C# 13 High-Performance Pipeline -----------

    #region ----------- Overloads of ThrowIfNullOrEmpty -----------

    /// <summary>
    /// Throws an <see cref="ArgumentEmptyOrWhitespaceException"/> if the specialized span structure is empty.
    /// </summary>
    /// <typeparam name="TValue">The underlying element type within the span.</typeparam>
    /// <param name="argument">The read-only span instance to check.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty<TValue>(
        ReadOnlySpan<TValue> argument, 
        string? paramName = null
    )
    {
        if (argument.IsEmpty)
        {
            throw new ArgumentEmptyOrWhitespaceException(
                $"The crystalline memory span '{paramName ?? "value"}' cannot be empty.", paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentEmptyOrWhitespaceException"/> if the generalized ref struct memory carrier is empty.
    /// </summary>
    /// <typeparam name="TSpan">The memory structure type which allows ref structs.</typeparam>
    /// <param name="argument">The memory instance to check for emptiness.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty<TSpan>(
        TSpan argument,
        string? paramName = null
    )
        where TSpan : allows ref struct
    {
        if (argument is null)
        {
            throw new ArgumentEmptyOrWhitespaceException($"Argument '{paramName ?? "value"}' can neither be null nor empty.", paramName);
        }
        
        bool isEmpty = _defaultSpanUtilityService.IsEmpty(ref argument);
        if (!isEmpty)
        {
            return;
        }
        throw new ArgumentEmptyOrWhitespaceException($"Argument '{paramName ?? "value"}' can neither be null nor empty.", paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentEmptyOrWhitespaceException"/> if the argument memory structure is empty.
    /// </summary>
    /// <typeparam name="TSpan">The memory structure type which allows ref structs.</typeparam>
    /// <typeparam name="TValue">The underlying element type within the memory structure.</typeparam>
    /// <param name="argument">The memory instance to check for emptiness.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentEmptyOrWhitespaceException">Thrown when the argument memory length is zero.</exception>
    /// <exception cref="NotSupportedException">Thrown when an unsupported type is provided into the guard clause.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty<TSpan, TValue>(
        TSpan argument, 
        string? paramName = null
    )
        where TSpan : allows ref struct
    {
        if (typeof(TSpan) == typeof(ReadOnlySpan<TValue>))
        {
            ref ReadOnlySpan<TValue> span = ref Unsafe.As<TSpan, ReadOnlySpan<TValue>>(ref argument);
            if (span.IsEmpty)
            {
                throw new ArgumentEmptyOrWhitespaceException("The high-performance continuous memory span cannot be empty.", paramName);
            }
            return;
        }

        if (typeof(TSpan) == typeof(Span<TValue>))
        {
            ref Span<TValue> span = ref Unsafe.As<TSpan, Span<TValue>>(ref argument);
            if (span.IsEmpty)
            {
                throw new ArgumentEmptyOrWhitespaceException("The volatile memory span cannot be empty.", paramName);
            }
            return;
        }

        if (typeof(TSpan) == typeof(ReadOnlyMemory<TValue>))
        {
            ref ReadOnlyMemory<TValue> memory = ref Unsafe.As<TSpan, ReadOnlyMemory<TValue>>(ref argument);
            if (memory.IsEmpty)
            {
                throw new ArgumentEmptyOrWhitespaceException("The heap-allocated asynchronous memory block cannot be empty.", paramName);
            }
            return;
        }

        if (typeof(TSpan) == typeof(Memory<TValue>))
        {
            ref Memory<TValue> memory = ref Unsafe.As<TSpan, Memory<TValue>>(ref argument);
            if (memory.IsEmpty)
            {
                throw new ArgumentEmptyOrWhitespaceException("The mutable asynchronous memory block cannot be empty.", paramName);
            }
            return;
        }

        if (typeof(TSpan) == typeof(ReadOnlySequence<TValue>))
        {
            ref ReadOnlySequence<TValue> sequence = ref Unsafe.As<TSpan, ReadOnlySequence<TValue>>(ref argument);
            if (sequence.IsEmpty)
            {
                throw new ArgumentEmptyOrWhitespaceException("The segmented multi-segment memory sequence cannot be empty.", paramName);
            }
            return;
        }

        if (typeof(TSpan) == typeof(ArraySegment<TValue>))
        {
            ref ArraySegment<TValue> arraySegment = ref Unsafe.As<TSpan, ArraySegment<TValue>>(ref argument);
            if (arraySegment.Count == 0)
            {
                throw new ArgumentEmptyOrWhitespaceException("The localized array segment slice cannot be empty.", paramName);
            }
            return;
        }

        if (typeof(TSpan) == typeof(TValue[]))
        {
            TValue[] array = Unsafe.As<TSpan, TValue[]>(ref argument);
            if (array.Length == 0)
            {
                throw new ArgumentEmptyOrWhitespaceException("The managed standard array cannot be empty.", paramName);
            }
            return;
        }

        if (typeof(TValue) == typeof(char) && typeof(TSpan) == typeof(string))
        {
            string str = Unsafe.As<TSpan, string>(ref argument);
            if (str.Length == 0)
            {
                throw new ArgumentEmptyOrWhitespaceException("The native string input cannot be empty.", paramName);
            }
            return;
        }

        throw new NotSupportedException($"The type '{typeof(TSpan).FullName}' is not a recognized high-performance memory structure inside the defensive utility pipeline.");
    }
    #endregion

    #region ----------- Overloads of ThrowIfNullOrWhitespace -----------
    /// <summary>
    /// Throws an exception if the character span is empty or contains only whitespace.
    /// </summary>
    /// <param name="argument">The character span to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrWhitespace(
        ReadOnlySpan<char> argument, 
        string? paramName = null
    )
    {
        if (argument.IsEmpty)
        {
            throw new ArgumentEmptyOrWhitespaceException("Character span cannot be empty.", paramName);
        }

        if (argument.IsWhiteSpace())
        {
            throw new ArgumentEmptyOrWhitespaceException("Character span cannot consist only of white-space characters.", paramName);
        }
    }

    /// <summary>
    /// Throws an exception if the character memory is empty or contains only whitespace.
    /// </summary>
    /// <param name="argument">The character memory to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrWhitespace(
        ReadOnlyMemory<char> argument, 
        string? paramName = null
    )
    {
        ThrowIfNullOrWhitespace(argument.Span, paramName);
    }

    /// <summary>
    /// Throws an exception if the character memory segment is empty or contains only whitespace.
    /// </summary>
    /// <param name="argument">The mutable character memory to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrWhitespace(
        Memory<char> argument, 
        string? paramName = null
    )
    {
        ThrowIfNullOrWhitespace(argument.Span, paramName);
    }

    /// <summary>
    /// Throws an exception if the unmanaged memory stream is null or empty.
    /// </summary>
    /// <param name="argument">The unmanaged memory stream instance to check.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    public static void ThrowIfNullOrEmpty(
        UnmanagedMemoryStream? argument, 
        string? paramName = null
    )
    {
        if (argument == null) 
        {
            throw new ArgumentNullException(paramName);
        }
        if (argument.Length == 0)
        {
            throw new ArgumentEmptyOrWhitespaceException("The unmanaged memory stream contains no data.", paramName);
        }
    }

    /// <summary>
    /// Throws an exception if the segmented character sequence is empty or contains only whitespace.
    /// </summary>
    /// <param name="argument">The segmented character sequence to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    public static void ThrowIfNullOrWhitespace(
        in ReadOnlySequence<char> argument, 
        string? paramName = null
    )
    {
        if (argument.IsEmpty)
        {
            throw new ArgumentEmptyOrWhitespaceException("Sequence cannot be empty.", paramName);
        }

        if (argument.IsSingleSegment)
        {
            ThrowIfNullOrWhitespace(argument.First.Span, paramName);
            return;
        }

        foreach (var segment in argument)
        {
            if (!segment.Span.IsWhiteSpace())
            {
                return;
            }
        }

        throw new ArgumentEmptyOrWhitespaceException("Sequence cannot consist only of white-space characters.", paramName);
    }

    #endregion
    
    #endregion
    
#elif PASS_BY_REFERENCE

    /// Passing by reference will be used in these method call,
    /// which has better performance for all kinds of argument at present.
    /// There is an alternative, the method defined in the `#if PASS_BY_VALUE` block.

    #region ----------- .NET 9.0+ / C# 13 High-Performance Pipeline -----------

    #region ----------- Overloads of ThrowIfNullOrEmpty -----------
    /// <summary>
    /// Throws an <see cref="ArgumentEmptyOrWhitespaceException"/> if the generalized ref struct memory carrier is empty.
    /// </summary>
    /// <typeparam name="TSpan">The memory structure type which allows ref structs.</typeparam>
    /// <param name="argument">The memory instance passed by absolute reference to eliminate copy overhead.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty<TSpan>(
        in TSpan argument,
        string? paramName = null
    )
        where TSpan : allows ref struct
    {
        // 修正：將隱式局部變數改為傳引用方式，消除 struct 複製
        TSpan localArg = argument;
        if (localArg is null)
        {
            throw new ArgumentEmptyOrWhitespaceException($"Argument '{paramName ?? "value"}' can neither be null nor empty.", paramName);
        }

        bool isEmpty = _defaultSpanUtilityService.IsEmpty(ref localArg);
        if (isEmpty)
        {
            throw new ArgumentEmptyOrWhitespaceException($"Argument '{paramName ?? "value"}' can neither be null nor empty.", paramName);
        }
    }

    /// <summary>
    /// Highly optimized guard clause leveraging static generic type mapping for zero-cost runtime abstraction.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty<TSpan, TValue>(
        in TSpan argument, string? paramName = null
    )
        where TSpan : allows ref struct
    {
        // 透過 Unsafe 配合唯讀跳轉，達成硬體級別的高效能分支
        ref TSpan nonConstArg = ref Unsafe.AsRef(in argument);

        if (TypeCache<TSpan, TValue>.IsReadOnlySpan)
        {
            ref ReadOnlySpan<TValue> span = ref Unsafe.As<TSpan, ReadOnlySpan<TValue>>(ref nonConstArg);
            if (span.IsEmpty)
            {
                throw new ArgumentEmptyOrWhitespaceException("The high-performance continuous memory span cannot be empty.", paramName);
            }
            return;
        }

        if (TypeCache<TSpan, TValue>.IsSpan)
        {
            ref Span<TValue> span = ref Unsafe.As<TSpan, Span<TValue>>(ref nonConstArg);
            if (span.IsEmpty)
            {
                throw new ArgumentEmptyOrWhitespaceException("The volatile memory span cannot be empty.", paramName);
            }
            return;
        }

        if (TypeCache<TSpan, TValue>.IsReadOnlyMemory)
        {
            ref ReadOnlyMemory<TValue> memory = ref Unsafe.As<TSpan, ReadOnlyMemory<TValue>>(ref nonConstArg);
            if (memory.IsEmpty)
            {
                throw new ArgumentEmptyOrWhitespaceException("The heap-allocated asynchronous memory block cannot be empty.", paramName);
            }
            return;
        }

        if (TypeCache<TSpan, TValue>.IsMemory)
        {
            ref Memory<TValue> memory = ref Unsafe.As<TSpan, Memory<TValue>>(ref nonConstArg);
            if (memory.IsEmpty)
            {
                throw new ArgumentEmptyOrWhitespaceException("The mutable asynchronous memory block cannot be empty.", paramName);
            }
            return;
        }

        if (TypeCache<TSpan, TValue>.IsReadOnlySequence)
        {
            ref ReadOnlySequence<TValue> sequence = ref Unsafe.As<TSpan, ReadOnlySequence<TValue>>(ref nonConstArg);
            if (sequence.IsEmpty) throw new ArgumentEmptyOrWhitespaceException("The segmented multi-segment memory sequence cannot be empty.", paramName);
            return;
        }

        if (TypeCache<TSpan, TValue>.IsArraySegment)
        {
            ref ArraySegment<TValue> arraySegment = ref Unsafe.As<TSpan, ArraySegment<TValue>>(ref nonConstArg);
            if (arraySegment.Count == 0)
            {
                throw new ArgumentEmptyOrWhitespaceException("The localized array segment slice cannot be empty.", paramName);
            }
            return;
        }

        if (TypeCache<TSpan, TValue>.IsArray)
        {
            TValue[] array = Unsafe.As<TSpan, TValue[]>(ref nonConstArg);
            if (array is null || array.Length == 0)
            {
                throw new ArgumentEmptyOrWhitespaceException("The managed standard array cannot be empty.", paramName);
            }
            return;
        }

        if (TypeCache<TSpan, TValue>.IsString)
        {
            string str = Unsafe.As<TSpan, string>(ref nonConstArg);
            if (str is null || str.Length == 0)
            {
                throw new ArgumentEmptyOrWhitespaceException("The native string input cannot be empty.", paramName);
            }
            return;
        }

        throw new NotSupportedException($"The type '{typeof(TSpan).FullName}' is not recognized inside the cached defensive utility pipeline.");
    }

    #endregion

    #region ----------- Overloads of ThrowIfNullOrWhitespace -----------

    /// <summary>
    /// Throws an <see cref="ArgumentEmptyOrWhitespaceException"/> if the read-only character span is empty or consists only of white-space characters.
    /// </summary>
    /// <param name="argument">The continuous character span to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrWhitespace(
        ReadOnlySpan<char> argument,
        string? paramName = null
    )
    {
        if (argument.IsEmpty)
        {
            throw new ArgumentEmptyOrWhitespaceException($"Argument '{paramName ?? "value"}' cannot be empty.", paramName);
        }

        if (argument.IsWhiteSpace())
        {
            throw new ArgumentEmptyOrWhitespaceException($"Argument '{paramName ?? "value"}' cannot consist only of white-space characters.", paramName);
        }
    }

    public static void ThrowIfNullOrWhitespace(
        ReadOnlyMemory<char> argument,
        string? paramName = null
    )
    {
        // 直接對接 Span 特化管線，防止任何非必要堆疊複製
        ThrowIfNullOrWhitespace(argument.Span, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentEmptyOrWhitespaceException"/> if the character memory segment is empty or consists only of white-space characters.
    /// </summary>
    /// <param name="argument">The character memory carrier to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrWhitespace(
        Memory<char> argument,
        string? paramName = null
    )
    {
        // 直接對接 Span 特化管線，防止任何非必要堆疊複製
        ThrowIfNullOrWhitespace(argument.Span, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentEmptyOrWhitespaceException"/> if the segmented character sequence is empty or contains only whitespace.
    /// </summary>
    /// <param name="argument">The segmented multi-segment sequence passed by absolute reference to eliminate stack-copying.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrWhitespace(
        in ReadOnlySequence<char> argument,
        string? paramName = null
    )
    {
        if (argument.IsEmpty)
        {
            throw new ArgumentEmptyOrWhitespaceException("Sequence infrastructure segment cannot be empty.", paramName);
        }

        // 單一節點優化路徑（Fast-Path）
        if (argument.IsSingleSegment)
        {
            ReadOnlySpan<char> span = argument.First.Span;
            if (span.IsWhiteSpace())
            {
                throw new ArgumentEmptyOrWhitespaceException("Sequence cannot consist only of white-space characters.", paramName);
            }
            return;
        }

        // 多節點防禦掃描（Slow-Path）：利用內建 Enumerator 進行無配置（Allocation-free）跳轉
        bool allWhitespace = true;
        foreach (var segment in argument)
        {
            if (!segment.Span.IsWhiteSpace())
            {
                allWhitespace = false;
                break;
            }
        }

        if (allWhitespace)
        {
            throw new ArgumentEmptyOrWhitespaceException("Sequence cannot consist only of white-space characters.", paramName);
        }
    }
    
    #endregion

    #endregion

#endif

#else

    #region ----------- Legacy .NET Standard / Framework Backward Compatible Pipeline -----------

    /// <summary>
    /// Throws an exception if the <see cref="ReadOnlySpan{T}"/> is empty.
    /// </summary>
    /// <typeparam name="T">The underlying element type within the span.</typeparam>
    /// <param name="argument">The read-only span instance to check.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty<T>(ReadOnlySpan<T> argument, string? paramName = null)
    {
        if (argument.IsEmpty)
        {
            throw new ArgumentEmptyOrWhitespaceException("The crystalline memory span cannot be empty.", paramName);
        }
    }

    /// <summary>
    /// Throws an exception if the <see cref="ReadOnlyMemory{T}"/> is empty.
    /// </summary>
    /// <typeparam name="T">The underlying element type within the memory block.</typeparam>
    /// <param name="argument">The read-only memory instance to check.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty<T>(ReadOnlyMemory<T> argument, string? paramName = null)
    {
        if (argument.IsEmpty)
        {
            throw new ArgumentEmptyOrWhitespaceException("The memory block cannot be empty.", paramName);
        }
    }

    /// <summary>
    /// Throws an exception if the <see cref="ReadOnlySequence{T}"/> is empty.
    /// </summary>
    /// <typeparam name="T">The underlying element type within the segmented sequence.</typeparam>
    /// <param name="argument">The read-only sequence instance to check.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    public static void ThrowIfNullOrEmpty<T>(in ReadOnlySequence<T> argument, string? paramName = null)
    {
        if (argument.IsEmpty)
        {
            throw new ArgumentEmptyOrWhitespaceException("The segmented memory sequence cannot be empty.", paramName);
        }
    }

    /// <summary>
    /// Throws an exception if the character span is empty or contains only whitespace.
    /// </summary>
    /// <param name="argument">The character span to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrWhitespace(ReadOnlySpan<char> argument, string? paramName = null)
    {
        if (argument.IsEmpty)
        {
            throw new ArgumentEmptyOrWhitespaceException("Character span cannot be empty.", paramName);
        }

        if (argument.IsWhiteSpace())
        {
            throw new ArgumentEmptyOrWhitespaceException("Character span cannot consist only of white-space characters.", paramName);
        }
    }

    /// <summary>
    /// Throws an exception if the character memory is empty or contains only whitespace.
    /// </summary>
    /// <param name="argument">The character memory to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrWhitespace(ReadOnlyMemory<char> argument, string? paramName = null)
    {
        ThrowIfNullOrWhitespace(argument.Span, paramName);
    }

    /// <summary>
    /// Throws an exception if the segmented character sequence is empty or contains only whitespace.
    /// </summary>
    /// <param name="argument">The segmented character sequence to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    public static void ThrowIfNullOrWhitespace(in ReadOnlySequence<char> argument, string? paramName = null)
    {
        if (argument.IsEmpty)
        {
            throw new ArgumentEmptyOrWhitespaceException("Sequence cannot be empty.", paramName);
        }

        if (argument.IsSingleSegment)
        {
            ThrowIfNullOrWhitespace(argument.First.Span, paramName);
            return;
        }

        foreach (var segment in argument)
        {
            if (!segment.Span.IsWhiteSpace())
            {
                return;
            }
        }

        throw new ArgumentEmptyOrWhitespaceException("Sequence cannot consist only of white-space characters.", paramName);
    }

    /// <summary>
    /// Throws an exception if the <see cref="Memory{T}"/> is empty.
    /// </summary>
    /// <typeparam name="T">The underlying element type within the memory block.</typeparam>
    /// <param name="argument">The mutable memory instance to check.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty<T>(Memory<T> argument, string? paramName = null)
    {
        ThrowIfNullOrEmpty((ReadOnlyMemory<T>)argument, paramName);
    }

    /// <summary>
    /// Throws an exception if the <see cref="ArraySegment{T}"/> is empty.
    /// </summary>
    /// <typeparam name="T">The underlying element type within the array segment.</typeparam>
    /// <param name="argument">The array segment instance to check.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty<T>(ArraySegment<T> argument, string? paramName = null)
    {
        ThrowIfNullOrEmpty((ReadOnlySpan<T>)argument, paramName);
    }

    /// <summary>
    /// Throws an exception if the character memory segment is empty or contains only whitespace.
    /// </summary>
    /// <param name="argument">The mutable character memory to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrWhitespace(Memory<char> argument, string? paramName = null)
    {
        ThrowIfNullOrWhitespace(argument.Span, paramName);
    }

    /// <summary>
    /// Throws an exception if the unmanaged memory stream is null or empty.
    /// </summary>
    /// <param name="argument">The unmanaged memory stream instance to check.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    public static void ThrowIfNullOrEmpty(UnmanagedMemoryStream? argument, string? paramName = null)
    {
        if (argument == null) throw new ArgumentNullException(paramName);
        if (argument.Length == 0)
        {
            throw new ArgumentEmptyOrWhitespaceException("The unmanaged memory stream contains no data.", paramName);
        }
    }

    #endregion

#endif
}