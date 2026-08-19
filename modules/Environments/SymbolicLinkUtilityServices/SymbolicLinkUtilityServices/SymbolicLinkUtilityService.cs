using System;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using System.Security.AccessControl;
using CustomDataAnnotations.Maintenance;
using EnvironmentUtilityServices;

namespace SymbolicLinkUtilityServices;

/// <summary>
/// Utility class to update the symbolic link and keep the ACL list of old symbolic link.
/// </summary>
public class SymbolicLinkUtilityService: ISymbolicLinkUtilityService
{
    private readonly IFileSystem _fileSystem;
    private readonly IAclManager _aclManager;

    private readonly IPlatformService _platformService;
    public bool IsWindows => _platformService.IsWindows();

    public SymbolicLinkUtilityService(
        IFileSystem fileSystem,
        IAclManager aclManager,
        IPlatformService platformService
    )
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(aclManager);
        ArgumentNullException.ThrowIfNull(platformService);

        this._fileSystem = fileSystem;
        this._aclManager = aclManager;
        this._platformService = platformService;
    }

    /// <summary>
    /// Detects if the specified path is part of a cyclic symbolic link structure 
    /// that could lead to stack overflow or infinite loops during recursion.
    /// </summary>
    /// <param name="path">The absolute file system path to inspect.</param>
    /// <returns>True if a cycle is detected; otherwise, false.</returns>
    public bool IsCyclicReparsePoint(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;

        // 追蹤已訪問路徑的 HashSet，用於偵測循環參照
        var visitedPaths = new HashSet<string>();
        string currentPath = _fileSystem.Path.GetFullPath(path);

        while (!string.IsNullOrEmpty(currentPath))
        {
            if (visitedPaths.Contains(currentPath))
            {
                // 發現循環參照，返回 True
                return true;
            }

            visitedPaths.Add(currentPath);

            // 檢查該路徑是否為符號連結或重解析點
            var info = _fileSystem.DirectoryInfo.New(currentPath);
            if (!info.Exists) break;

            if (info.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                // 取得連結指向的實際目標路徑
                string? target = info.LinkTarget;
                if (string.IsNullOrEmpty(target)) break;

                // 將目標轉換為絕對路徑以進行進一步檢查
                currentPath = _fileSystem.Path.GetFullPath(target);
            }
            else
            {
                // 非連結節點，向上尋找父目錄繼續檢查
                var parent = _fileSystem.DirectoryInfo.New(currentPath).Parent;
                if (parent == null) break;
                currentPath = parent.FullName;
            }
        }

        return false;
    }


    /// <summary>
    /// Update the symbolic link with <paramref name="options"/> and keep the ACL list of old symbolic link.
    /// </summary>
    /// <param name="options"><seealso cref="global::SymbolicLinkUtilityServices.SymbolicLinkOptions"/></param>
    public void TryToUpdateLink(
        SymbolicLinkOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.LinkPath))
        {
            throw new ArgumentException("Source Link (LinkPath) must be neither null nor empty", nameof(options));
        }
        if (string.IsNullOrWhiteSpace(options.TargetPath))
        {
            throw new ArgumentException("Target Link (TargetPath) must be neither null nor empty", nameof(options));
        }

        if (options.LockObject != null)
        {
            lock (options.LockObject)
            {
                UpdateLink(options);
            }
        }
        else
        {
            UpdateLink(options);
        }
    }

    /// <summary>
    /// The core logic of updating symbolic link.
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public void UpdateLink(
        SymbolicLinkOptions options
    )
    {
        // 1. 檢查目標是否存在
        if (options.EnsureTargetExists)
        {
            bool targetExists = _fileSystem.File.Exists(options.TargetPath) || _fileSystem.Directory.Exists(options.TargetPath);
            if (!targetExists)
            {
                throw new FileNotFoundException($"terminate to update the symbolic link since {options.TargetPath} does not exists");
            }
        }

        bool isDirectory = _fileSystem.Directory.Exists(options.TargetPath);
        bool linkExists = _fileSystem.File.Exists(options.LinkPath) || _fileSystem.Directory.Exists(options.LinkPath);

        // 用於暫存舊連結權限的變數
        FileSecurity? fileSecurityBackup = null;
        DirectorySecurity? directorySecurityBackup = null;

        // 2. 安全檢查與權限備份
        if (linkExists)
        {
            bool isExistingDir = _fileSystem.Directory.Exists(options.LinkPath);

            if (options.EnsureSourceIsLink)
            {
                IFileSystemInfo linkInfo = isExistingDir
                    ? _fileSystem.DirectoryInfo.New(options.LinkPath)
                    : _fileSystem.FileInfo.New(options.LinkPath);

                if (!linkInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    throw new ArgumentException($"Access defined due to safety: '{options.LinkPath}' exists and it is considered as a symbolic link.");
                }
            }

            // 2.1 --- 核心修改：在刪除前備份舊連結的存取控制清單 (ACL) ---
            (fileSecurityBackup,directorySecurityBackup) = BackupAcl(options);

            // 3. 刪除舊的連結
            TryToDeleteSymbolicLink(isExistingDir, options.LinkPath);
        }

        // 確保父目錄存在
        string? parentDir = _fileSystem.Path.GetDirectoryName(options.LinkPath);
        if (!string.IsNullOrEmpty(parentDir) && !_fileSystem.Directory.Exists(parentDir))
        {
            _fileSystem.Directory.CreateDirectory(parentDir);
        }

        // 4. 建立新的符號連結
        TryToCreateSymbolicLink(isDirectory, options.LinkPath, options.TargetPath);

        // 5. --- 核心修改：還原權限 ---
        Restore(options,fileSecurityBackup,directorySecurityBackup);
    }

    public void TryToDeleteSymbolicLink(
        bool isDirectory,
        string linkPath
    )
    {
        if (isDirectory)
        {
            if (_fileSystem.Directory.Exists(linkPath))
            {
                _fileSystem.Directory.Delete(linkPath);
            }
        }
        else
        {
            if (_fileSystem.File.Exists(linkPath))
            {
                _fileSystem.File.Delete(linkPath);
            }
        }
    }

    public void TryToCreateSymbolicLink(
        bool isDirectory,
        string linkPath,
        string targetPath
    )
    {
        if (isDirectory)
        {
            _fileSystem.Directory.CreateSymbolicLink(linkPath, targetPath);
        }
        else
        {
            _fileSystem.File.CreateSymbolicLink(linkPath, targetPath);
        }
    }

    private (FileSecurity?, DirectorySecurity?) BackupAcl(
        SymbolicLinkOptions options
    )
    {
        FileSecurity? fileSecurity = null;
        DirectorySecurity? directorySecurity = null;
        try
        {
            bool isExistingDir = _fileSystem.Directory.Exists(options.LinkPath);

            if (IsWindows)
            {
                if (isExistingDir)
                {
                    directorySecurity = (DirectorySecurity)_aclManager.GetAccessControl(options.LinkPath, AccessControlSections.All);
                }
                else
                {
                    fileSecurity = (FileSecurity)_aclManager.GetAccessControl(options.LinkPath, AccessControlSections.All);
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // 權限不足時忽略或記錄
        }
        return (fileSecurity, directorySecurity);
    }

    [Obsolete("Use BackupAcl instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [TechnicalDebt(CategoryType.MockingIssue,"BackupAcl")]
    private (FileSecurity?, DirectorySecurity?) GetAcl(
        SymbolicLinkOptions options
    )
    {
        FileSecurity? fileSecurity = null;
        DirectorySecurity? directorySecurity = null;
        try
        {
            bool isExistingDir = _fileSystem.Directory.Exists(options.LinkPath);

            if (IsWindows)
            {
                if (isExistingDir)
                {
                    directorySecurity = _fileSystem.Directory.GetAccessControl(options.LinkPath, AccessControlSections.All);
                }
                else
                {
                    fileSecurity = _fileSystem.File.GetAccessControl(options.LinkPath, AccessControlSections.All);
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // 權限不足時忽略或記錄
        }
        return (fileSecurity, directorySecurity);
    }

    private void Restore(
            SymbolicLinkOptions options,
            FileSecurity? fileSecurity,
            DirectorySecurity? directorySecurity
        )
    {
        bool isDirectory = _fileSystem.Directory.Exists(options.TargetPath);
        if (IsWindows)
        {
            try
            {
                if (isDirectory && directorySecurity != null)
                {
                    _aclManager.SetAccessControl(options.LinkPath, directorySecurity);
                }
                else if (!isDirectory && fileSecurity != null)
                {
                    _aclManager.SetAccessControl(options.LinkPath, fileSecurity);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failure to restore ACL list of '{options.TargetPath}' (but updating symbolic link successfully) with {ex.Message}", ex);
            }
        }
    }
    
    [Obsolete("Use StoreAcl instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [TechnicalDebt(CategoryType.MockingIssue, "StoreAcl")]
    
    private void SetAcl(
        SymbolicLinkOptions options,
        FileSecurity? fileSecurity,
        DirectorySecurity? directorySecurity
    )
    {
        bool isDirectory = _fileSystem.Directory.Exists(options.TargetPath);
        if (IsWindows)
        {
            try
            {
                if (isDirectory && directorySecurity != null)
                {
                    _fileSystem.Directory.SetAccessControl(options.LinkPath, directorySecurity);
                }
                else if (!isDirectory && fileSecurity != null)
                {
                    _fileSystem.File.SetAccessControl(options.LinkPath, fileSecurity);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failure to restore ACL list of '{options.TargetPath}' (but updating symbolic link successfully) with {ex.Message}", ex);
            }
        }
    }
}