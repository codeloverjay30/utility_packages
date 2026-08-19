using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGraphServices;
    public class GCGraphService
    {
        private const int KEY_INPUT_TIME = 10; // 每10ms偵測使用者按下哪個鍵盤按鍵
        private const int RERENDER_TIME = 100; // 每100ms重新渲染
        public void GraphGCStatus()
        {
            Console.WriteLine("Please press c to continue...");

            // 建立一個表格
            var table = new Table().Border(TableBorder.Rounded).Title("[yellow]GC 即時監控儀表板[/]");
            table.AddColumn("[grey]指標[/]");
            table.AddColumn("[bold white]數值[/]");

            AnsiConsole.Live(table)
                .AutoClear(false)
                .Start(ctx =>
                {
                    // 開始啟動計時器
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    while(true)
                    {

                        ConsoleKeyInfo keyInfo;
                        if(Console.KeyAvailable)
                        {
                            keyInfo = Console.ReadKey(true);
                            // 按下 'c' 鍵退出
                            if(keyInfo.Key == ConsoleKey.C) 
                            {
                                break;
                            }
                        }

                        if(sw.ElapsedMilliseconds >= RERENDER_TIME)
                        {
                            // 每100ms重新渲染表格
                            table.Rows.Clear();

                            // 取得數據
                            GCMemoryInfo gcInfo = GC.GetGCMemoryInfo();
                            var memory = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
                            var g0 = GC.CollectionCount(0);
                            var g1 = GC.CollectionCount(1);
                            var g2 = GC.CollectionCount(2);

                            // 繪製內容
                            table.AddRow("總記憶體使用" , $"{memory:F2} MB");
                            table.AddRow("最大代數" , $"[yellow]{GC.MaxGeneration:F2}[/]");
                            table.AddRow("是否為壓縮式GC?" , $"[yellow]{gcInfo.Compacted}[/]");
                            table.AddRow("Heap 大小 (bytes)" , $"[yellow]{gcInfo.HeapSizeBytes:F2}[/]");
                            table.AddRow("記憶體負載 (bytes)" , $"[yellow]{gcInfo.MemoryLoadBytes:F2}[/]");
                            table.AddRow("上次是否為阻塞式" , $"[grey]{gcInfo.Index}[/]");
                            table.AddRow("針對這次回收代數，GC目前正在觀察的物件數量" , $"[grey]{gcInfo.PinnedObjectsCount}[/]");
                            table.AddRow("針對這次回收，物件晉升量 (bytes)" , $"[grey]{gcInfo.PromotedBytes}[/]");
                            table.AddRow("Gen 0 回收次數" , $"[green]{g0}[/]");
                            table.AddRow("Gen 1 回收次數" , $"[blue]{g1}[/]");
                            table.AddRow("Gen 2 回收次數" , $"[red]{g2}[/]");

                            ctx.Refresh();

                            sw.Restart();
                        }

                        // 每10ms偵測使用者按下哪個鍵盤按鍵
                        Thread.Sleep(KEY_INPUT_TIME); 
                    }
                });
        }
    }

