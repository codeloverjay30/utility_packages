using System;
using System.Collections.Generic;
using System.Text;

namespace SortingUtilityServices
{
    public interface ISortingUtilityService
    {
        // 改成回傳 Tuple，讓呼叫端決定如何處理，避免 ref 導致的執行緒安全難題
        (T min , T max) GetSortedPair<T>(T a , T b , IComparer<T> comparer = null) where T : IComparable<T>;
   
    }
}
