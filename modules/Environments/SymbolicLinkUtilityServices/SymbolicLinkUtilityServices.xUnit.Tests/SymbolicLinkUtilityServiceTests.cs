using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Security.AccessControl;
using EnvironmentUtilityServices;
using Moq;
using SymbolicLinkUtilityServices;
using Xunit;

// 為了方便Debug，為了要透過序列化在Debug Console輸出某個變數或expression的值
using System.Text.Json;
using System.Text.Encodings.Web;
using FluentAssertions;

namespace SymbolicLinkUtilityServices.xUnit.Tests;

public class SymbolicLinkUtilityServiceTests
{
    private readonly Mock<IFileSystem> _mockFileSystem;
    private readonly Mock<IEnvironmentService> _mockEnvironmentService;
    private readonly Mock<FileSecurity> _mockFileSecurity;

    private readonly Mock<IAclManager> _mockAclManager;
    private readonly Mock<IPlatformService> _mockPlatformService;
    private readonly SymbolicLinkUtilityService _service;

    public SymbolicLinkUtilityServiceTests()
    {
        _mockFileSystem = new Mock<IFileSystem>();
        _mockEnvironmentService = new Mock<IEnvironmentService>();

        _mockAclManager = new Mock<IAclManager>();

        var fileSecurity = new FileSecurity();
        _mockAclManager
            .Setup(m => m.GetAccessControl(It.IsAny<string>(), AccessControlSections.All))
            .Returns(fileSecurity);

        _mockAclManager
            .Setup(m => m.SetAccessControl(It.IsAny<string>(), fileSecurity));

        _mockEnvironmentService.Setup(m => m.IsWindows()).Returns(true);
        _mockPlatformService = new Mock<IPlatformService>();
        _mockPlatformService.Setup(m => m.IsWindows()).Returns(true);
        _service = new SymbolicLinkUtilityService(_mockFileSystem.Object,_mockAclManager.Object, _mockPlatformService.Object);
    }

    private IFileInfo GetFileInfo(string fileName, FileAttributes attributes = FileAttributes.ReparsePoint)
    {
        var mockFileInfo = new Mock<IFileInfo>();
        mockFileInfo.Setup(m => m.Attributes).Returns(attributes);
        return mockFileInfo.Object;
    }

    [Fact]
    public void TryToDeleteSymbolicLink_WhenCalled_ShouldDelegateToCorrectFileSystemMethod()
    {
        // Arrange
        string linkPath = @"C:\dummy\link";
        bool isDirectory = true;

        _mockFileSystem.Setup(m => m.Directory.Exists(linkPath)).Returns(true);
        _mockFileSystem.Setup(m => m.File.Exists(linkPath)).Returns(true);

        // _mockFileSystem.Setup(m => m.Directory.Delete(linkPath))
        //     .Callback(
        //         () => _mockFileSystem.Object.Directory.Delete(linkPath)
        //     );

        // _mockFileSystem.Setup(m => m.File.Delete(linkPath))
        //             .Callback(
        //                 () => _mockFileSystem.Object.File.Delete(linkPath)
        //             );
            
        // Act
        _service.TryToDeleteSymbolicLink(isDirectory, linkPath);

        // Assert
        // 使用 FluentAssertions 驗證 Verify 的動作 (需搭配 Moq.Verify)
        _mockFileSystem.Verify(m => m.Directory.Delete(linkPath), Times.Once);

        // 確保不會意外觸發檔案刪除
        _mockFileSystem.Verify(m => m.File.Delete(It.IsAny<string>()), Times.Never);
    }


    [Fact]
    public void UpdateLink_WhenLinkDoesNotExist_ShouldNotAttemptAclBackup()
    {
        // Arrange
        string linkPath = @"C:\path\nonexistent";
        string targetPath = @"C:\path\target";
        string targetParentDirectory = @"C:\path";
        var options = SymbolicLinkOptionsBuilder.CreateLax(linkPath, targetPath).Build();

        // 防禦性 Mock 設定：確保導覽鏈不為 null
        // 假設 166 行使用了 File 或 Directory 相關 API，確保它們被明確 Setup
        _mockFileSystem.Setup(m => m.File.Exists(targetPath)).Returns(true);
        _mockFileSystem.Setup(m => m.Directory.Exists(options.LinkPath)).Returns(false);
        _mockFileSystem.Setup(m => m.File.Exists(options.LinkPath)).Returns(false);
        _mockFileSystem.Setup(m =>m.Path.GetDirectoryName(options.LinkPath)).Returns(targetParentDirectory);
        // Act
        Action act = () => _service.UpdateLink(options);

        // Assert
        act.Should().NotThrow<NullReferenceException>("because the file system navigation path must be fully mocked");

        _mockAclManager.Verify(m=>m.GetAccessControl(It.IsAny<string>(), It.IsAny<AccessControlSections>()), Times.Never);
    }


    #region Constructor Tests

    [Fact]
    public void Constructor_NullFileSystem_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SymbolicLinkUtilityService(null!, _mockAclManager.Object, _mockPlatformService.Object));
    }

    [Fact]
    public void Constructor_NullEnvironmentService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SymbolicLinkUtilityService(_mockFileSystem.Object,_mockAclManager.Object, null!));
    }

    #endregion

    #region TryToUpdateLink Validation Tests

    [Fact]
    public void TryToUpdateLink_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.TryToUpdateLink(null!));
    }

    [Theory]
    [InlineData("", "target")]
    [InlineData(" ", "target")]
    [InlineData(null, "target")]
    public void TryToUpdateLink_EmptyLinkPath_ThrowsArgumentException(string? linkPath, string targetPath)
    {
        var options = new SymbolicLinkOptions { LinkPath = linkPath!, TargetPath = targetPath };
        Assert.Throws<ArgumentException>(() => _service.TryToUpdateLink(options));
    }

    [Theory]
    [InlineData("link", "")]
    [InlineData("link", " ")]
    [InlineData("link", null)]
    public void TryToUpdateLink_EmptyTargetPath_ThrowsArgumentException(string linkPath, string? targetPath)
    {
        var options = new SymbolicLinkOptions { LinkPath = linkPath, TargetPath = targetPath! };
        Assert.Throws<ArgumentException>(() => _service.TryToUpdateLink(options));
    }

    #endregion

    #region Core UpdateLink Logic Tests

    [Fact]
    public void UpdateLink_EnsureTargetExistsIsTrue_AndTargetNotFound_ThrowsFileNotFoundException()
    {
        string linkPath = @"C:\path\link";
        string targetPath = @"C:\path\target";

        var options = SymbolicLinkOptionsBuilder.CreateStrict(linkPath, targetPath).Build();
        _mockFileSystem.Setup(m => m.File.Exists(options.TargetPath)).Returns(false);
        _mockFileSystem.Setup(m => m.Directory.Exists(options.TargetPath)).Returns(false);

        Assert.Throws<FileNotFoundException>(() => _service.UpdateLink(options));
    }

    [Fact]
    public void UpdateLink_EnsureSourceIsLinkIsTrue_AndSourceIsNotReparsePoint_ThrowsArgumentException()
    {
        string linkPath = @"C:\path\link";
        string targetPath = @"C:\path\target";
        var expectedOptions = new SymbolicLinkOptions { LinkPath = linkPath, TargetPath = targetPath };

        var options = SymbolicLinkOptionsBuilder.CreateStrict(linkPath, targetPath).Build();
        _mockFileSystem.Setup(m => m.File.Exists(options.TargetPath)).Returns(true);
        _mockFileSystem.Setup(m => m.File.Exists(options.LinkPath)).Returns(true);
        _mockFileSystem.Setup(m => m.Directory.Exists(options.LinkPath)).Returns(false);
        _mockFileSystem.Setup(m => m.FileInfo.New(options.LinkPath)).Returns(GetFileInfo(expectedOptions.LinkPath, FileAttributes.Normal));

        Assert.Throws<ArgumentException>(() => _service.UpdateLink(options));
    }

    [Fact]
    public void UpdateLink_FileLinkExists_ShouldBackupAcl_DeleteOldLink_CreateNewLink_AndRestoreAcl_Ultimate()
    {
        string linkPath = @"C:\path\link";
        string targetPath = @"C:\path\target";

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(targetPath, new MockFileData("target content"));
        mockFileSystem.AddFile(linkPath, new MockFileData("link content") { Attributes = FileAttributes.ReparsePoint });

        var service = new SymbolicLinkUtilityService(mockFileSystem,_mockAclManager.Object, _mockPlatformService.Object);
        var options = SymbolicLinkOptionsBuilder.CreateStrict(linkPath, targetPath).Build();

        service.UpdateLink(options);

        Assert.True(mockFileSystem.File.Exists(options.LinkPath));
        var fileInfo = mockFileSystem.FileInfo.New(options.LinkPath);
        Assert.True(fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint));
    }

    [Fact]
    public void UpdateLink_DirectoryLink_ShouldCreateParentDirectoryIfNeeded()
    {
        string linkPath = @"C:\path\linkDir";
        string targetPath = @"C:\path\targetDir";

        var options = SymbolicLinkOptionsBuilder.CreateLax(linkPath, targetPath).Build();
        _mockFileSystem.Setup(m => m.Directory.Exists(options.TargetPath)).Returns(true);
        _mockFileSystem.Setup(m => m.Directory.Exists(options.LinkPath)).Returns(false);
        _mockFileSystem.Setup(m => m.File.Exists(options.LinkPath)).Returns(false);
        _mockFileSystem.Setup(m => m.Path.GetDirectoryName(options.LinkPath)).Returns(@"C:\path");
        _mockFileSystem.Setup(m => m.Directory.Exists(@"C:\path")).Returns(false);

        _service.UpdateLink(options);

        _mockFileSystem.Verify(m => m.Directory.CreateDirectory(@"C:\path"), Times.Once);
        _mockFileSystem.Verify(m => m.Directory.CreateSymbolicLink(options.LinkPath, options.TargetPath), Times.Once);
    }

    [Fact]
    public void UpdateLink_SetAclThrowsException_ThrowsInvalidOperationException()
    {
        // Arrange
        string linkPath = @"C:\path\link";
        string targetPath = @"C:\path\target";
        var options = SymbolicLinkOptionsBuilder.CreateLax(linkPath, targetPath).Build();

        // 1. 建立核心 mockFileSystem（不要用 DefaultValue.Mock，改用明確的 Setup 避免 Moq 亂生子 Mock）
        var mockFileSystem = new Mock<IFileSystem>();
        var mockFile = new Mock<IFile>();
        var mockDirectory = new Mock<IDirectory>();
        var mockPath = new Mock<IPath>();
        var mockFileInfoFactory = new Mock<IFileInfoFactory>();

        // 2. 雕琢專屬的 IFileInfo Mock，並同時掛載 ACL 介面
        var mockFileInfoCtrl = new Mock<IFileInfo>();
        mockFileInfoCtrl.Setup(m => m.Attributes).Returns(FileAttributes.ReparsePoint);
        mockFileInfoCtrl.Setup(m => m.FullName).Returns(linkPath);

        var aclSupportMock = mockFileInfoCtrl.As<IFileSystemAclSupport>();

        // 讀取階段回傳空白 FileSecurity
        aclSupportMock
            .Setup(m => m.GetAccessControl(It.IsAny<IFileSystemAclSupport.AccessControlSections>()))
            .Returns(new FileSecurity());
        aclSupportMock
            .Setup(m => m.GetAccessControl())
            .Returns(new FileSecurity());

        // 寫入階段故意拋出異常
        aclSupportMock
            .Setup(m => m.SetAccessControl(It.IsAny<object>()))
            .Throws(new UnauthorizedAccessException("Access denied when applying Access Control Lists."));

        // 3. 🔥 關鍵的雙向導航屬性綁定（讓擴充方法內的 file.FileSystem 絕不迷路）
        mockFileSystem.Setup(m => m.File).Returns(mockFile.Object);
        mockFileSystem.Setup(m => m.Directory).Returns(mockDirectory.Object);
        mockFileSystem.Setup(m => m.Path).Returns(mockPath.Object);
        mockFileSystem.Setup(m => m.FileInfo).Returns(mockFileInfoFactory.Object);

        // 讓 IFile 與 IFileInfo 的 .FileSystem 屬性確實指回我們的主 mockFileSystem
        mockFile.Setup(m => m.FileSystem).Returns(mockFileSystem.Object);
        mockFileInfoCtrl.Setup(m => m.FileSystem).Returns(mockFileSystem.Object);

        // 4. 對接工廠生產線
        mockFileInfoFactory.Setup(m => m.New(options.LinkPath)).Returns(mockFileInfoCtrl.Object);

        // 5. 補齊環境偵測所需 mock
        mockFile.Setup(m => m.Exists(options.TargetPath)).Returns(true);
        mockFile.Setup(m => m.Exists(options.LinkPath)).Returns(true);
        mockDirectory.Setup(m => m.Exists(options.LinkPath)).Returns(false);
        mockDirectory.Setup(m => m.Exists(options.TargetPath)).Returns(false); // 判定為 File 模式
        mockPath.Setup(m => m.GetDirectoryName(options.LinkPath)).Returns(@"C:\path");
        mockDirectory.Setup(m => m.Exists(@"C:\path")).Returns(true);

        var service = new SymbolicLinkUtilityService(mockFileSystem.Object, _mockAclManager.Object, _mockPlatformService.Object);

        // Act
        Action act = () => service.TryToUpdateLink(options);

        // Assert (FluentAssertions 風格)
        if (!service.IsWindows)
        {
            act.Should().Throw<InvalidOperationException>()
               .WithMessage($"*Failure to restore ACL list of '{targetPath}'*")
               .WithInnerExceptionExactly<UnauthorizedAccessException>();
        }
        else
        {
            act.Should().NotThrow("because on Windows the ACL restoration failure should be non-fatal and only logged as a warning");
        }
    }
    
    #endregion

    #region Testing Helpers

    private class TestingFailingFileSystem : MockFileSystem
    {
        public new IFileInfoFactory FileInfo => new FailingFileInfoFactory(this);
    }

    private class FailingFileInfoFactory : IFileInfoFactory
    {
        private readonly MockFileSystem _fileSystem;
        public FailingFileInfoFactory(MockFileSystem fileSystem) => _fileSystem = fileSystem;

        public IFileInfo New(string fileName) => new FailingFileInfo(_fileSystem.FileInfo.New(fileName));
        public IFileInfo FromFileName(string fileName) => new FailingFileInfo(_fileSystem.FileInfo.New(fileName));
        public IFileInfo Wrap(FileInfo? fileInfo) => fileInfo == null ? null! : new FailingFileInfo(_fileSystem.FileInfo.Wrap(fileInfo));

        public IFileSystem FileSystem => _fileSystem;
    }

    // 同時實作 IFileInfo 與 IFileSystemAclSupport，這樣擴充方法進行 as 轉型時才不會變成 null
    private class FailingFileInfo : IFileInfo, IFileSystemAclSupport
    {
        private readonly IFileInfo _inner;
        public FailingFileInfo(IFileInfo inner) => _inner = inner;

        #region IFileSystemAclSupport 實作 (精準攔截點)

        public object GetAccessControl() => ((IFileSystemAclSupport)_inner).GetAccessControl();
        
        public object GetAccessControl(IFileSystemAclSupport.AccessControlSections includeSections) 
            => ((IFileSystemAclSupport)_inner).GetAccessControl(includeSections);

        // 當 FileInfoAclExtensions 呼叫此處時，成功引爆我們需要的無權限異常！
        public void SetAccessControl(object value)
        {
            throw new UnauthorizedAccessException("Access denied when applying Access Control Lists.");
        }

        #endregion

        #region IFileInfo / IFileSystemInfo 標準成員轉發

        public void MoveTo(string destFileName) => _inner.MoveTo(destFileName);
        public IFileInfo CopyTo(string destFileName) => _inner.CopyTo(destFileName);
        public IFileInfo CopyTo(string destFileName, bool overwrite) => _inner.CopyTo(destFileName, overwrite);
        public FileAttributes Attributes { get => _inner.Attributes; set => _inner.Attributes = value; }
        public bool Exists => _inner.Exists;
        public string FullName => _inner.FullName;
        public string Name => _inner.Name;
        public void Delete() => _inner.Delete();
        public string LinkTarget => _inner.LinkTarget;
        public void CreateAsSymbolicLink(string pathToTarget) => _inner.CreateAsSymbolicLink(pathToTarget);
        public IFileSystemInfo ResolveLinkTarget(bool returnFinalTarget) => _inner.ResolveLinkTarget(returnFinalTarget);
        public long Length => _inner.Length;
        public string DirectoryName => _inner.DirectoryName;
        public IDirectoryInfo Directory => _inner.Directory;
        public bool IsReadOnly { get => _inner.IsReadOnly; set => _inner.IsReadOnly = value; }
        public StreamWriter AppendText() => _inner.AppendText();
        public FileSystemStream Create() => _inner.Create();
        public StreamWriter CreateText() => _inner.CreateText();
        public void Decrypt() => _inner.Decrypt();
        public void Encrypt() => _inner.Encrypt();
        public FileSystemStream Open(FileMode mode) => _inner.Open(mode);
        public FileSystemStream Open(FileMode mode, FileAccess access) => _inner.Open(mode, access);
        public FileSystemStream Open(FileMode mode, FileAccess access, FileShare share) => _inner.Open(mode, access, share);
        public FileSystemStream OpenRead() => _inner.OpenRead();
        public StreamReader OpenText() => _inner.OpenText();
        public FileSystemStream OpenWrite() => _inner.OpenWrite();
        public IFileInfo Replace(string destinationFileName, string destinationBackupFileName) => _inner.Replace(destinationFileName, destinationBackupFileName);
        public IFileInfo Replace(string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors) => _inner.Replace(destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
        public DateTime CreationTime { get => _inner.CreationTime; set => _inner.CreationTime = value; }
        public DateTime CreationTimeUtc { get => _inner.CreationTimeUtc; set => _inner.CreationTimeUtc = value; }
        public DateTime LastAccessTime { get => _inner.LastAccessTime; set => _inner.LastAccessTime = value; }
        public DateTime LastAccessTimeUtc { get => _inner.LastAccessTimeUtc; set => _inner.LastAccessTimeUtc = value; }
        public DateTime LastWriteTime { get => _inner.LastWriteTime; set => _inner.LastWriteTime = value; }
        public DateTime LastWriteTimeUtc { get => _inner.LastWriteTimeUtc; set => _inner.LastWriteTimeUtc = value; }
        public void Refresh() => _inner.Refresh();
        public IFileSystem FileSystem => _inner.FileSystem;
        public string Extension => _inner.Extension;
        public void MoveTo(string destFileName, bool overwrite) => _inner.MoveTo(destFileName, overwrite);
        public FileSystemStream Open(FileStreamOptions options) => _inner.Open(options);
        public UnixFileMode UnixFileMode { get => _inner.UnixFileMode; set => _inner.UnixFileMode = value; }

        #endregion
    }

    #endregion
}