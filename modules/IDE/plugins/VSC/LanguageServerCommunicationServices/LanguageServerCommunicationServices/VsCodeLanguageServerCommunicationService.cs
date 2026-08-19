using CliUtilityServices;
using CliWrap;
using LanguageServerCommunicationServices;
using LanguageServerUtilityServices.Infrastructure.Interfaces;

namespace LanguageServerCommunicationService;

public class VsCodeLanguageServerCommunicationService : ILanguageServerCommunicationService
{
    private readonly ILanguageServerUtilityService _languageServerUtilityService;
    public VsCodeLanguageServerCommunicationService(
        ILanguageServerUtilityService languageServerUtilityService
    )
    {
        ArgumentNullException.ThrowIfNull(languageServerUtilityService, nameof(languageServerUtilityService));

        _languageServerUtilityService = languageServerUtilityService;
    }

    /// <inheritdoc cref="global::LanguageServerCommunicationServices.ILanguageServerCommunicationService.ShowInfoAsync(string, PluginInfo, CancellationToken)"/>
    public async Task ShowInfoAsync(
        string message,
        PluginInfo pluginInfo,
        CancellationToken cancellationToken = default
    )
    {
        // 防禦性檢查：訊息不可為空白
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        ArgumentNullException.ThrowIfNull(pluginInfo);

        // 防禦性檢查：插件名稱不可為空白
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginInfo.Name);

        try
        {
            // 對傳入參數進行跳脫處理，防止命令列注入或引號破壞結構
            string escapedMessage = message.Replace("\"", "\\\"");
            IEnumerable<string> arguments = new List<string>()
            {
                $"--reuse-window",
                $"--command {pluginInfo.Name}.showMessage \"{escapedMessage}\"",
            };

            await _languageServerUtilityService.ShowMessageAsync("code", arguments, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // 攔截 CliUtilityServices 因逾時觸發的取消例外
            throw new TimeoutException("向 VS Code 發送訊息逾時（超過 3 秒）。");
        }
        catch (Exception ex)
        {
            // 防禦性例外封裝，記錄並向上拋出或轉換為應用層級例外，避免外部應用程式崩潰
            throw new InvalidOperationException($"無法向 VS Code 發送訊息: {ex.Message}", ex);
        }
    }
        
    /// <inheritdoc cref="global::LanguageServerCommunicationServices.ILanguageServerCommunicationService.ExecuteAsync(CommandLineInput, CancellationToken)"/>
    public async Task ExecuteAsync(
        CommandLineInput commandLineInput,
        CancellationToken cancellationToken = default
    )
    {
        // 防禦性檢查：指令不可為空
        ArgumentNullException.ThrowIfNull(commandLineInput,nameof(commandLineInput));

        try
        {
            // 設定逾時防禦機制（例如：3 秒逾時，避免程序無回應卡死）
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            // 執行自動偵測與呼叫
            await _languageServerUtilityService.StartAsync(commandLineInput);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // 攔截 CliUtilityServices 因逾時觸發的取消例外
            throw new TimeoutException("向 VS Code 發送訊息逾時（超過 3 秒）。");
        }
        catch (Exception ex)
        {
            // 防禦性例外封裝，記錄並向上拋出或轉換為應用層級例外，避免外部應用程式崩潰
            throw new InvalidOperationException($"無法向 VS Code 發送訊息: {ex.Message}", ex);
        }
    }
}
