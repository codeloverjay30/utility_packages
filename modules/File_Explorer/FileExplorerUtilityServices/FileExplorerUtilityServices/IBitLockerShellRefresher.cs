namespace FileExplorerUtilityServices;

public interface IBitLockerShellRefresher
{
    /// <summary>
    /// Notify task bar and File Explorer to refresh the icon resource located in <paramref name="drive"/>
    /// </summary>
    /// <param name="drive">To notify which icons on the drive should be refreshed</param>
    void NotifyToRefresh(ReadOnlySpan<char> drive);
}
