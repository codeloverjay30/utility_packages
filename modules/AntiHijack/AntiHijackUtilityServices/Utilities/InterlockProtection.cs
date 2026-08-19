using System;
using System.Threading;
using System.Threading.Tasks;

namespace AntiHijackUtilityServices.Core;

/// <summary>
/// 雙重鎖定防掛起守護執行緒。若駭客使用除錯器中斷（Suspend）單一線程，另一執行緒將立即使程式 FailFast 崩潰
/// </summary>
public class InterlockProtection
{
    private static long _tickA = 0;
    private static long _tickB = 0;
    private static int _isRunning = 0; // 0 = 停止, 1 = 運行中
    private static CancellationTokenSource? _cts;

    public void StartProtectiveShield()
    {
        // 確保執行緒安全地僅初始化一次
        if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1) return;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        // 執行緒 A
        Task.Run(() => {
            while (!token.IsCancellationRequested)
            {
                Interlocked.Increment(ref _tickA);
                long lastTickB = Interlocked.Read(ref _tickB);
                
                Thread.Sleep(45); // 錯開交叉睡眠區間

                if (Interlocked.Read(ref _tickB) == lastTickB)
                {
                    SecurityBreach("偵測到背景安全執行緒 B 被駭客掛起！");
                }
            }
        }, token);

        // 執行緒 B
        Task.Run(() => {
            while (!token.IsCancellationRequested)
            {
                Interlocked.Increment(ref _tickB);
                long lastTickA = Interlocked.Read(ref _tickA);
                
                Thread.Sleep(55);

                if (Interlocked.Read(ref _tickA) == lastTickA)
                {
                    SecurityBreach("偵測到背景安全執行緒 A 被駭客掛起！");
                }
            }
        }, token);
    }

    public void StopShield()
    {
        if (Interlocked.Exchange(ref _isRunning, 0) == 1)
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }

    private static void SecurityBreach(string reason)
    {
        Interlocked.Exchange(ref _isRunning, 0);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[CRITICAL ALERT] {reason} 啟動終極防禦，強制核心中止。");
        Console.ResetColor();
        
        // 絕對不要使用普通 Environment.Exit，駭客可以掛鉤(Hook)它。FailFast 會直接向 OS 引發核心異常退出。
        Environment.FailFast($"[ANTI-HIJACK] {reason}");
    }
}