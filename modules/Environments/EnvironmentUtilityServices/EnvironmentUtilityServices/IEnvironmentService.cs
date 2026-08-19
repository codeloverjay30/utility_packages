namespace EnvironmentUtilityServices;

public interface IEnvironmentService
{
    bool IsWindows();

    bool IsLinux();

    bool IsMacOS();

    bool IsUncPath(string path);
}
