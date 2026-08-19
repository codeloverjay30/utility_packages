using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using WorkspaceUtility.Core.Services;
using DriveInfoUtilityServices;
using CliUtilityServices;
using EnvironmentUtilityServices;
using SymbolicLinkUtilityServices;
using CliUtilityServices.Terminals;
using System.IO.Abstractions.TestingHelpers;
using VscUtilityServices.Tests;

namespace WorkspaceUtility.Tests;

/// <summary>
/// Comprehensive defensive unit tests for <see cref="TaskDispatcher"/> profiling ecosystem security and boundary limits.
/// </summary>
public class TaskDispatcherTests_Old1
{
    private MockFileSystem _mockFileSystem;
    private Mock<IDriveInfoUtilityService>? _mockDriveInfoService;
    private Mock<ISymbolicLinkUtilityService>? _mockSymLinkService;
    private TaskDispatcher? _sut;

    public TaskDispatcherTests_Old1()
    {
        _mockFileSystem = new MockFileSystem();
    }

    private void Setup()
    {
        _mockDriveInfoService ??= new Mock<IDriveInfoUtilityService>(MockBehavior.Strict);
        _mockSymLinkService ??= new Mock<ISymbolicLinkUtilityService>(MockBehavior.Strict);

        _sut = new TaskDispatcher(
            _mockFileSystem,
            new Mock<ICommandLineRunner>().Object,
            _mockDriveInfoService.Object,
            _mockSymLinkService.Object,
            new Mock<IPlatformService>().Object,
            new Mock<IEnvironmentService>().Object,
            new Mock<ITerminalProvider>().Object
        );
    }

    [Fact]
    public async Task ExecuteTaskDefensivelyAsync_ShouldThrowNotSupportedException_WhenLanguageIsUnknown()
    {
        // Arrange
        var targetPath = @"C:\TestWorkspace\cobol1.cobol";
        _mockFileSystem = new MockFileSystem();

        _mockDriveInfoService = new Mock<IDriveInfoUtilityService>(MockBehavior.Strict);
        _mockDriveInfoService.Setup(d => d.IsDriveReadyAndAccessible(It.IsAny<string>())).Returns(true);
        _mockDriveInfoService.Setup(d => d.IsCrossDrive(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        _mockSymLinkService = new Mock<ISymbolicLinkUtilityService>(MockBehavior.Strict);
        _mockSymLinkService.Setup(s => s.IsCyclicReparsePoint(It.IsAny<string>())).Returns(false);

        string unknownLang = "cobol";
        _mockFileSystem.AddFile(targetPath, new MockFileData("Cobol1"));

        Setup();

        // Act
        Func<Task> act = async () => await _sut!.ExecuteTaskDefensivelyAsync(
            targetPath,
            "Test",
            unknownLang,
            "1.0"
        );

        // Assert: 修正萬用字元樣式以精準匹配 SUT 拋出的核心防禦訊息
        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage($"*'{unknownLang}' is explicitly unrecognized*");
    }

    [Fact]
    public async Task ExecuteTaskDefensivelyAsync_ShouldThrowInvalidOperationException_WhenReparsePointIsCyclic()
    {
        // Arrange
        var targetPath = @"C:\TestWorkspace\cobol1.cobol";
        _mockFileSystem = new MockFileSystem();

        _mockDriveInfoService = new Mock<IDriveInfoUtilityService>(MockBehavior.Strict);
        _mockDriveInfoService.Setup(d => d.IsDriveReadyAndAccessible(It.IsAny<string>())).Returns(true);
        _mockDriveInfoService.Setup(d => d.IsCrossDrive(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        _mockSymLinkService = new Mock<ISymbolicLinkUtilityService>(MockBehavior.Strict);
        _mockSymLinkService.Setup(s => s.IsCyclicReparsePoint(It.IsAny<string>())).Returns(true);

        string unknownLang = "cobol";
        _mockFileSystem.AddFile(targetPath, new MockFileData("Cobol1"));

        Setup();

        // Act
        Func<Task> act = async () => await _sut!.ExecuteTaskDefensivelyAsync(
            targetPath,
            "Test",
            unknownLang,
            "1.0"
        );

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*StackOverflowException*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileSystemIsNull()
    {
        // Act
        Action act = () => new TaskDispatcher(
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("fileSystem");
    }

    [Fact]
    public async Task ExecuteTaskDefensivelyAsync_ShouldThrowFileNotFoundException_WhenModernDotNetHasNoDlls()
    {
        // Arrange
        var targetPath = @"C:\ModernWorkspace";
        _mockFileSystem = new MockFileSystem();

        // 主動建立空目錄，不存放任何 .dll 檔案
        _mockFileSystem.AddDirectory(targetPath);

        _mockDriveInfoService = new Mock<IDriveInfoUtilityService>(MockBehavior.Strict);
        _mockDriveInfoService.Setup(d => d.IsDriveReadyAndAccessible(targetPath)).Returns(true);

        _mockSymLinkService = new Mock<ISymbolicLinkUtilityService>(MockBehavior.Strict);
        _mockSymLinkService.Setup(s => s.IsCyclicReparsePoint(targetPath)).Returns(false);

        Setup();

        // Act
        Func<Task> act = async () => await _sut!.ExecuteTaskDefensivelyAsync(
            targetPath,
            "FirstMethodThatWillInvokedOnceTheTargetIsTriggered",
            "csharp",
            "10.0"
        );

        // Assert: 遵循 FluentAssertions 鐵律驗證精準訊息
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*No compiled .NET assemblies found under*");
    }

[Fact]
    public async Task ExecuteTaskDefensivelyAsync_ShouldThrowMissingMethodException_WhenDllExistsButMethodIsMissing()
    {
        // Arrange
        var mockCommonDll = new MockCommonDll();
        var dllPath = mockCommonDll.ExecutingAssemblyPath;
        
        // 透過分部類別(partial class)擴充獲得 100% 安全無損的真實 IL 二進位流
        var dllContentBytes = mockCommonDll.ExecutingAssemblyContentBytes;
        
        _mockFileSystem = new MockFileSystem();

        // 模擬工作區記憶體中存在合法的組譯檔案結構，但內部絕不具備目標自訂方法
        _mockFileSystem.AddFile(dllPath, new MockFileData(dllContentBytes));

        _mockDriveInfoService = new Mock<IDriveInfoUtilityService>(MockBehavior.Strict);
        _mockDriveInfoService.Setup(d => d.IsDriveReadyAndAccessible(It.IsAny<string>())).Returns(true);

        _mockSymLinkService = new Mock<ISymbolicLinkUtilityService>(MockBehavior.Strict);
        _mockSymLinkService.Setup(s => s.IsCyclicReparsePoint(It.IsAny<string>())).Returns(false);

        Setup();

        // Act
        string nonExistentMethod = "InvalidMethodNameThatDoesNotExist";
        Func<Task> act = async () => await _sut!.ExecuteTaskDefensivelyAsync(
            _mockFileSystem.Path.GetDirectoryName(dllPath)!,
            nonExistentMethod,
            "csharp",
            "10.0"
        );

        // Assert: 遵循最高優先級鐵律，使用 FluentAssertions 攔截精準異常訊息
        await act.Should().ThrowAsync<MissingMethodException>()
            .WithMessage($"*Target lifecycle method '{nonExistentMethod}' could not be resolved*");
    }
}
    