namespace SortingUtilityServices
{
    public class SortingUtilityService : ISortingUtilityService
    {
        /// <summary>
        /// Exchange a and b when a is considered to be greater than b.
        /// Ensuring a is always less than b.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <param name="comparer">Comparer</param>
        /// <remarks>
        /// When the comparer <paramref name="comparer"/> is NOT specified, use the default comparer.
        /// </remarks>
        // 改成回傳 Tuple，讓呼叫端決定如何處理，避免 ref 導致的執行緒安全難題
        public (T min , T max) GetSortedPair<T>(T a , T b , IComparer<T> comparer = null) where T : IComparable<T>
        {
            comparer ??= Comparer<T>.Default;
            return comparer.Compare(a , b) > 0 ? (b , a) : (a , b);
        }
    }
}
