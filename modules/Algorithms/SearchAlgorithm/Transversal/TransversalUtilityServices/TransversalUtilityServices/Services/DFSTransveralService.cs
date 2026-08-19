using System.Reflection;

namespace TransversalUtilityServices
{
    public class DFSTransversalService : ITransversalService
    {
        /// <summary>
        /// 純粹的 DFS 物件樹遍歷引擎
        /// </summary>
        /// <param name="root">起始物件</param>
        /// <param name="onVisited">當巡檢到一個物件時要執行的動作 (回傳物件本身)</param>
        public void Transverse(object root, Action<object> onVisited)
        {
            ScanRecursive(root, onVisited,new HashSet<object>());
        }

        public void ScanRecursive(object obj, Action<object> onVisited, HashSet<object> visited)
        {
            if (obj == null) return;

            // 1. 避免循環引用
            if (!visited.Add(obj)) return;

            // 2. 觸發回呼 (將目前巡檢到的物件丟給外部邏輯處理)
            onVisited(obj);

            // 3. 獲取所有具備資料的屬性
            var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

            foreach (var prop in props)
            {
                // 跳過基本型別，避免無謂的遞迴
                if (prop.PropertyType == typeof(string) || prop.PropertyType.IsPrimitive) continue;

                var value = prop.GetValue(obj);
                if (value == null) continue;

                // 處理集合 (DFS 向下探索)
                if (value is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        ScanRecursive(item, onVisited, visited);
                    }
                }
                // 處理單一物件 (DFS 向下探索)
                else if (prop.PropertyType.IsClass)
                {
                    ScanRecursive(value, onVisited, visited);
                }
            }
        }
    }
}