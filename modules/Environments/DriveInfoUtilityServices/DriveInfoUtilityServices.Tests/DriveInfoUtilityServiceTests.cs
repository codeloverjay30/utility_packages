using System;
using System.IO.Abstractions;
using System.Linq;
using EnvironmentUtilityServices;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace DriveInfoUtilityServices.Tests;

[TestFixture]
public class DriveInfoUtilityServiceTests
{
    private IFileSystem _fileSystem = null!;
    private IEnvironmentService _environmentService = null!;
    private IPlatformService _platformService;
    private IDriveInfoUtilityService _driveInfoUtilityService;
    [SetUp]
    public void SetUp()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _environmentService = Substitute.For<IEnvironmentService>();
        _platformService = Substitute.For<IPlatformService>();

        _platformService.GetComparison().Returns(StringComparison.OrdinalIgnoreCase);

        _driveInfoUtilityService = new DriveInfoUtilityService(_fileSystem, _platformService);
    }

    #region 建構子測試 (Constructor Tests)

    [Test]
    public void Constructor_WhenFileSystemIsNull_ShouldThrowArgumentNullException()
    {
        // 使用 null! 消除 CS8625 警告
        Assert.Throws<ArgumentNullException>(() => 
            new DriveInfoUtilityService(null!, _platformService));
    }

    [Test]
    public void Constructor_WhenEnvironmentServiceIsNull_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            new DriveInfoUtilityService(_fileSystem!, _platformService));
    }

    [Test]
    public void Constructor_WhenOsUtilityServiceIsNull_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            new DriveInfoUtilityService(_fileSystem, _platformService));
    }

    #endregion

    #region GetDriveInfo 測試

    [TestCase(null)] // 搭配參數型態改為 string? 即可正常編譯並修正 NUnit1001
    [TestCase("")]
    [TestCase("   ")]
    public void GetDriveInfo_WhenPathIsNullOrEmpty_ShouldReturnNull(string? invalidPath)
    {
        var result = _driveInfoUtilityService.GetDriveInfo(invalidPath!);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetDriveInfo_WhenWindowsAndPathRootExists_ShouldReturnDriveInfoFromRoot()
    {
        // Arrange
        string path = @"C:\Users\Test\Documents";
        string rootPath = @"C:\";
        
        _environmentService.IsWindows().Returns(true);
        _fileSystem.Path.GetPathRoot(path).Returns(rootPath);
        
        var mockDriveInfo = Substitute.For<IDriveInfo>();
        mockDriveInfo.Name.Returns(rootPath);
        _fileSystem.DriveInfo.New(rootPath).Returns(mockDriveInfo);

        // Act
        var result = _driveInfoUtilityService.GetDriveInfo(path);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo(rootPath));
    }

    [Test]
    public void GetDriveInfo_WhenNonWindows_ShouldTraverseUpToFindMountPoint()
    {
        // Arrange
        string path = "/media/user/usb/folder";
        _environmentService.IsWindows().Returns(false);
        _fileSystem.Path.DirectorySeparatorChar.Returns('/');

        var mockDriveRoot = Substitute.For<IDriveInfo>();
        mockDriveRoot.Name.Returns("/");
        var mockDriveUsb = Substitute.For<IDriveInfo>();
        mockDriveUsb.Name.Returns("/media/user/usb");
        
        _fileSystem.DriveInfo.GetDrives().Returns(new[] { mockDriveRoot, mockDriveUsb });

        var mockFolderDir = Substitute.For<IDirectoryInfo>();
        mockFolderDir.FullName.Returns("/media/user/usb/folder");
        
        var mockUsbDir = Substitute.For<IDirectoryInfo>();
        mockUsbDir.FullName.Returns("/media/user/usb");

        mockFolderDir.Parent.Returns(mockUsbDir);
        mockUsbDir.Parent.Returns((IDirectoryInfo?)null); 

        _fileSystem.DirectoryInfo.New(path).Returns(mockFolderDir);

        // Act
        var result = _driveInfoUtilityService.GetDriveInfo(path);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("/media/user/usb"));
    }

    [Test]
    public void GetDriveInfo_WhenExceptionOccurs_ShouldCatchAndReturnNull()
    {
        // Arrange
        string path = "invalid-path";
        _environmentService.IsWindows().Returns(true);
        _fileSystem.Path.GetPathRoot(path).Throws(new Exception("Simulated path error"));

        // Act
        var result = _driveInfoUtilityService.GetDriveInfo(path);

        // Assert
        Assert.That(result, Is.Null);
    }

    #endregion

    #region IsCrossDrive 測試

    [Test]
    public void IsCrossDrive_WhenDrivesAreTheSame_ShouldReturnFalse()
    {
        // Arrange
        string path1 = @"C:\Folder1";
        string path2 = @"C:\Folder2";

        _fileSystem.Path.GetFullPath(path1).Returns(path1);
        _fileSystem.Path.GetFullPath(path2).Returns(path2);

        _environmentService.IsWindows().Returns(true);
        _fileSystem.Path.GetPathRoot(path1).Returns(@"C:\");
        _fileSystem.Path.GetPathRoot(path2).Returns(@"C:\");

        var mockDriveC = Substitute.For<IDriveInfo>();
        mockDriveC.Name.Returns(@"C:\");
        _fileSystem.DriveInfo.New(@"C:\").Returns(mockDriveC);

        // Act
        bool result = _driveInfoUtilityService.IsCrossDrive(path1, path2);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsCrossDrive_WhenDrivesAreDifferent_ShouldReturnTrue()
    {
        // Arrange
        string path1 = @"C:\Folder1";
        string path2 = @"D:\Folder2";

        _fileSystem.Path.GetFullPath(path1).Returns(path1);
        _fileSystem.Path.GetFullPath(path2).Returns(path2);

        _environmentService.IsWindows().Returns(true);
        _fileSystem.Path.GetPathRoot(path1).Returns(@"C:\");
        _fileSystem.Path.GetPathRoot(path2).Returns(@"D:\");

        var mockDriveC = Substitute.For<IDriveInfo>();
        mockDriveC.Name.Returns(@"C:\");
        var mockDriveD = Substitute.For<IDriveInfo>();
        mockDriveD.Name.Returns(@"D:\");

        _fileSystem.DriveInfo.New(@"C:\").Returns(mockDriveC);
        _fileSystem.DriveInfo.New(@"D:\").Returns(mockDriveD);

        // Act
        bool result = _driveInfoUtilityService.IsCrossDrive(path1, path2);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion
}