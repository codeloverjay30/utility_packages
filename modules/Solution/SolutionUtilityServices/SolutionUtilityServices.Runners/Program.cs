using SolutionUtilityServices.Runners.Services;
using System;
using System.IO;
using System.Windows.Forms;

/// <summary>
/// <see cref="Program"/> 是整合方案抽離與專案升級功能的執行入口點，支援批次與手動兩種模式。"/>
/// </summary>
/// <remarks>
/// 不使用Top-Level statements是因為需要在 Main 方法上加上 [STAThread] 屬性，
/// 以支援 Windows Forms 的 OpenFileDialog。這樣可以確保不會因為執行緒相關的問題而崩壞(Crash)。
/// </remarks>
namespace SolutionUtilityServices.Runners
{
    class Program
    {
        [STAThread] // 視窗元件必須要有這個
        static void Main(string [ ] args)
        {
            string configPath = "";

            if(args.Length > 0 && File.Exists(args [ 0 ]))
            {
                // 批次模式 (由 PowerShell 呼叫)
                configPath = args [ 0 ];
                Console.WriteLine($"[Mode: Batch] Using config: {configPath}");
            }
            else
            {
                // 手動模式 (點開 .exe)
                Application.EnableVisualStyles();
                using var openFileDialog = new OpenFileDialog
                {
                    Filter = "JSON Config (*.json)|*.json" ,
                    Title = "選擇方案抽離組態檔"
                };

                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    configPath = openFileDialog.FileName;
                    Console.WriteLine($"[Mode: Interactive] Selected: {configPath}");
                }
            }

            if(!string.IsNullOrEmpty(configPath))
            {
                ProcessAutomation(configPath);
            }
        }

        static void ProcessAutomation(string path)
        {
            // 這裡放入您之前寫好的 JSON 驅動程式碼
            var configManager = new ConfigManager();
            configManager.Execute(path);
        }
    }
}
