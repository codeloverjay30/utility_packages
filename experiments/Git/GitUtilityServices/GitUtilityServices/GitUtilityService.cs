using System;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace GitUtilityServices;

public class GitUtilityService : IGitUtilityService
{
    private readonly ICommandRunner _commandRunner;
    private readonly IFileSystem _fileSystem;

    public GitUtilityService(
        ICommandRunner commandRunner,
        IFileSystem? fileSystem = null
    )
    {
        ArgumentNullException.ThrowIfNull(commandRunner, nameof(commandRunner));
        _commandRunner = commandRunner;
        _fileSystem = fileSystem ?? new FileSystem();
    }

    public void CheckModules(string rootPath)
    {
        // Use the injected fileSystem instead of static Directory
        var directories = _fileSystem.Directory.GetDirectories(rootPath);

        foreach (var dir in directories)
        {
            Console.WriteLine($"--- Checking: {_fileSystem.Path.GetFileName(dir)} ---");
            _commandRunner.ExecuteGitCommand(dir, "status -s");
        }
    }

    public async Task CheckModulesAsync(string rootPath)
    {
        var directories = _fileSystem.Directory.GetDirectories(rootPath);

        foreach (var dir in directories)
        {
            Console.WriteLine($"--- Checking: {_fileSystem.Path.GetFileName(dir)} ---");
            
            // 現在可以安全地 await 每個 Git 指令執行完成
            await _commandRunner.ExecuteGitCommandAsync(dir, "status -s");
        }
    }

    public void UpdateAndCommit(
        string solutionPath,
        string message
    )
    {
        // 1. 執行 dotnet pack 打包模組
        // 2. 獲取新的版本號並更新至引用端專案檔 (.csproj)

        var csprojFiles = _fileSystem.Directory.GetFiles(solutionPath, "*.csproj", SearchOption.AllDirectories);
        foreach (var file in csprojFiles)
        {
            // 邏輯：使用 XDocument 修改 Version 節點
            Console.WriteLine($"Updating version in {_fileSystem.Path.GetFileName(file)}...");
        }

        // 3. 自動執行 Git Add & Commit
        // 這確保了「模組更新」與「依賴變更」會在同一個 Commit 紀錄中
        _commandRunner.ExecuteGitCommand(solutionPath, "add .");
        _commandRunner.ExecuteGitCommand(solutionPath, $"commit -m \"{message}\"");
    }
    
    public async Task UpdateAndCommitAsync(
        string solutionPath, 
        string message
    )
    {
        var csprojFiles = _fileSystem.Directory.GetFiles(solutionPath, "*.csproj", SearchOption.AllDirectories);
        foreach (var file in csprojFiles)
        {
            // 使用 _fileSystem.Path 保持一致性
            Console.WriteLine($"Updating version in {_fileSystem.Path.GetFileName(file)}...");
        }

        // 確保指令按順序執行完成
        await _commandRunner.ExecuteGitCommandAsync(solutionPath, "add .");
        await _commandRunner.ExecuteGitCommandAsync(solutionPath, $"commit -m \"{message}\"");
    }
}