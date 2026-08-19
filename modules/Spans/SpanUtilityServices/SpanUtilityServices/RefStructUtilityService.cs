using System.Buffers;
using System.Runtime.CompilerServices;

namespace SpanUtilityServices;

public class Constants
{
    public const int FAILURE_TEST = -1;
}

#if NET9_0_OR_GREATER

#region ----------- High Performance without Reflection -----------

/// <inheritdoc />
public partial class SpanUtilityService : ISpanUtilityService
{
    /// <inheritdoc/>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetStatusOfUnknownRefStruct<T>(ref T instance) where T : allows ref struct
    {
        Type currentType = typeof(T);

        // 核心安全攔截：利用你開發的 IsRefStruct 進行守門
        if (!IsRefStruct(currentType))
        {
            // 防禦性分流：動態精準攔截任何泛型 T 的 ReadOnlySequence<T>
            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(ReadOnlySequence<>))
            {
                // 利用反射或動態轉換安全讀取 IsEmpty 屬性，徹底絕育記憶體錯位 Bug
                return DynamicReadOnlySequenceInspector.IsEmpty(ref instance, currentType) ?
                    (int)StatusInfo.IsEmpty : 
                    (int)StatusInfo.IsNotEmpty;
            }

            // 對於其他非 ref struct 且未預期的資料結構，安全返回失敗常數
            return (int)StatusInfo.FailureTest;
        }

        // 走到這裡，代表絕對是真實的 ref struct (如 Span / ReadOnlySpan)，指標黑魔法可以 100% 安全執行
        try
        {
            ref byte rawRef = ref Unsafe.As<T, byte>(ref instance);
            int length = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref rawRef, Unsafe.SizeOf<IntPtr>()));
            return Math.Max(length,0);
        }
        catch
        {
            return (int)StatusInfo.FailureTest;
        }
    }

    /// <inheritdoc/>
    public bool IsEmpty<T>(ref T instance) where T : allows ref struct
    {
        Type currentType = typeof(T);

        // 核心安全防線：優先判定是否為「非 ref struct」的標準序列
        if (!IsRefStruct(currentType))
        {
            // 針對常見的 ReadOnlySequence<byte> 與 <char> 進行常規極速硬轉 (此時安全，因為已排除 ref struct)
            if (currentType == typeof(ReadOnlySequence<byte>))
            {
                return Unsafe.As<T, ReadOnlySequence<byte>>(ref instance).IsEmpty;
            }
            if (currentType == typeof(ReadOnlySequence<char>))
            {
                return Unsafe.As<T, ReadOnlySequence<char>>(ref instance).IsEmpty;
            }

            // 防禦性分流：攔截任意泛型 T 的 ReadOnlySequence<> (如 ReadOnlySequence<int> 等)
            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(ReadOnlySequence<>))
            {
                // 安全且不配置記憶體地透過 Inspector 動態檢查 IsEmpty
                return DynamicReadOnlySequenceInspector.IsEmpty(ref instance, currentType);
            }
        }

        // 走到這裡，代表它是真實的 ref struct (Span/ReadOnlySpan) 或是其他常規資料結構
        // 必須以 ref 方式將實例傳遞給長度檢查器，若長度為 0 則代表為空
        return GetStatusOfUnknownRefStruct(ref instance) == (int)StatusInfo.IsEmpty;
    }

}

#endregion

#else

#region ----------- High Performance without Reflection -----------

/// <inheritdoc />
public partial class SpanUtilityService : ISpanUtilityService
{
    /// <summary>
    /// Utilizes direct raw pointer memory mapping instead of reflection cache to evaluate unknown ref struct boundaries safely.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetUnknownRefStructLength<T>(ref T instance)
    {
        // 防禦性程式設計：若在C# 13- (corresponding .NET 8.0-)，則因為`T`不能為ref struct
        // 再加上在其他的packages的API (如:`ArgumentEmptyOrWhitespaceException`的`ThrowIfNullOrEmpty`方法)，
        // 讓`IsEmpty<T>(T instance)`方法，在針對泛型(非`ref struct`)T時，永遠回傳false
        // 以不干擾主業務邏輯
        return (int)StatusInfo.FailureTest; /// StatusInfo.FailureTest => -2
    }

    public bool IsEmpty<T>(ref T instance)
    {
        // 防禦性程式設計：若在C# 13- (corresponding .NET 8.0-)，則因為`T`不能為ref struct
        // 再加上在其他的packages的API (如:`ArgumentEmptyOrWhitespaceException`的`ThrowIfNullOrEmpty`方法)，
        // 讓`IsEmpty<T>(T instance)`方法，在針對泛型(非`ref struct`)T時，永遠回傳false
        // 以不干擾主業務邏輯
        return GetUnknownRefStructLength(ref instance) == (int)StatusInfo.IsEmpty;
    }
}

#endregion

#endif
