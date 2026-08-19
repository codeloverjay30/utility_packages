using System.Diagnostics;
using System.IO.Abstractions;
using CliWrap;
using CliWrap.Buffered;
using CustomDataAnnotations.Maintenance;

namespace GitUtilityServices;

public class DefaultCommandRunner : ICommandRunner
{
    private readonly IFileSystem _fileSystem;

    public DefaultCommandRunner(
        IFileSystem? fileSystem = null
    )
    {
        // 如果沒有傳入 mock 則使用真實檔案系統
        _fileSystem = fileSystem ?? new FileSystem();
    }

    /// <inheritdoc cref="global::GitUtilityServices.DefaultCommandRunner.ExecuteGitCommandAsync(string, string)"/>
    [Obsolete("Use Async version instead")]
    [TechnicalDebt(CategoryType.MockingIssue | CategoryType.UnitTestIssue,"ExecuteGitCommandAsync(string workingDir, string command)")]
    public void ExecuteGitCommand(string workingDir, string command)
    {
        // --- 修正部分：使用 IFileSystem 驗證目錄 ---
        if (!_fileSystem.Directory.Exists(workingDir))
        {
            throw new DirectoryNotFoundException($"The working directory does not exist: {workingDir}");
        }

        var processInfo = new ProcessStartInfo("git", command)
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true, // 建議增加錯誤輸出導向
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        if (process == null) return;

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        // 優先顯示錯誤訊息
        if (!string.IsNullOrWhiteSpace(error))
        {
            Console.WriteLine($"Error: {error}");
        }
        else if (!string.IsNullOrWhiteSpace(output))
        {
            Console.WriteLine(output);
        }
        else
        {
            Console.WriteLine("No pending changes.");
        }
    }

    /// <summary>
    /// Execute cli command about `git`
    /// </summary>
    /// <param name="workingDir"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public async Task ExecuteGitCommandAsync(string workingDir, string command)
    {
        // 驗證目錄是否存在
        if (!_fileSystem.Directory.Exists(workingDir))
        {
            throw new DirectoryNotFoundException($"The working directory does not exist: {workingDir}");
        }

        // 使用 CliWrap 設定指令
        var result = await Cli.Wrap("git")
            .WithArguments(command)
            .WithWorkingDirectory(workingDir)
            .WithValidation(CommandResultValidation.None) // 讓程式碼決定如何處理錯誤，而不是直接拋出例外
            .ExecuteBufferedAsync();

        // 處理結果
        if (result.ExitCode != 0)
        {
            // 輸出錯誤資訊
            Console.WriteLine($"Git Error (Exit Code {result.ExitCode}):");
            Console.WriteLine(result.StandardError);
        }
        else if (!string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            Console.WriteLine(result.StandardOutput);
        }
        else
        {
            Console.WriteLine("Command executed successfully with no output.");
        }
    }
}