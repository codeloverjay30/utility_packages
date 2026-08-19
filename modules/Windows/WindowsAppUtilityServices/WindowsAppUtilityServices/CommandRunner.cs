using System;
using System.Diagnostics;
using System.Diagnostics.Abstractions;

namespace WindowsAppUtilityServices;

public class CommandRunner : ICommandRunner
{
    private readonly Func<IProcess> _processFactory;

    // 注入 IProcess 工廠，以便在執行時建立新的實例
    public CommandRunner(Func<IProcess> processFactory)
    {
        _processFactory = processFactory;
    }

    /// <inheritdoc cref="global::WindowsAppUtilityServices.ICommandRunner.ExecuteCommand(string)"/>
    public void ExecuteCommand(string command)
    {
        // 設定處理程序啟動資訊
        var procStartInfo = new ProcessStartInfo("cmd", "/c " + command)
        {
            RedirectStandardOutput = false,
            UseShellExecute = true,
            CreateNoWindow = false,
            Verb = "runas"
        };

        // 使用工廠建立 IProcess 實例並確保其在使用後釋放資源
        using (IProcess proc = _processFactory())
        {
            proc.StartInfo = procStartInfo;
            proc.Start();
            proc.WaitForExit();
        }
    }
}
    