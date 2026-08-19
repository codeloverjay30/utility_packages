using System.Diagnostics.Abstractions;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WindowsAppUtilityServices.Diagnostics;

namespace WindowsAppUtilityServices.Demo;

public static class ServiceExtensions
{
    public static void ConfigureServices(
        this HostApplicationBuilder builder
    )
    {
        builder.Services.AddSingleton<IProcessFactory, ProcessFactory>();
        builder.Services.AddSingleton<IProcess, Process>();

        // 3. (選填) 如果 CommandRunner 也需要註冊
        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddSingleton<IProcessUtilityService, WindowsProcessUtilityService>();
        builder.Services.AddTransient<Func<IProcess>>(provider => () => provider.GetRequiredService<IProcess>());
        builder.Services.AddTransient<ICommandRunner, CommandRunner>();

        // 3. 使用處理站註冊 WindowsAppMover
        builder.Services.AddTransient<IWindowsAppMover>(sp => 
        {
            // 從容器中取得已註冊的基礎服務
            var fileSystem = sp.GetRequiredService<IFileSystem>();
            var processService = sp.GetRequiredService<IProcessUtilityService>();
            var commandRunner = sp.GetRequiredService<ICommandRunner>();

            // 手動注入建構子參數與屬性初始化
            return new WindowsAppMover(fileSystem,processService, commandRunner);
        });

        // 4. 使用處理站註冊 WindowsAppsMover
        builder.Services.AddTransient<IWindowsAppsMover, WindowsAppsMover>();
    }
}