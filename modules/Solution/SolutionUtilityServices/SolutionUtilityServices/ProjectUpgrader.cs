using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using CommonModels;
using System.Text;
using System.Reflection;
using ExceptionFactories;
using System.IO.Abstractions;
using CustomDataAnnotations.Maintenance;

namespace SolutionUtilityServices
{
    /// <inheritdoc cref="global::SolutionUtilityServices.CSharpProjectUpdater"/>
    /// <remarks>
    /// It executes .NET SDK CLI command and matches the pattern using regex which is a little bit faster 
    /// but is not stable and not more maintenable than <see cref="global::SolutionUtilityServices.CSharpProjectUpdater"/> 
    /// as the format of output may be changed in the future release of .NET SDK CLI.
    /// Consider to use <see cref="global::SolutionUtilityServices.CSharpProjectUpdater"/> if the performance does not matter for you.
    /// </remarks>
    [Obsolete("""
    It executes .NET SDK CLI command and matches the pattern using regex which is a little bit faster 
    but is not stable and not more maintenable than <see cref="global::SolutionUtilityServices.CSharpProjectUpdater"/> 
    as the format of output may be changed in the future release of .NET SDK CLI.
    Consider to use <see cref="global::SolutionUtilityServices.CSharpProjectUpdater"/> if the performance does not matter for you.
    """)]
    [TechnicalDebt(CategoryType.CodeSmell | CategoryType.InstableBehaviorIssue,"global::SolutionUtilityServices.CSharpProjectUpdater")]
    public class ProjectUpgrader : IProjectUpgrader
    {
        private readonly string _solutionPath;
        private static readonly INetSdkInfo _defaultNetSdkInfo = new DefaultNetSdkInfo();
        private readonly INetSdkInfo _netSdkInfo;
        private static readonly IFileSystem _defaultFileSystem = new FileSystem();
        private readonly IFileSystem _fileSystem;
        private static readonly ICommandRunner _defaultCommandRunner = new CommandRunner();
        private readonly ICommandRunner _commandRunner;

        /// <param name="solutionPath">The root directory of the target solution.</param>
        public ProjectUpgrader(
            string solutionPath ,
            INetSdkInfo netSdkInfo = null,
            IFileSystem fileSystem = null,
            ICommandRunner commandRunner = null
        )
        {
            _solutionPath = solutionPath;
            _netSdkInfo = netSdkInfo ?? _defaultNetSdkInfo;
            _fileSystem = fileSystem ?? _defaultFileSystem;
            _commandRunner = commandRunner ?? _defaultCommandRunner;
        }

        /// <inheritdoc cref="global::SolutionUtilityServices.CSharpProjectUpdater.FullUpgrade()"/>
        public StatusJsonModels FullUpgrade()
        {
            System.Diagnostics.Debugger.Break();
            var statusJsonModels = new StatusJsonModels();
            var statusJsonModel = new StatusJsonModel();
            // 1. Upgrade TargetFramework in .csproj
            string latestVersion = GetLatestSdkVersion();
            string targetFramework = $"net{latestVersion}";

            statusJsonModel.IsSuccess = true;
            statusJsonModel.CategoryName = "Upgrade";
            statusJsonModel.Metadata.TryAdd("CurrentStep" , "1");
            statusJsonModel.Metadata.TryAdd(".NET SDK" , $"{targetFramework}");
            statusJsonModel.Description = $"[Step 1] Upgrading TargetFramework to: {targetFramework}";
            statusJsonModels.StatusList.Add( statusJsonModel );

            var csprojFiles = _fileSystem.Directory.GetFiles(_solutionPath , "*.csproj" , SearchOption.AllDirectories);
            foreach(var file in csprojFiles)
            {
                var status = UpgradeFrameworkInFile(file , targetFramework);
                statusJsonModels.StatusList.Add(status);
            }

            statusJsonModel = new StatusJsonModel();
            statusJsonModel.IsSuccess = true;
            statusJsonModel.CategoryName = "Upgrade";
            statusJsonModel.Metadata.TryAdd("CurrentStep" , "2");
            statusJsonModel.Metadata.TryAdd("NuGet packages" , $"latest");
            statusJsonModel.Description = "[Step 2] Upgrading NuGet packages to latest versions...";
            statusJsonModels.StatusList.Add(statusJsonModel);

            statusJsonModels.StatusList.Add(UpgradeNuGetPackages());

            statusJsonModel = new StatusJsonModel();
            statusJsonModel.IsSuccess = true;
            statusJsonModel.CategoryName = "Restore";
            statusJsonModel.Metadata.TryAdd("CurrentStep" , "3");
            statusJsonModel.Metadata.TryAdd("NuGet packages" , $"latest");
            statusJsonModel.Description = "[Step 3] Finalizing with dotnet restore...";
            statusJsonModels.StatusList.Add(statusJsonModel);
            
            _commandRunner.RunCommand("dotnet" , "restore" , _solutionPath);

            return statusJsonModels;
        }

        /// <inheritdoc cref="global::SolutionUtilityServices.CSharpProjectUpdater.UpgradeNuGetPackagesAsync()"/>
        public StatusJsonModel UpgradeNuGetPackages()
        {
            var statusJsonModel = new StatusJsonModel();
            var stringBuilder = new StringBuilder();

            try
            {
                var csprojFiles = _fileSystem.Directory.GetFiles(_solutionPath , "*.csproj" , SearchOption.AllDirectories);

                foreach(var csproj in csprojFiles)
                {
                    stringBuilder.AppendLine($"[NuGet] Checking updates for: {_fileSystem.Path.GetFileName(csproj)}");

                    // Get outdated packages list
                    string output = _commandRunner.RunCommandWithOutput("dotnet" , $"list \"{csproj}\" package --outdated");

                    stringBuilder.AppendLine(output);

                    // Parse package names using Regex
                    /// Typical lines:
                    /// [net10.0]:
                    /// Top-level Package      Requested   Resolved   Latest
                    /// > Spectre.Console      0.54.0      0.54.0     0.55.1
                    var pattern = @" >\s+([\w\.]+)\s+([\d\.\w-]+)\s+([\d\.\w-]+)\s+([\d\.\w-]+)";
                    var matches = Regex.Matches(output , pattern);

                    foreach(Match match in matches)
                    {
                        string packageName = match.Groups [ 1 ].Value;
                        stringBuilder.AppendLine($"[NuGet] Updating {packageName}...");
                        _commandRunner.RunCommand("dotnet" , $"add \"{csproj}\" package {packageName}" , _solutionPath);
                    }
                }
                statusJsonModel.Result = stringBuilder.ToString();
                statusJsonModel.IsSuccess = true;
                statusJsonModel.ErrorMessage = string.Empty;
                return statusJsonModel;
            }
            catch(Exception ex)
            {
                statusJsonModel.IsSuccess = false;
                statusJsonModel.Result = "Error";
                statusJsonModel.OverallErrorMessage = "Unknwon error occurred";
                statusJsonModel.ErrorMessage = ex.Message;
                statusJsonModel.DetailedErrorMessage = new ExceptionFactory(ex).Create();
                return statusJsonModel;
            }
        }

        /// <inheritdoc cref="global::SolutionUtilityServices.CSharpProjectUpdater.GetLatestSdkVersion()"/>
        public string GetLatestSdkVersion()
        {
            var output = _commandRunner.RunCommandWithOutput("dotnet" , "--list-sdks");
            var match = Regex.Matches(output , @"(\d+\.\d+)")
                             .Cast<Match>()
                             .Select(m => m.Value)
                             .OrderByDescending(v => v)
                             .FirstOrDefault();

            return match ?? _netSdkInfo.DefaultLatestVersion;
        }

        /// <inheritdoc cref="global::SolutionUtilityServices.CSharpProjectUpdater.UpgradeFrameworkInFile(string, string)"/>
        public StatusJsonModel UpgradeFrameworkInFile(
            string filePath ,
            string targetFramework
        )
        {
            var statusJsonModel = new StatusJsonModel();
            statusJsonModel.Metadata.TryAdd(".NET SDK" , targetFramework);
            statusJsonModel.Name = MethodBase.GetCurrentMethod().Name;
            statusJsonModel.CategoryName = "Upgrade";
            statusJsonModel.Description = "Upgrade .NET version of project";

            try
            {
                string content = _fileSystem.File.ReadAllText(filePath);
                string pattern = @"<TargetFramework>(.*?)</TargetFramework>";
                string updatedContent = Regex.Replace(content , pattern , $"<TargetFramework>{targetFramework}</TargetFramework>");

                if(content != updatedContent)
                {
                    _fileSystem.File.WriteAllText(filePath , updatedContent);
                    statusJsonModel.Result = $"[Updated] Framework -> {targetFramework} in {_fileSystem.Path.GetFileName(filePath)}";
                }
                statusJsonModel.IsSuccess = true;
                statusJsonModel.ErrorMessage = string.Empty;
                return statusJsonModel;
            }
            catch(InvalidOperationException ex)
            {
                statusJsonModel.IsSuccess = false;
                statusJsonModel.Result = "Error";
                statusJsonModel.OverallErrorMessage = "File not found or can not access file";
                statusJsonModel.ErrorMessage = ex.Message;
                statusJsonModel.DetailedErrorMessage = new ExceptionFactory(ex).Create();
                return statusJsonModel;
            }
            catch(Exception ex)
            {
                statusJsonModel.IsSuccess = false;
                statusJsonModel.Result = "Error";
                statusJsonModel.OverallErrorMessage = "Unknwon error occurred";
                statusJsonModel.ErrorMessage = ex.Message;
                statusJsonModel.DetailedErrorMessage = new ExceptionFactory(ex).Create();
                return statusJsonModel;
            }
        }
    }
}
