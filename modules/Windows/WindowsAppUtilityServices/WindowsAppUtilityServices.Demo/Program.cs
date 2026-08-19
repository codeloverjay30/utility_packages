using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WindowsAppUtilityServices;
using WindowsAppUtilityServices.Demo;

// 手動建立`List<AppSettings>`實體
var settings = new List<AppSettings> 
{ 
    new AppSettings 
    { 
        ProcessName = "Package Cache", 
        SourcePath = @"C:\Users", 
        TargetPath = @"D:\Users",
    }, 
};

var builder = Host.CreateApplicationBuilder(args);

// 2. 註冊為單例 (Singleton)，將`List<AppSettings>`實體包裝成 IOptions
builder.Services.AddSingleton(Options.Create(settings));

builder.ConfigureServices();

using var host = builder.Build();
var appsMover = host.Services.GetRequiredService<IWindowsAppsMover>();
appsMover.MoveManyApps();