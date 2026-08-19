using System.IO.Abstractions;
using EnvironmentUtilityServices;

namespace DriveInfoUtilityServices;

public class DriveInfoUtilityService: IDriveInfoUtilityService
{
    
    private readonly IFileSystem _fileSystem;
    
    private readonly IPlatformService _platformService;

    public DriveInfoUtilityService(
        IFileSystem fileSystem,
        IPlatformService platformService
    )
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(platformService);


        this._fileSystem = fileSystem;
        this._platformService = platformService;
    }

    /// <summary>
    /// Check <paramref name="path1"/> and <paramref name="path2"/> are in different drives or not.
    /// </summary>
    /// <param name="path1">path of drive 1</param>
    /// <param name="path2">path of drive 2</param>
    /// <returns>
    /// returns true iff <paramref name="path1"/> and <paramref name="path2"/> are in same drive or 
    /// </returns>

    public bool IsCrossDrive(
        string path1,
        string path2
    )
    {
        string fullPath1 = _fileSystem.Path.GetFullPath(path1);
        string fullPath2 = _fileSystem.Path.GetFullPath(path2);

        
        IDriveInfo? d1 = GetDriveInfo(fullPath1);
        IDriveInfo? d2 = GetDriveInfo(fullPath2);

        StringComparison comparison = _platformService.GetComparison();
        return !string.Equals(d1?.Name, d2?.Name, comparison);
    }

    public IDriveInfo? GetDriveInfo(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        try
        {
            // 策略 A：Windows 環境可以直接透過根目錄識別
            if (_platformService.IsWindows())
            {
                string pathRoot = _fileSystem.Path.GetPathRoot(path) ?? string.Empty;
                if (!string.IsNullOrEmpty(pathRoot))
                {
                    return _fileSystem.DriveInfo.New(pathRoot);
                }
            }

            // 策略 B：跨平台（Linux/macOS）通用解法
            var directoryInfo = _fileSystem.DirectoryInfo.New(path);
            var allDrives = _fileSystem.DriveInfo.GetDrives();

            IDirectoryInfo? currentDir = directoryInfo;

            while (currentDir != null)
            {
                // 比對目前的資料夾路徑是否就是某個磁碟的掛載點名稱
                var match = allDrives.FirstOrDefault(d =>
                    string.Equals(d.Name.TrimEnd(_fileSystem.Path.DirectorySeparatorChar),
                                  currentDir.FullName.TrimEnd(_fileSystem.Path.DirectorySeparatorChar),
                                  StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    return match;
                }

                currentDir = currentDir.Parent;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// Defensively validates whether the logical drive of the specified target path is fully ready, 
    /// accessible, and safe for subsequent I/O recursive operations.
    /// </summary>
    /// <param name="targetPath">The absolute directory path to evaluate.</param>
    /// <returns>True if the drive exists, is marked as ready by the OS, and its root is accessible; otherwise, false.</returns>
    public bool IsDriveReadyAndAccessible(string targetPath)
    {
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return false;
        }

        try
        {
            // 1. 防禦不合法的路徑格式，確保能正確解析出驅動器根目錄 (e.g., "C:\")
            string? pathRoot = _fileSystem.Path.GetPathRoot(targetPath);
            if (string.IsNullOrWhiteSpace(pathRoot))
            {
                return false;
            }

            // 運行時，Windows 環境
            if (_platformService.IsWindows())
            {
                if (_platformService.IsUncPath(targetPath))
                {
                    return _fileSystem.Directory.Exists(targetPath);
                }

                // 2. 調用您現有的 DriveInfoUtilityServices 取得 IIDriveInfo 抽象對象
                IDriveInfo? driveInfo = GetDriveInfo(targetPath);
                if (driveInfo == null)
                {
                    return false;
                }

                // 3. 核心防禦點：驗證 IsReady 狀態
                // 嚴防拔除的隨身碟或斷線的網路磁碟機。
                if (!driveInfo.IsReady)
                {
                    return false;
                }

                // 4. 進階防禦點：檢查磁碟類型與根目錄存在性，嚴防作業系統邊界死鎖
                if (driveInfo.RootDirectory == null || !driveInfo.RootDirectory.Exists)
                {
                    return false;
                }

                // 5. 最終權限探查：嘗試讀取磁碟可用空間，確保進程未被 ACL (存取控制清單) 封鎖
                // 若此處拋出 UnauthorizedAccessException 或 IOException，代表磁碟實質不可用。
                long remainingSpace = driveInfo.TotalFreeSpace;
                return true;
            }
            // 運行時，非Windows 環境
            return _fileSystem.Directory.Exists(targetPath);
        }
        catch (ArgumentException)
        {
            // 攔截不合法的路徑字元
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            // 攔截權限不足的磁碟
            return false;
        }
        catch (IOException)
        {
            // 攔截底層硬體錯誤（如損毀的磁區或瞬間斷線的網路共用資料夾）
            return false;
        }
    }
}