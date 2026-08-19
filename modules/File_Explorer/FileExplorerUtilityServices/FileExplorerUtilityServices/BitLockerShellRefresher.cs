using System;
using System.Runtime.InteropServices;

namespace FileExplorerUtilityServices;

public partial class BitLockerShellRefresher: IBitLockerShellRefresher
{
    // 使用現代 .NET 的 LibraryImport
    // 效能更高，且在編譯時期就會自動產生底層安全緩衝區與 P/Invoke 程式碼
    [LibraryImport("shell32.dll", EntryPoint = "SHChangeNotify")]
    public static partial void SHChangeNotify(
        int wEventId,
        uint uFlags,
        IntPtr dwItem1,
        IntPtr dwItem2
    );

    // Constants used in Notification.
    private const int SHCNE_ASSOCCHANGED = 0x08000000; // 關聯性變更（強制重新整理圖示與選單）
    private const uint SHCNF_IDLIST = 0x0000;

    public void NotifyToRefresh(ReadOnlySpan<char> drive)
    {
        // 模擬高效能字串處理：驗證或解析磁碟代號 (遵循 ReadOnlySpan 規範)
        ReadOnlySpan<char> driveTarget = drive;

        if (ValidateDriveSpan(driveTarget))
        {
            // 觸發 Windows Shell 全域通知
            // 這會告訴工作列 (Taskbar) 與檔案總管 (File Explorer) 重新載入右鍵選單的圖示資源
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }
    }

    private bool ValidateDriveSpan(ReadOnlySpan<char> drive)
    {
        // 運用 Span 進行高效能檢查，不產生額外字串垃圾 (GC clean)
        return drive.Length >= 2 && drive[1] == ':';
    }
}
