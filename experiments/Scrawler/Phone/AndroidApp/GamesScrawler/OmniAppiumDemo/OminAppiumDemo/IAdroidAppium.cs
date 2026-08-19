
using System;
using System.Collections.ObjectModel;

namespace OpenQA.Selenium
{
    /// <summary>
    /// 定義用於操控瀏覽器或行動裝置應用程式的介面。
    /// </summary>
    public interface IWebDriver : ISearchContext, IDisposable
    {
        // 獲取目前視窗的 URL (在 Appium 中通常代表 Webview 的網址)
        string Url { get; set; }

        // 獲取目前視窗的標題
        string Title { get; }

        // 獲取目前頁面的原始碼 (XML 或 HTML)
        string PageSource { get; }

        // 獲取目前視窗的句柄 (Handle)
        string CurrentWindowHandle { get; }

        // 獲取所有開啟視窗的句柄
        ReadOnlyCollection<string> WindowHandles { get; }

        // 關閉目前視窗
        void Close();

        // 結束目前的 Session 並關閉所有關聯的視窗
        void Quit();

        // 切換操控對象 (如視窗、框架、警示視窗)
        ITargetLocator SwitchTo();

        // 進行導覽動作 (如回到上一頁、重新整理)
        INavigation Navigate();

        // 設定管理工具 (如 Cookie、逾時設定、視窗大小)
        IOptions Manage();
    }
}