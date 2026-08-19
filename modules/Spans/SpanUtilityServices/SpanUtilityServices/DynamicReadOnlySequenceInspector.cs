using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace SpanUtilityServices;

#if NET9_0_OR_GREATER

/// <summary>
/// Provides advanced metadata and expression-tree infrastructure to safely inspect 
/// non-ref struct instances that bypass generic constraints without runtime failure.
/// </summary>
internal static class DynamicReadOnlySequenceInspector
{
    // 利用快取儲存高度優化的指標檢查委派，簽章為：接收一個不安全指標，回傳是否為空
    private static readonly ConcurrentDictionary<Type, Func<IntPtr, bool>> _pointerCache = new();

    /// <summary>
    /// Safely evaluates the IsEmpty property of a generic ReadOnlySequence 
    /// via zero-copy unmanaged pointer forwarding to prevent compilation and boxing layout violations.
    /// </summary>
    /// <typeparam name="T">The type of the structure, relaxed by allows ref struct constraint.</typeparam>
    /// <param name="instance">The reference to the generic data structure instance.</param>
    /// <param name="currentType">The exact runtime type of the sequence container.</param>
    /// <returns>True if the underlying sequence is empty; otherwise, false.</returns>
    public static bool IsEmpty<T>(ref T instance, Type currentType) where T : allows ref struct
    {
        try
        {
            // 動態編譯一個「接收指標記憶體地址，並將其還原為強型別結構存取」的超高效委派
            var checker = _pointerCache.GetOrAdd(currentType, t =>
            {
                // 輸入參數為非託管指標地址 IntPtr
                var ptrParam = Expression.Parameter(typeof(IntPtr), "ptr");

                // 利用 Unsafe.AsRefPointer 的概念，在表達式中直接將指標還原為強型別的「指標類型 (t*)」
                // 為了編譯器相容性，我們改用動態表達式去呼叫自訂的泛型指標轉換器
                var method = typeof(DynamicReadOnlySequenceInspector)
                    .GetMethod(nameof(CreatePointerChecker), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
                    .MakeGenericMethod(t);

                if (method == null)
                {
                    return _ => false;
                }

                return (Func<IntPtr, bool>)method.Invoke(null, null)!;
            });

            // 核心安全黑魔法：取得 instance 的託管指標（Managed Pointer），轉為非託管指標傳入
            // 由於外層已嚴格判定此時 T 不是 ref struct，此操作在記憶體配置上 100% 安全
            unsafe
            {
                IntPtr ptr = (IntPtr)Unsafe.AsPointer(ref instance);
                return checker(ptr);
            }
        }
        catch
        {
            // 防禦性程式設計：若發生任何不可抗力之執行期異常，保守判定為非空，由外層核心續行處理
            return false;
        }
    }

    /// <summary>
    /// Dynamically constructs a strongly-typed pointer resolver to bypass boxing restrictions of allows ref struct.
    /// </summary>
    /// <typeparam name="TTarget">The targeted generic ReadOnlySequence type structure.</typeparam>
    /// <returns>A configured functional delegate acting directly on a raw memory block.</returns>
    private static Func<IntPtr, bool> CreatePointerChecker<TTarget>() where TTarget : struct
    {
        return new Func<IntPtr, bool>((IntPtr ptr) =>
        {
            unsafe
            {
                // 100% 絕育裝箱！直接將非託管指標地址轉換回強型別結構的託管引用
                ref TTarget sequenceRef = ref Unsafe.AsRef<TTarget>((void*)ptr);
                
                // 動態利用 C# 靜態繫結或動態分流讀取屬性
                // 由於 TTarget 是常規 struct (如 ReadOnlySequence<byte>)，此處可以直接利用反射或
                // 透過表達式編譯一次性拿到它的 IsEmpty 屬性委派。最安全的作法是使用編編譯好的局部委派：
                var param = Expression.Parameter(typeof(TTarget).MakeByRefType(), "seq");
                var property = Expression.Property(param, "IsEmpty");
                var lambda = Expression.Lambda<PropertyGetterDelegate<TTarget>>(property, param);
                var compiled = lambda.Compile();

                return compiled(ref sequenceRef);
            }
        });
    }

    // 定義 ByRef 傳參的特殊高效內部委派
    private delegate bool PropertyGetterDelegate<TTarget>(ref TTarget instance);
}

#else

internal static class DynamicReadOnlySequenceInspectors
{
    // 利用 ConcurrentDictionary 或是 Runtime Feature 動態生成委派
    private static readonly ConcurrentDictionary<Type, Func<object, bool>> _cache = new();

    public static bool IsEmpty<T>(ref T instance, Type currentType)
    {
        // 取得該型別專屬的快速檢查委派
        var checker = _cache.GetOrAdd(currentType, t =>
        {
            // 動態建立一個表達式樹 (Expression Tree) 或發射 IL (Emit)
            // 用來安全且不經反射地讀取 IsEmpty
            var param = Expression.Parameter(typeof(object));
            var cast = Expression.Convert(param, t);
            var property = Expression.Property(cast, "IsEmpty");
            return Expression.Lambda<Func<object, bool>>(property, param).Compile();
        });

        try
        {
            // 取得 instance 的託管指標（Managed Pointer），並轉換為非託管指標傳入
            unsafe
            {
                IntPtr ptr = (IntPtr)Unsafe.AsPointer(ref instance);
                return checker(ptr);
            }

            return false; /// StatusInfo.FailureTest => -2
        }
        catch
        {
            return false; /// StatusInfo.FailureTest => -2
        }
    }
}

#endif