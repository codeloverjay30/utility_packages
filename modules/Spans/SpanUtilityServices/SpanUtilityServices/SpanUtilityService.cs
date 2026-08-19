using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace SpanUtilityServices;

/// <summary>
/// High-performance architectural service for identifying .NET memory types and high-performance ecosystem components.
/// </summary>
public partial class SpanUtilityService : ISpanUtilityService
{
    internal static class Constants
    {
        public const string StringRentedBuffer = "StringRentedBuffer";
        public const string ValueStringBuilder = "ValueStringBuilder";
        public const string BufferSegment = "BufferSegment";
        public const string ReadOnlySequenceReader = "ReadOnlySequenceReader";

        public const string StringPool = "StringPool";

        public const string IArrayPool = "IArrayPool";

        public class CompiledMethodMetadata
        {
            public const string GenericMethodSuffix = "`1";
        }

        /// <summary>
        /// full qualified class (including namespace) of package in newer version 
        /// Its namespace is `CommunityToolkit.HighPerformance`
        /// </summary>
        public class CommunityToolkit
        {
            public const string Memory2D = "CommunityToolkit.HighPerformance.Memory2D";
            public const string ReadOnlyMemory2D = "CommunityToolkit.HighPerformance.ReadOnlyMemory2D";
        }
    }
    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public bool IsContinuousSpan(Type type)
    {
        GuardAgainstNull(type);
        if (!type.IsGenericType)
        {
            return false;
        }
        Type genericDef = type.GetGenericTypeDefinition();
        return genericDef == typeof(Span<>) ||
               genericDef == typeof(ReadOnlySpan<>);
    }

    /// <inheritdoc />
    public bool IsMemoryBlock(Type type)
    {
        GuardAgainstNull(type);
        if (!type.IsGenericType)
        {
            return false;
        }
        Type genericDef = type.GetGenericTypeDefinition();
        return genericDef == typeof(Memory<>) ||
               genericDef == typeof(ReadOnlyMemory<>);
    }

    /// <inheritdoc />
    public bool IsMultiDimensionalMemory(Type type)
    {
        GuardAgainstNull(type);
        if (!type.IsGenericType)
        {
            return false;
        }
        Type genericDef = type.GetGenericTypeDefinition();
        // Supported via CommunityToolkit.HighPerformance
        return genericDef.FullName != null && 
              (genericDef.FullName.StartsWith(
                Constants.CommunityToolkit.Memory2D 
                /* "CommunityToolkit.HighPerformance.Memory2D" */) || 
               genericDef.FullName.StartsWith(
                Constants.CommunityToolkit.ReadOnlyMemory2D 
                /* "CommunityToolkit.HighPerformance.ReadOnlyMemory2D" */ )
            );
    }

    /// <inheritdoc />
    public bool IsMemoryManagerOrSegment(Type type)
    {
        GuardAgainstNull(type);
        
        // 1. Check base class for MemoryManager<T>
        Type? currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(MemoryManager<>))
            {
                return true;
            }
            currentType = currentType.BaseType;
        }

        // 2. Check ReadOnlySequenceSegment<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReadOnlySequenceSegment<>))
        {
            return true;
        }

        // 3. Custom or internal BufferSegment text-match defense
        if (type.Name.Equals(Constants.BufferSegment /* "BufferSegment" */, StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool IsDiscontinuousSequence(Type type)
    {
        GuardAgainstNull(type);
        if (!type.IsGenericType)
        {
            return false;
        }
        return type.GetGenericTypeDefinition() == typeof(ReadOnlySequence<>);
    }

    /// <inheritdoc />
    public bool IsSequenceReader(Type type)
    {
        GuardAgainstNull(type);
        if (!type.IsGenericType)
        {
            return false;
        }
        Type genericDef = type.GetGenericTypeDefinition();
        return genericDef == typeof(SequenceReader<>) || 
               genericDef.Name.Equals(
                $"{Constants.ReadOnlySequenceReader}{Constants.CompiledMethodMetadata.GenericMethodSuffix}"
                /* "ReadOnlySequenceReader`1" */,
                StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public bool IsBufferControlInterface(Type type)
    {
        GuardAgainstNull(type);

        // Scan the interface hierarchy to prevent hidden structural failure
        if (type.IsInterface)
        {
            if (IsTargetInterface(type))
            {
                return true;
            }
        }

        foreach (Type iface in type.GetInterfaces())
        {
            if (IsTargetInterface(iface))
            {
                return true;
            }
        }

        return false;

        static bool IsTargetInterface(Type t) =>
            t.IsGenericType && (
                t.GetGenericTypeDefinition() == typeof(IBufferWriter<>) ||
                t.GetGenericTypeDefinition() == typeof(IMemoryOwner<>)
            );
    }

    /// <inheritdoc />
    public bool IsPoolInfrastructure(Type type)
    {
        GuardAgainstNull(type);

        // Check if inherits from ArrayPool<T>
        Type? currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(ArrayPool<>))
            {
                return true;
            }
            currentType = currentType.BaseType;
        }

        // Check CommunityToolkit StringPool or generic IArrayPool interface mapping
        if (type.Name.Equals(Constants.StringPool /* "StringPool" */, StringComparison.Ordinal) || 
            type.Name.Equals(Constants.IArrayPool /* "IArrayPool"*/, StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool IsRefStruct(Type type)
    {
        GuardAgainstNull(type);

        // .NET 8/9/10 皆原生支援此屬性，能完美防禦所有自訂的 ref struct
        return type.IsByRefLike;
    }

    /// <inheritdoc />
    public bool IsHighPerformanceStringDefense(Type type)
    {
        GuardAgainstNull(type);

        // Defensive name matching for unreleased open-source repository templates copy-pasted into solution
        string typeName = type.Name;
        return typeName.Equals(Constants.StringRentedBuffer /* "StringRentedBuffer" */, StringComparison.Ordinal) || 
               typeName.Equals(Constants.ValueStringBuilder /* "ValueStringBuilder" */, StringComparison.Ordinal);
    }
    
    /// <summary>
    /// Defensive argument validation guard clause.
    /// </summary>
    private static void GuardAgainstNull(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type), "The structural type snapshot evaluated cannot be null reference.");
        }
    }
}