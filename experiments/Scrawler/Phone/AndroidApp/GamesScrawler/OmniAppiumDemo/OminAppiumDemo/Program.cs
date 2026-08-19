using System; 
using System.IO; 
using OpenQA.Selenium; 
using OpenQA.Selenium.Appium; 
using OpenQA.Selenium.Appium.Android; 
using OpenQA.Selenium.Appium.Service; 
using OpenQA.Selenium.Support.UI;

var loginIconPath = @"D:\workspace\utility packages\Scrawler\Phone\AndroidApp\GamesScrawler\OmniAppiumDemo\胸懷三國美人計-登入畫面-登入遊戲按鈕.jpg";

var options = new AppiumOptions();
options.PlatformName = "Android";
options.AutomationName = "UiAutomator2";
options.AddAdditionalAppiumOption("udid", "10.102.213.137:5555");

// --- 加入以下參數來跳過安裝與重設 ---

// 不要重新安裝輔助程式 (如果手機已經有了)
options.AddAdditionalAppiumOption("appium:skipServerInstallation", true);

// 不要重設 App 狀態，保持目前手機的環境
options.AddAdditionalAppiumOption("appium:noReset", true);
options.AddAdditionalAppiumOption("appium:fullReset", false);
// (選配) 如果您已經手動打開了 App，可以跳過等待 App 啟動
options.AddAdditionalAppiumOption("appium:skipDeviceInitialization", true);
options.AddAdditionalAppiumOption("appium:ensureWebviewsHavePages", true);
// 遊戲包名 (剛才查到的)

// 忽略隱藏 API 政策錯誤，避免權限彈窗
options.AddAdditionalAppiumOption("appium:ignoreHiddenApiPolicyError", true);

options.AddAdditionalAppiumOption("appium:appPackage", "com.lmsg.twgp");
// 遊戲啟動的 Activity (通常需要查))
options.AddAdditionalAppiumOption("appium:appActivity", "com.wpxgame.sdk.MainActivity");

// 安全性：若需執行 shell 指令 (Appium 3 新規定：必須加驅動前綴)
// 啟動 server 時需加 --allow-insecure=uiautomator2:adb_shell
options.AddAdditionalAppiumOption("includeSafariInWebviews", true);

// Appium Server 的位址
// 注意：Appium 2.x 預設路徑通常不需要 /wd/hub，除非你有特別設定
var serverUri = new Uri("http://127.0.0.1:4723/"); 

var driver = new AndroidDriver(serverUri, options);

try
{
    Console.WriteLine("正在讀取目標圖片...");
    // 讀取你準備好的基準圖片並轉成 Base64
    // Appium 3 C# 範例
    string buttonBase64 = Convert.ToBase64String(File.ReadAllBytes(loginIconPath));
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
    driver.ExecuteScript("mobile: clickGesture", new Dictionary<string, object>
    {
        { "x", 1140 },
        { "y", 736 }
    });
    Console.WriteLine("點擊成功！");
}
catch (NoSuchElementException ex)
{
    Console.WriteLine("找不到該圖片，請確認圖片精確度或手機畫面。");
}
catch (Exception ex)
{
    Console.WriteLine($"發生錯誤: {ex.Message}");
}
finally
{
    driver.Quit();
}