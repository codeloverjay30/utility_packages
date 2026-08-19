namespace ConsoleUtilityServices
{
    public class ConsoleService : IConsoleService
    {
        /// <summary>
        /// ensure the running environment can use `Console`.
        /// </summary>
        /// <returns></returns>
        public bool CanUseConsole()
        {
            try
            {
                return !Console.IsOutputRedirected && Console.WindowHeight > 0;
            }
            catch
            {
                return false;
            }
        }

        public void WriteLine(string message) 
        {
            if(CanUseConsole())
            {
                Console.WriteLine(message);
            }
        }
    }
}
