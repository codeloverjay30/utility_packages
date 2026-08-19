using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Build.Construction;

namespace ProjectsVersionUtilityServices;

public class ProjectVersionManager : IProjectVersionManager
{
    private readonly IFileSystem _fileSystem;

    public ProjectVersionManager(IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
    }

/// <summary>
    /// Updates the version of the project or a specific package within the project.
    /// </summary>
    /// <param name="projectPath">The absolute path to the project file.</param>
    /// <param name="newVersion">The new version string to apply.</param>
    /// <param name="packageName">The name of the package to update. If null, updates the project's root version.</param>
    /// <exception cref="FileNotFoundException">Thrown when the target project file does not exist.</exception>
    public void UpdateVersion(
        string projectPath,
        string newVersion,
        string? packageName = null
    )
    {
        if (!_fileSystem.File.Exists(projectPath))
        {
            throw new FileNotFoundException("The target project file was not found.", projectPath);
        }

        // 防禦性設計：從抽象檔案系統讀取文字，避免 MSBuild 引擎直接敲擊實體 I/O 導致 Mock 失效
        string originalContent = _fileSystem.File.ReadAllText(projectPath);

        // 使用 StringReader 搭配 XmlReader 載入，並明確綁定 FullPath 穩定 MSBuild DOM 狀態
        using var stringReader = new StringReader(originalContent);
        using var xmlReader = XmlReader.Create(stringReader);
        
        var project = ProjectRootElement.Create(xmlReader);
        project.FullPath = projectPath;

        // 執行版本變更邏輯
        if (!string.IsNullOrEmpty(packageName))
        {
            UpdatePackageVersion(project, packageName, newVersion);
        }
        else
        {
            UpdateProjectVersion(project, newVersion);
        }

        // ==================== 核心修正區塊 ====================
        var sb = new StringBuilder();
        
        // 使用 StringWriter 匹配 project.Save(TextWriter) 多載
        using (var stringWriter = new StringWriter(sb))
        {
            // MSBuild 內建的 Save(TextWriter) 會自動處理標準的 XML 縮排與格式
            project.Save(stringWriter);
        }

        // 透過防禦性抽象檔案系統強制寫回，確保 MockFileSystem 能正確同步並通過 FluentAssertions 斷言
        _fileSystem.File.WriteAllText(projectPath, sb.ToString());
        // ====================================================
    }

    private void UpdateProjectVersion(ProjectRootElement project, string newVersion)
    {
        var versionProp = project.Properties.FirstOrDefault(p => p.Name == "Version");
        if (versionProp != null)
        {
            versionProp.Value = newVersion;
        }
        else
        {
            var group = project.PropertyGroups.FirstOrDefault() ?? project.AddPropertyGroup();
            group.AddProperty("Version", newVersion);
        }
    }

    private void UpdatePackageVersion(ProjectRootElement project, string packageName, string newVersion)
    {
        var item = project.Items
            .FirstOrDefault(i => (i.ItemType == "PackageVersion" || i.ItemType == "PackageReference")
                                 && i.Include == packageName);

        if (item != null)
        {
            var metadata = item.Metadata.FirstOrDefault(m => m.Name == "Version" || m.Name == "VersionOverride");
            if (metadata != null)
            {
                metadata.Value = newVersion;
            }
            else
            {
                item.AddMetadata("VersionOverride", newVersion);
            }
        }
    }
    
    /// <summary>
    /// Find configuration file named <paramref name="fileName"/> recursively from down to top (from <paramref name="startPath"/> to the root drive)
    /// Behaves like `MSBuild` engine
    /// </summary>
    /// <param name="startPath">the path of container</param>
    /// <param name="fileName">file name as target that one wants to find</param>
    /// <returns></returns>

    public string? FindConfigInAncestors(
        string startPath,
        string fileName
    )
    {
        var currentDir = _fileSystem.DirectoryInfo.New(startPath);
        while (currentDir != null)
        {
            var filePath = _fileSystem.Path.Combine(currentDir.FullName, fileName);
            if (_fileSystem.File.Exists(filePath)) 
            {
                return filePath;
            }
            currentDir = currentDir.Parent;
        }
        return null;
    }
}