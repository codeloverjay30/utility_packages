namespace SpanUtilityServices;

public partial interface ISpanUtilityService
{
#if NET9_0_OR_GREATER
    /// <summary>
    /// Utilizes direct raw pointer memory mapping 
    /// evaluate unknown ref struct boundaries safely.
    /// </summary>
    /// <param name="instance">The instance</param>
    /// <returns>
    /// A number that indicates unknown ref struct instance <paramref name="instance"/> is empty
    /// 
    /// 
    /// </returns>
    int GetStatusOfUnknownRefStruct<T>(ref T instance) where T : allows ref struct;

    /// <summary>
    /// Check the <paramref name="instance"/> is empty.
    /// </summary>
    /// <param name="instance">The instance</param>
    /// <returns>Return <see langword="true"/> iff it is considered to be empty. Otherwise, return <see langword="false"/></returns>
    bool IsEmpty<T>(ref T instance) where T : allows ref struct;
#else
    /// <summary>
    /// Utilizes direct raw pointer memory mapping 
    /// evaluate unknown ref struct boundaries safely.
    /// </summary>
    /// <param name="instance">The instance</param>
    /// <returns>The length of unknown ref struct instance <paramref name="instance"/></returns>
    int GetUnknownRefStructLength<T>(ref T instance);
    
    /// <summary>
    /// Check the <paramref name="instance"/> is empty.
    /// </summary>
    /// <param name="instance">The instance</param>
    /// <returns>Return <see langword="true"/> iff it is considered to be empty. Otherwise, return <see langword="false"/></returns>
    bool IsEmpty<T>(ref T instance);
#endif
}
