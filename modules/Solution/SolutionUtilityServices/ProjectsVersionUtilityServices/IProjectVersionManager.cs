namespace ProjectsVersionUtilityServices;

public interface IProjectVersionManager
{
    /// <summary>
    /// 更新專案版本或套件覆寫版本
    /// </summary>
    void UpdateVersion(string projectPath, string newVersion, string? packageName = null);
    
    /// <summary>
    /// 模擬 MSBuild 向上搜尋組態檔
    /// </summary>
    string? FindConfigInAncestors(string startPath, string fileName);
}
