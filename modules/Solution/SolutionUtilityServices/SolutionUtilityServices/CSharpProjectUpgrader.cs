using CommonModels;
using ExceptionFactories;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.IO.Abstractions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Locator;

namespace SolutionUtilityServices
{
    /// <summary>
    /// A utility class to upgrade all projects in a solution to the latest available .NET SDK 
    /// and update all installed NuGet packages to their latest versions.
    /// </summary>
    public class CSharpProjectUpdater : ICSharpProjectUpgrader
    {
        private readonly IProjectFileService _projectFileService;
        private readonly string _solutionPath;
        private static readonly INetSdkInfo _defaultNetSdkInfo = new DefaultNetSdkInfo();
        private readonly INetSdkInfo _netSdkInfo;
        private static readonly IFileSystem _defaultFileSystem = new FileSystem();
        private readonly IFileSystem _fileSystem;
        private static readonly ICommandRunner _defaultCommandRunner = new CommandRunner();
        private readonly ICommandRunner _commandRunner;
         /// <param name="solutionPath">The root directory of the target solution.</param>
        public CSharpProjectUpdater(
            string solutionPath ,
            INetSdkInfo netSdkInfo = null,
            IFileSystem fileSystem = null,
            ICommandRunner commandRunner = null,
            IProjectFileService projectFileService = null
        )
        {
            _solutionPath = solutionPath;
            _netSdkInfo = netSdkInfo ?? _defaultNetSdkInfo;
            _fileSystem = fileSystem ?? _defaultFileSystem;
            _commandRunner = commandRunner ?? _defaultCommandRunner;
            _projectFileService = projectFileService ?? new MSBuildProjectFileService();
        }

        /// <summary>
        /// Orchestrates the full upgrade process: SDK detection, Framework upgrade, and NuGet updates.
        /// </summary>
        public async Task<StatusJsonModels> FullUpgradeAsync()
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

            foreach(var file in csprojFiles)
            {
                statusJsonModels.StatusList.Add(await UpgradeNuGetPackagesAsync(file));
            }
            

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

        /// <summary>
        /// Get the latest version of .NET SDK that are installed in the devices at present.
        /// </summary>
        public string GetLatestSdkVersion()
        {
            return _netSdkInfo.GetInstalledLatestVersion();
        }

        /// <summary>
        /// Upgrade `<TargetFramework>` which is appear in `*.csproj`.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="targetFramework"></param>
        /// <returns></returns>

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
                _projectFileService.SetTargetFramework(filePath, targetFramework);

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

        /// <summary>
        /// Upgrade nuget packages in the project which located at <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath">project file path</param>
        /// <returns></returns>

        public async Task<StatusJsonModel> UpgradeNuGetPackagesAsync(
            string filePath
        )
        {
            var statusJsonModel = new StatusJsonModel();
            try{
                if (!_fileSystem.File.Exists(filePath))
                {
                    throw new FileNotFoundException($"The file `{filePath}` does not found");
                }
                var packageReferences = new List<PackageReference>(); 
                packageReferences = (await _projectFileService.GetLatestPackageUpdatesAsync(packageReferences)).ToList();
                _projectFileService.UpdatePackageVersions(filePath,packageReferences);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"Successfully update {packageReferences.Count()} packages");
                foreach(var package in packageReferences)
                {
                    stringBuilder.AppendLine($"package {package.Name} is updated to {package.Version}");
                }
                statusJsonModel.Result = stringBuilder.ToString();
                statusJsonModel.IsSuccess = true;
                return statusJsonModel;
            }
            catch (Exception ex)
            {
                statusJsonModel.IsSuccess = false;
                statusJsonModel.OverallErrorMessage = "NuGet 升級過程中發生錯誤";
                statusJsonModel.ErrorMessage = ex.Message;
                statusJsonModel.DetailedErrorMessage = new ExceptionFactory(ex).Create();
                return statusJsonModel;
            }
        }
    }
}