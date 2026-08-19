using CommonModels;
using ExceptionFactories;
using System.Diagnostics;
using System.Diagnostics.Abstractions;
using System.IO.Abstractions;
using System.Text;
using WindowsAppUtilityServices;
using WindowsAppUtilityServices.Diagnostics;

namespace WindowsAppUtilityServices;

public class WindowsAppMover : IWindowsAppMover
{
    private readonly IProcessUtilityService _processUtilityService;
    private readonly IFileSystem _fileSystem;
    private readonly ICommandRunner _commandRunner;
    public WindowsAppMover(
        IFileSystem fileSystem,
        IProcessUtilityService processUtilityService,
        ICommandRunner commandRunner
    )
    {
        ArgumentNullException.ThrowIfNull(processUtilityService, nameof(processUtilityService));
        ArgumentNullException.ThrowIfNull(commandRunner, nameof(commandRunner));

        _fileSystem = fileSystem ?? new FileSystem();
        _processUtilityService = processUtilityService;
        _commandRunner = commandRunner;
    }


    /// <inheritdoc cref="global::WindowsAppUtilityServices.IWindowsAppMover.MoveOneApp(AppSettings, MoveDirectoryOptions)"/>
    public StatusJsonModel MoveOneApp(
        AppSettings appSetting,
        MoveDirectoryOptions options = MoveDirectoryOptions.Default
    )
    {
        var statusJsonModel = new StatusJsonModel();
        var stringBuilder = new StringBuilder();
        try
        {
            _processUtilityService.SafeKillAndExit(appSetting.ProcessName);

            if (!_fileSystem.Directory.Exists(appSetting.SourcePath))
            {
                stringBuilder.AppendLine($"Can't find the process {appSetting.ProcessName}. Please check the version or the process");
                statusJsonModel.IsSuccess = false;
                statusJsonModel.Result = stringBuilder.ToString();

                return statusJsonModel;
            }

            var targetParentDirectory = _fileSystem.Path.GetDirectoryName(appSetting.TargetPath);
            if (!_fileSystem.Directory.Exists(targetParentDirectory))
            {
                _fileSystem.Directory.CreateDirectory(targetParentDirectory);
            }

            stringBuilder.AppendLine($"Moving to: {appSetting.TargetPath}...");

            // 這裡建議使用 Xcopy 或 Robocopy 保持權限完整性
            _commandRunner.ExecuteCommand($"robocopy \"{appSetting.SourcePath}\" \"{appSetting.TargetPath}\" /E /Z /MT:16 /R:1 /W:1");

            // 4. 重新命名原始資料夾 (備份) 並建立 Junction
            string backupPath = appSetting.SourcePath + "_Backup";
            List<MoveDirectoryOptions> normalOptions = new List<MoveDirectoryOptions>() { MoveDirectoryOptions.Default, MoveDirectoryOptions.Normal };
            if (normalOptions.Contains(options))
            {
                _fileSystem.Directory.Move(appSetting.SourcePath, backupPath);
            }
            else
            {
                string folderName = _fileSystem.Path.GetFileName(appSetting.SourcePath);
                string backupName = folderName + "_Backup";
                _commandRunner.ExecuteCommand($"ren \"{appSetting.SourcePath}\" \"{backupName}\"");
            }
            // 使用 mklink 建立目錄連接 (Junction)
            // 指令格式: mklink /J "原始路徑" "目標路徑"
            _commandRunner.ExecuteCommand($"mklink /J \"{appSetting.SourcePath}\" \"{appSetting.TargetPath}\"");

            stringBuilder.AppendLine($"Finish moving to{appSetting.ProcessName}");
            stringBuilder.AppendLine($"If it works correctly, you can delete`{backupPath}` directory");

            return statusJsonModel;
        }
        catch (Exception ex)
        {
            statusJsonModel.IsSuccess = true;
            statusJsonModel.OverallErrorMessage = ex.Message;
            statusJsonModel.ErrorMessage = $"exception ocurred{ex.Message}!!!";
            statusJsonModel.DetailedErrorMessage = new ExceptionFactory(ex).Create();

            return statusJsonModel;
        }
    }
}
    
