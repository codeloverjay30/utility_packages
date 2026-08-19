using Azure.Identity;
using Azure.ResourceManager;

// 因為你剛才在 CLI 成功 az login，
// 下面這行程式碼會自動找到剛才那個 "Azure subscription 1"
var credential = new DefaultAzureCredential();
var armClient = new ArmClient(credential);

var sub = armClient.GetSubscriptionResource(
    new Azure.Core.ResourceIdentifier("/subscriptions/6508e063-d36f-4fd8-80ad-82e02273cccc")
);

var subData = sub.Get().Value;
Console.WriteLine($"驗證成功！目前 C# 正在操作的訂閱是：{subData.Data.DisplayName}");

Console.WriteLine($"訂閱名稱：{subData.Data.DisplayName}");
Console.WriteLine($"目前狀態：{subData.Data.State}"); // 這裡應該會顯示 Disabled 或 Warned

// 嘗試執行一個真正的資源操作，這通常會失敗
try
{
    Console.WriteLine("嘗試讀取資源群組...");
    var groups = sub.GetResourceGroups().GetAll();
    foreach(var rg in groups) { Console.WriteLine(rg.Data.Name); }
}
catch(Exception ex)
{
    Console.WriteLine($"操作失敗：{ex.Message}");
}
