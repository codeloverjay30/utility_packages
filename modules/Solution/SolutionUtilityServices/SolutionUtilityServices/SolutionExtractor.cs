using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO.Abstractions;
namespace SolutionUtilityServices
{
    /// <summary>
    /// A utility class designed to extract projects or clone entire solutions for VS 2026.
    /// Includes namespace refactoring and .slnx generation.
    /// </summary>
    public class SolutionExtractor : ISolutionExtractor
    {
        private readonly SolutionModel _sourceSolution;
        private readonly SolutionModel _targetSolution;

        private static readonly IFileSystem _defaultFileSystem = new FileSystem();
        private readonly IFileSystem _fileSystem;
        private static readonly IFileExtensionChecker _defaultFileExtensionChecker = new FileExtensionChecker(_defaultFileSystem);
        private readonly IFileExtensionChecker _fileExtensionChecker;
        private static readonly IExcludedEntriesUtilityService _defaultExcludedEntriesUtilityService = new ExcludedEntriesUtilityService();
        private readonly IExcludedEntriesUtilityService _excludedEntriesUtilityService;
        public SolutionExtractor(
            SolutionModel sourceSolution, 
            SolutionModel targetSolution, 
            IFileSystem fileSystem = null,
            IFileExtensionChecker fileExtensionChecker = null,
            IExcludedEntriesUtilityService excludedEntriesUtilityService = null
        )
        {
            ArgumentNullException.ThrowIfNull(sourceSolution);
            ArgumentNullException.ThrowIfNull(targetSolution);

            _sourceSolution = sourceSolution;
            _targetSolution = targetSolution;

            _fileSystem = fileSystem ?? _defaultFileSystem;
            _fileExtensionChecker = fileExtensionChecker ?? _defaultFileExtensionChecker;
            _excludedEntriesUtilityService = excludedEntriesUtilityService ?? _defaultExcludedEntriesUtilityService;
        }

        /// <summary>
        /// [Requirement 1] Extracts only specific projects to the target directory.
        /// </summary>
        /// <param name="projectNames">List of project folder names to extract.</param>
        public void ExtractSpecificProjects(List<ProjectModel> sourceProjects)
        {
            InitializeTargetSolution();

            var targetProjects = new List<ProjectModel>();
            for(int i=0;i<sourceProjects.Count;i++)
            {
                var sourceProject = sourceProjects [i];
                var targetProject = _targetSolution.Projects [i];
                targetProjects.Add(targetProject);
                CopyAndRefactor(_sourceSolution,sourceProject, targetProject);
            }

            GenerateSlnx(_targetSolution, targetProjects);
            SyncEnvironmentFiles();
        }

        /// <summary>
        /// [Requirement 2] Clones the ENTIRE solution (all projects and files) to the target directory.
        /// </summary>
        public void ExtractWholeSolution()
        {
            ExtractSpecificProjects(_sourceSolution.Projects);
        }

        /// <summary>
        /// Initialize targetb solution
        /// </summary>

        private void InitializeTargetSolution()
        {
            var targetSolutionPath = _targetSolution.RootPath;
            if(!_fileSystem.Directory.Exists(targetSolutionPath))
            {
                _fileSystem.Directory.CreateDirectory(targetSolutionPath);
            }
        }

        /// <summary>
        /// Generate solution file named <see cref="SolutionModel"/> of <paramref name="targetSolution"/>
        /// </summary>
        /// <param name="targetSolution">target solution</param>
        /// <param name="targetProjects">target project</param>
        private void GenerateSlnx(
            SolutionModel targetSolution,
            IEnumerable<ProjectModel> targetProjects
        )
        {
            if(!_fileSystem.Directory.Exists(targetSolution.RootPath))
            {
                _fileSystem.Directory.CreateDirectory(targetSolution.RootPath);
            }
            string slnxPath = _fileSystem.Path.Combine(targetSolution.RootPath , $"{targetSolution.SolutionName}.slnx");

            XElement solution = new XElement("Solution");
            foreach(var targetProject in targetProjects)
            {
                var targetSolutionRootPath = targetSolution.RootPath;
                var targetProjectFullName = _fileSystem.Path.Combine(targetSolutionRootPath , targetProject.ProjectName , $"{targetProject.ProjectName}.csproj");
                var relativePath = _fileSystem.Path.GetRelativePath(targetSolutionRootPath , targetProjectFullName);
                XElement projectElement = new XElement("Project");
                projectElement.SetAttributeValue("Path" , relativePath);
                solution.Add(projectElement);
            }

            XDocument doc = new XDocument(new XDeclaration("1.0" , "utf-8" , "yes") , solution);

            // 使用 _fileSystem 開啟串流，以確保用 _fileSystem來存檔
            using(var stream = _fileSystem.File.Create(slnxPath))
            {
                doc.Save(stream);
            }
        }

        /// <summary>
        /// Copy the projects from <paramref name="sourceProject"/> to <paramref name="targetProject"/> under <paramref name="sourceSolution"/> solution
        /// And rename the namespace from <see cref="ProjectModel.RootNamespace"/> of <paramref name="sourceProject"/> 
        /// to <see cref="ProjectModel.RootNamespace"/> of <paramref name="targetProject"s/>
        /// </summary>
        /// <param name="sourceSolution">the solution where contains <paramref name="sourceProject"/></param>
        /// <param name="sourceProject">source project that will be copied from</param>
        /// <param name="targetProject">the target project that will be copied to</param>
        private void CopyAndRefactor(
            SolutionModel sourceSolution,
            ProjectModel sourceProject,
            ProjectModel targetProject
        )
        {
            var allFiles = _fileSystem.Directory.GetFiles(sourceSolution.RootPath , "*.*" , SearchOption.AllDirectories);
            foreach(string file in allFiles)
            {
                if(_excludedEntriesUtilityService.IsExcludedPath(file))
                {
                    continue;
                }

                // 1. 取得檔案相對於來源專案根目錄的路徑
                string relativePath = _fileSystem.Path.GetRelativePath(sourceProject.RootPath , file);

                // 2. 結合目標專案根目錄，算出真正的「目標檔案路徑」
                string destFilePath = _fileSystem.Path.Combine(targetProject.RootPath , relativePath);

                // 3. 確保目標子目錄存在
                string destFolder = _fileSystem.Path.GetDirectoryName(destFilePath);
                if(!_fileSystem.Directory.Exists(destFolder))
                {
                    _fileSystem.Directory.CreateDirectory(destFolder);
                }

                if(_fileExtensionChecker.NeedsToBeReplaced(file))
                {
                    string content = _fileSystem.File.ReadAllText(file);
                    content = content.Replace(sourceProject.RootNamespace , targetProject.RootNamespace);
                    _fileSystem.File.WriteAllText(destFilePath , content , Encoding.UTF8);
                }
                else
                {
                    _fileSystem.File.Copy(file , destFilePath , true);
                }
            }
        }

        /// <summary>
        /// sync the files about configuration that will be all appied under <see cref="SolutionExtractor._targetSolution"/> Solution 
        /// </summary>

        private void SyncEnvironmentFiles()
        {
            CopyFileIfExist("nuget.config");
            CopyFileIfExist("Directory.Build.props");
        }

        private void CopyFileIfExist(string fileName)
        {
            var sourceSolutionRoot = _sourceSolution.RootPath;
            var targetSolutionRoot = _targetSolution.RootPath;
            string src = _fileSystem.Path.Combine(sourceSolutionRoot , fileName);
            if(_fileSystem.File.Exists(src))
            {
                _fileSystem.File.Copy(src , _fileSystem.Path.Combine(targetSolutionRoot , fileName) , true);
            }
        }
    }
}
