using System.IO.Abstractions;

namespace DriveInfoUtilityServices;

public interface IDriveInfoUtilityService
{
    bool IsCrossDrive(
        string path1,
        string path2
    );

    IDriveInfo? GetDriveInfo(string path);

    bool IsDriveReadyAndAccessible(string targetPath);
}
