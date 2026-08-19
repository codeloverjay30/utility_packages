using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json; // 改用更安全且支援註解的解析器
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.PushLocal);

    [Parameter("指定掃描目錄路徑")]
    readonly AbsolutePath SolutionsPath;

    [Parameter("指定忽略組態檔 (例如 projects.ignore.json)")]
    readonly string IgnoreConfigFile;

    AbsolutePath TargetSearchPath => SolutionsPath ?? RootDirectory;
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    // 使用快取欄位避免重複 IO 掃描
    IReadOnlyCollection<AbsolutePath> _packableProjectCache;
    IReadOnlyCollection<AbsolutePath> PackableProjects => _packableProjectCache ??= GetPackableProjectPaths().ToList();

    // 關鍵效能優化：定義不需要進入掃描的目錄黑名單
    private static readonly HashSet<string> _excludeDirs = new(StringComparer.OrdinalIgnoreCase)    
    {
        "bin", "obj", ".git", ".vs", "_build", "artifacts", "node_modules", ".vshistory"
    };

    public class IgnoreSettings
    {
        public List<string> IgnoreProjectNames { get; set; } = new();
        public List<string> IgnoreDirectories { get; set; } = new();
    }

    /// <summary>
    /// 高效能掃描專案檔，並套用自定義忽略邏輯
    /// </summary>
    IEnumerable<AbsolutePath> GetPackableProjectPaths()
    {
        Log.Information("🚀 開始高效能掃描專案檔 (已排除開發暫存目錄)...");
        
        // 檢查環境變數是否正確指向你的 D 槽 SDK
        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (string.IsNullOrEmpty(dotnetRoot) || !dotnetRoot.Contains("D:", StringComparison.OrdinalIgnoreCase))
        {
            Log.Warning("⚠️ 偵測到 DOTNET_ROOT 未正確設定為 D 槽路徑，這可能會影響編譯速度或成功率。");
        }

        var settings = LoadIgnoreSettings();
        
        // 預先解析忽略目錄的絕對路徑
        var ignoredPaths = settings.IgnoreDirectories
            .Select(p => Path.IsPathRooted(p) ? p : Path.GetFullPath(Path.Combine(RootDirectory, p)))
            .Select(p => p.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            .ToList();

        // 使用 Directory.EnumerateFiles 進行串流式掃描，減少記憶體佔用
        return Directory.EnumerateFiles(TargetSearchPath, "*.csproj", SearchOption.AllDirectories)
            .Where(path => 
            {
                var dirParts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                
                // 效能核心：如果在路徑零件中發現黑名單（如 bin/obj），直接跳過該檔案
                if (dirParts.Any(p => _excludeDirs.Contains(p))) return false;

                var fileName = Path.GetFileNameWithoutExtension(path);

                // 過濾指定專案名稱
                if (settings.IgnoreProjectNames.Any(name => fileName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    return false;

                // 過濾指定目錄
                var fullFilePath = Path.GetFullPath(path);
                if (ignoredPaths.Any(ip => fullFilePath.StartsWith(ip, StringComparison.OrdinalIgnoreCase)))
                    return false;

                return true;
            })
            .Select(x => (AbsolutePath)x);
    }

    /// <summary>
    /// 使用 System.Text.Json 安全地讀取設定檔
    /// </summary>
    private IgnoreSettings LoadIgnoreSettings()
    {
        if (string.IsNullOrEmpty(IgnoreConfigFile)) return new IgnoreSettings();
        var configPath = RootDirectory / IgnoreConfigFile;
        if (!File.Exists(configPath)) return new IgnoreSettings();

        try 
        {
            var json = File.ReadAllText(configPath);
            
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip, // 允許你在 JSON 中寫 // 註解
                AllowTrailingCommas = true,                     // 允許最後一個元素後加逗號
                PropertyNameCaseInsensitive = true              // 忽略大小寫差異
            };

            return JsonSerializer.Deserialize<IgnoreSettings>(json, options) ?? new IgnoreSettings();
        } 
        catch (Exception ex) 
        {
            Log.Error($"❌ 解析組態檔失敗 (路徑: {configPath}): {ex.Message}");
            return new IgnoreSettings();
        }
    }

    Target Restore => _ => _
        .Executes(() =>
        {
            Log.Information($"📦 開始還原 {PackableProjects.Count} 個專案的依賴...");
            foreach (var project in PackableProjects)
            {
                DotNetRestore(s => s.SetProjectFile(project));
            }
        });

    Target Pack => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information($"🛠️ 準備打包 {PackableProjects.Count} 個專案...");
            foreach (var project in PackableProjects)
            {
                DotNetPack(s => s
                    .SetProject(project)
                    .SetOutputDirectory(ArtifactsDirectory)
                    .SetConfiguration(Configuration.Release)
                    .SetNoRestore(true));
            }
        });

    Target PushLocal => _ => _
        .DependsOn(Pack)
        .Executes(() =>
        {
            // 你指定的本地 NuGet 儲存庫路徑
            AbsolutePath localSource = @"D:\workspace\MyLocalNuget\ClassLibrary";
            
            Log.Information($"🚀 發佈套件至本地路徑: {localSource}");
            
            ArtifactsDirectory.GlobFiles("*.nupkg").ForEach(package =>
            {
                DotNetNuGetPush(s => s
                    .SetTargetPath(package)
                    .SetSource(localSource)
                    .SetSkipDuplicate(true));
            });
        });
}