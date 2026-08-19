using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AntiHijackUtilityService.Sensors;
using AntiHijackUtilityServices.Abstractions;
using AntiHijackUtilityServices.Core;
using EnvironmentUtilityServices;

namespace AntiHijackUtilityServices;

/// <summary>
/// Execution host for the security evaluation module.
/// </summary>
public static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static void Main()
    {
        try
        {
            IFileSystem fileSystem = new MockFileSystem();
            IEnvironmentService environmentService = new EnvironmentService();
            IOsUtilityService osUtilityService = new OsUtilityService(fileSystem,environmentService);
            IPlatformService platformService = new PlatformService(environmentService, osUtilityService);
            
            // 優化：正式激活雙重執行緒心跳監控器，防止駭客動態修改或使用除錯器中斷掛起線程
            var interlockShield = new InterlockProtection();
            interlockShield.StartProtectiveShield();
            
            var validator = new OSPlatformValidator(platformService);
            var sensors = new List<ISafetySensor>
            {
                new DebuggerDetector(platformService),
                new CpuIdDetector(platformService),
                new VirtualMachineDetector(platformService),
            };

            var coordinator = new AntiHijackCoordinator(validator, sensors);

            Console.WriteLine("[System Context] Beginning runtime environment isolation check...");
            
            bool isEnvironmentSecure = coordinator.VerifyEcosystemHealth();

            if (!isEnvironmentSecure)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ALERT] Compromised environment signature discovered! Terminating process context.");
                Console.ResetColor();
                Environment.Exit(-1);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[SUCCESS] Ecosystem integrity cleared. Environment is certified as safe.");
            Console.ResetColor();
        }
        catch (PlatformNotSupportedException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[CRITICAL] Operating system unsupported: {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[UNHANDLED FAULT] Global protective recovery intercepted: {ex.Message}");
            Console.ResetColor();
        }
    }
}