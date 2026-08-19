using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using CliUtilityServices;
using CliUtilityServices.Terminals;
using DriveInfoUtilityServices;
using EnvironmentUtilityServices;
using FluentAssertions;
using Moq;
using SymbolicLinkUtilityServices;
using WorkspaceUtility.Core.Services;
using VscUtilityServices.Tests;
using Xunit;

namespace WorkspaceUtility.Tests;

/// <summary>
/// Comprehensive defensive unit tests for <see cref="TaskDispatcher"/> profiling ecosystem security, boundary limits, and assembly sandboxing.
/// </summary>
public class TaskDispatcherTests
{
    private MockFileSystem _mockFileSystem;
    private Mock<ICommandLineRunner> _mockCommandLineRunner;
    private Mock<IDriveInfoUtilityService> _mockDriveInfoService;
    private Mock<ISymbolicLinkUtilityService> _mockSymLinkService;
    private Mock<IPlatformService> _mockPlatformService;
    private Mock<IEnvironmentService> _mockEnvironmentService;
    private Mock<ITerminalProvider> _mockTerminalProvider;
    private TaskDispatcher _sut;

    public TaskDispatcherTests()
    {
        _mockFileSystem = new MockFileSystem();
        _mockCommandLineRunner = new Mock<ICommandLineRunner>(MockBehavior.Strict);
        _mockDriveInfoService = new Mock<IDriveInfoUtilityService>(MockBehavior.Strict);
        _mockSymLinkService = new Mock<ISymbolicLinkUtilityService>(MockBehavior.Strict);
        _mockPlatformService = new Mock<IPlatformService>(MockBehavior.Strict);
        _mockEnvironmentService = new Mock<IEnvironmentService>(MockBehavior.Strict);
        _mockTerminalProvider = new Mock<ITerminalProvider>(MockBehavior.Strict);

        // 預設配置基礎通用 Happy Path 阻斷
        _mockEnvironmentService.Setup(e => e.IsWindows()).Returns(true);
        _mockPlatformService.Setup(p => p.IsWindows()).Returns(true);
        
        _mockDriveInfoService.Setup(d => d.IsDriveReadyAndAccessible(It.IsAny<string>())).Returns(true);
        _mockSymLinkService.Setup(s => s.IsCyclicReparsePoint(It.IsAny<string>())).Returns(false);

        _sut = CreateSut();
    }

    private TaskDispatcher CreateSut()
    {
        return new TaskDispatcher(
            _mockFileSystem,
            _mockCommandLineRunner.Object,
            _mockDriveInfoService.Object,
            _mockSymLinkService.Object,
            _mockPlatformService.Object,
            _mockEnvironmentService.Object,
            _mockTerminalProvider.Object
        );
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileSystemIsNull()
    {
        // Act
        Action act = () => new TaskDispatcher(null!, _mockCommandLineRunner.Object, _mockDriveInfoService.Object, _mockSymLinkService.Object, _mockPlatformService.Object, _mockEnvironmentService.Object, _mockTerminalProvider.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().Where(a => a.ParamName == "fileSystem");
    }

    [Fact]
    public async Task ExecuteTaskDefensivelyAsync_ShouldThrowInvalidOperationException_WhenDriveIsNotAccessible()
    {
        // Arrange
        string targetPath = @"X:\OfflineWorkspace";
        _mockDriveInfoService.Setup(d => d.IsDriveReadyAndAccessible(targetPath)).Returns(false);

        // Act
        Func<Task> act = async () => await _sut.ExecuteTaskDefensivelyAsync(targetPath, "TaskName", "csharp", "10.0");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*is either not ready, disconnected, or unreadable*");
    }

    [Fact]
    public async Task ExecuteTaskDefensivelyAsync_ShouldThrowInvalidOperationException_WhenPathContainsCyclicSymlink()
    {
        // Arrange
        string targetPath = @"C:\CyclicWorkspace";
        _mockSymLinkService.Setup(s => s.IsCyclicReparsePoint(targetPath)).Returns(true);

        // Act
        Func<Task> act = async () => await _sut.ExecuteTaskDefensivelyAsync(targetPath, "TaskName", "csharp", "10.0");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*contains a cyclic symbolic link that may cause a StackOverflowException*");
    }

    [Fact]
    public async Task ExecuteTaskDefensivelyAsync_ShouldThrowNotSupportedException_WhenLanguageIsExplicitlyUnknown()
    {
        // Arrange
        string targetPath = @"C:\Workspace";
        string unknownLang = "cobol";

        // Act
        Func<Task> act = async () => await _sut.ExecuteTaskDefensivelyAsync(targetPath, "TestTask", unknownLang, "1.0");

        // Assert
        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage($"*'{unknownLang}' is explicitly unrecognized*");
    }

    [Fact]
    public async Task ExecuteTaskDefensivelyAsync_ShouldThrowPlatformNotSupportedException_WhenLegacyFrameworkRunsOnNonWindows()
    {
        // Arrange
        string targetPath = @"/usr/share/workspace";
        _mockPlatformService.Setup(p => p.IsWindows()).Returns(false); // 模擬 Linux 環境

        // Act: 試圖呼叫 .NET 框架舊版本 (e.g., 2.0 或非 10.0 的 C#)
        Func<Task> act = async () => await _sut.ExecuteTaskDefensivelyAsync(targetPath, "LegacyTask", "csharp", "4.5");

        // Assert
        await act.Should().ThrowAsync<PlatformNotSupportedException>()
            .WithMessage("*exclusive to Windows platforms*");
    }

    [Fact]
    public async Task ExecuteTaskDefensivelyAsync_ShouldThrowFileNotFoundException_WhenModernDotNetHasNoAssemblies()
    {
        // Arrange
        string targetPath = @"C:\EmptyModernWorkspace";
        _mockFileSystem.AddDirectory(targetPath); // 建立空目錄，完全沒有 .dll

        // Act
        Func<Task> act = async () => await _sut.ExecuteTaskDefensivelyAsync(targetPath, "AnyMethod", "csharp", "10.0");

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*No compiled .NET assemblies found under*");
    }

    [Fact]
    public async Task ExecuteTaskDefensivelyAsync_ShouldThrowMissingMethodException_WhenDllExistsButTargetMethodIsMissing()
    {
        // Arrange
        string targetPath = @"C:\CompiledWorkspace";
        _mockFileSystem.AddDirectory(targetPath);

        // 利用測試專案提供的 MockCommonDll Partial 擴充架構，
        // 提取當前 host 執行的、100% 合法且具備完整 IL 的 PE 二進位結構 bytes，徹底防禦 BadImageFormatException
        var mockDllContainer = new MockCommonDll();
        byte[] validAssemblyBytes = mockDllContainer.ExecutingAssemblyContentBytes;
        
        string fakeDllPath = _mockFileSystem.Path.Combine(targetPath, "ValidInfrastructure.dll");
        _mockFileSystem.AddFile(fakeDllPath, new MockFileData(validAssemblyBytes));

        // Act: 故意尋找一個絕對不存在的方法名稱
        string nonExistentMethod = "CryptoHackInjectedMethodNameThatDoesNotExist";
        Func<Task> act = async () => await _sut.ExecuteTaskDefensivelyAsync(targetPath, nonExistentMethod, "csharp", "10.0");

        // Assert: 系統在遍歷真實 DLL 後，找不到對應 Method 必須安全拋出異常
        await act.Should().ThrowAsync<MissingMethodException>()
            .WithMessage($"*Target lifecycle method '{nonExistentMethod}' could not be resolved*");
    }
}