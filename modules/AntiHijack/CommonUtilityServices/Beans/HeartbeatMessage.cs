using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtilityService.Beans
{
    // 第一次請求的回傳結果
    public class HeartbeatMessage
    {
        public string Nonce { get; set; } // 隨機碼
       // 注意：這裡不接收 T1，讓 T1 留在伺服器
        public long CurrentTime { get; set; } // 當前時間
        public string AppId { get; set; } // App編號
        public DateTimeOffset Timestamp { get; set; } // 時間戳記

    }
}
