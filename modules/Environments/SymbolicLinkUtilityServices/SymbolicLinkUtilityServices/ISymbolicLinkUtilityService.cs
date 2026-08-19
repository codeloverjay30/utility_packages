namespace SymbolicLinkUtilityServices;

public interface ISymbolicLinkUtilityService
{
    bool IsWindows { get; }

    bool IsCyclicReparsePoint(string path);
    void TryToUpdateLink(
        SymbolicLinkOptions options
    );

    void UpdateLink(
        SymbolicLinkOptions options
    );

    void TryToDeleteSymbolicLink(
        bool isDirectory,
        string linkPath
    );
}
