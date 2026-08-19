namespace ConsoleUtilityServices
{
    public interface IConsoleService
    {
        bool CanUseConsole();
        void WriteLine(string message); // 擴充：未來可統一管理輸出
    }
}
