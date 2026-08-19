using SolutionUtilityServices.Runners.Models;
using SolutionUtilityServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Text.Json;
using System.IO.Abstractions;

namespace SolutionUtilityServices.Runners.Services
{
    public class ConfigManager
    {

        private static readonly IFileSystem _defaultFileSystem = new FileSystem();
        private readonly IFileSystem _fileSystem;
        private static readonly INetSdkInfo _defaultNetSdkInfo = new DefaultNetSdkInfo();
        private readonly INetSdkInfo _netSdkInfo;
        private static readonly ICommandRunner _defaultCommandRunner = new CommandRunner();
        private readonly ICommandRunner _commandRunner;
        private static readonly IFileExtensionChecker _defaultFileExtensionChecker = new FileExtensionChecker(_defaultFileSystem);
        private readonly IFileExtensionChecker _fileExtensionChecker;
        private static readonly IExcludedEntriesUtilityService _defaultExcludedEntriesUtilityService = new ExcludedEntriesUtilityService();
        private readonly IExcludedEntriesUtilityService _excludedEntriesUtilityService;

        public ConfigManager(
            IFileSystem fileSystem = null,
            INetSdkInfo netSdkInfo = null ,
            ICommandRunner commandRunner = null,
            IFileExtensionChecker fileExtensionChecker = null,
            IExcludedEntriesUtilityService excludedEntriesUtilityService = null
            
        )
        {
            _fileSystem = fileSystem ?? _defaultFileSystem;
            _netSdkInfo = netSdkInfo ?? _defaultNetSdkInfo;
            _commandRunner = commandRunner ?? _defaultCommandRunner;
            _fileExtensionChecker = fileExtensionChecker ?? _defaultFileExtensionChecker;
            _excludedEntriesUtilityService = excludedEntriesUtilityService ?? _defaultExcludedEntriesUtilityService;
        }
        /// <summary>
        /// 透過視窗選擇 JSON 組態檔並執行對應的抽離與升級邏輯
        /// </summary>
        [STAThread] // 必須標記為 STAThread 才能執行視窗元件
        public void ProcessWithUI()
        {
            using(OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                openFileDialog.Filter = "JSON Config Files (*.json)|*.json";
                openFileDialog.Title = "Select Extraction Configuration File";

                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Execute(openFileDialog.FileName);
                }
            }
        }

        public void Execute(string configPath)
        {
            try
            {
                // 1. 讀取並驗證 JSON
                string jsonString = _fileSystem.File.ReadAllText(configPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var config = JsonSerializer.Deserialize<ExtractorConfig>(jsonString , options);

                Validate(config);

                Console.WriteLine($"[Process] Starting with config: {Path.GetFileName(configPath)}");

                // 2. 初始化 Extractor
                var extractor = new SolutionExtractor(
                    config.SourceSolution ,
                    config.TargetSolution ,
                    _fileSystem,
                    _fileExtensionChecker,
                    _excludedEntriesUtilityService
                );

                // 3. 根據 isExtractWholeSolution 執行不同方法
                // 處理 JSON 格式中可能被雙引號括住的 bool 字串
                bool isWhole = bool.Parse(config.IsExtractWholeSolution.ToString());

                if(isWhole)
                {
                    extractor.ExtractWholeSolution();
                }
                else
                {
                    extractor.ExtractSpecificProjects(config.SourceSolution.Projects);
                }

                // 4. 呼叫 Upgrader 進行技術債清償
                var upgrader = new ProjectUpgrader(
                    config.TargetSolution.RootPath,
                    _netSdkInfo,
                    _fileSystem,
                    _commandRunner
                );
                upgrader.FullUpgrade();

                MessageBox.Show("All tasks completed successfully!" , "Success" , MessageBoxButtons.OK , MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}" , "Configuration Error" , MessageBoxButtons.OK , MessageBoxIcon.Error);
            }
        }

        private void Validate(ExtractorConfig config)
        {
            if(string.IsNullOrEmpty(config.SourceSolution.RootPath)) throw new Exception("Field 'sourceRoot' is required.");
            if(string.IsNullOrEmpty(config.TargetSolution.RootPath)) throw new Exception("Field 'targetRoot' is required.");
            if(string.IsNullOrEmpty(config.TargetSolution.SolutionName)) throw new Exception("Field 'newSlnName' is required.");
            if(string.IsNullOrEmpty(config.SourceSolution?.Projects?.FirstOrDefault()?.RootNamespace)) throw new Exception("Field 'oldNamespace' is required.");

            // 如果不是抽離整個方案，則 要抽離的方案必填
            if(config.IsExtractWholeSolution == false &&
                (config.SourceSolution?.Projects?.Count ?? 0) > 0)
            {
                throw new Exception("Field 'projectsToExtract' cannot be empty when 'isExtractWholeSolution' is false.");
            }
        }
    }
}
