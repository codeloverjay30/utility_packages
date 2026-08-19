using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCViewers;
    public class GCStatusService
    {
        public string GetInfoMessage()
        {
            StringBuilder stringBuilder = new StringBuilder();
            // 取得目前分配的總記憶體 (以 byte 為單位)
            long totalMemory = GC.GetTotalMemory(false);
            stringBuilder.AppendLine($"current memory usage: {totalMemory / 1024.0 / 1024.0:F2} MB");

            // 查看各個世代發生的回收次數
            stringBuilder.AppendLine($"0th Generation GC count: {GC.CollectionCount(0)}");
            stringBuilder.AppendLine($"1th Generation GC count: {GC.CollectionCount(1)}");
            stringBuilder.AppendLine($"2th Generation GC count: {GC.CollectionCount(2)}");

            return stringBuilder.ToString();
        }
    }
