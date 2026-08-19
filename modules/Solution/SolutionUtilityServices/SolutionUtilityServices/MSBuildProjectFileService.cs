using System.Collections.Concurrent;
using Microsoft.Build.Evaluation;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Polly;
using Polly.Retry;

namespace SolutionUtilityServices
{
    public class MSBuildProjectFileService : IProjectFileService
    {
        private readonly ConcurrentDictionary<string, Task<string>> _versionCache = new();
        private readonly INugetService _nugetService;

        private readonly ResiliencePipeline<string> _resiliencePipeline;
        public MSBuildProjectFileService(
            INugetService nugetService = null
        )
        {
            _nugetService = nugetService ?? new NugetService();

            // 初始化 Polly v8 策略 (結合重試與併發限制)
            _resiliencePipeline = new ResiliencePipelineBuilder<string>()
                .AddRetry(new RetryStrategyOptions<string>
                {
                    ShouldHandle = new PredicateBuilder<string>().Handle<Exception>(),
                    BackoffType = DelayBackoffType.Exponential,
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1)
                })
                .AddConcurrencyLimiter(5, 10) // 限制最大併發請求數
                .Build();
        }

        /// <summary>
        /// Set version of target framework of the project <paramref name="filePath"/> to <paramref name="targetFramework"/>
        /// </summary>
        /// <param name="filePath">project file path</param>
        /// <param name="targetFramework">version of target framework will be updated to</param>
        public void SetTargetFramework(
            string filePath, 
            string targetFramework
        )
        {
            var project = new Project(filePath);
            project.SetProperty("TargetFramework", targetFramework);
            project.Save();
            ProjectCollection.GlobalProjectCollection.UnloadProject(project);
        }

        /// <summary>
        /// Get package reference of the project <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath">project file path</param>
        /// <returns></returns>
        public IEnumerable<PackageReference> GetPackageReferences(string filePath)
        {
            var project = new Project(filePath);
            var items = project.GetItems("PackageReference")
                .Select(i => new PackageReference(i.EvaluatedInclude, i.GetMetadataValue("Version")))
                .ToList();
            
            ProjectCollection.GlobalProjectCollection.UnloadProject(project);
            return items;
        }

        /// <summary>
        /// Update package named <paramref name="packageName"/> of the project <paramref name="filePath"/> to the version <paramref name="newVersion"/>
        /// </summary>
        /// <param name="filePath">project file path</param>
        /// <param name="packageName">package name</param>
        /// <param name="newVersion">version that <paramref name="packageName"/> will be updated to</param>
        public void UpdatePackageVersion(
            string filePath, 
            string packageName, 
            string newVersion
        )
        {
            var project = new Project(filePath);
            var package = project.GetItems("PackageReference")
                .FirstOrDefault(i => i.EvaluatedInclude == packageName);

            if (package != null)
            {
                package.SetMetadataValue("Version", newVersion);
                project.Save();
            }
            ProjectCollection.GlobalProjectCollection.UnloadProject(project);
        }

        /// <summary>
        /// Update all packages of of the project <paramref name="filePath"/>
        /// For each record of <paramref name="packageUpdates"/>, 
        /// packages name is the `Name` property of the record
        /// it corresponding version is the `Version` property of the record
        /// </summary>
        /// <param name="filePath">project file path</param>
        /// <param name="packageUpdates">a <see cref="global::System.Collections.Generic"/> of <see cref="global::SolutionUtilityServices.PackageReference"/> of package names and package version that will be updated to</param>
        public void UpdatePackageVersions(
            string filePath, 
            IEnumerable<PackageReference> packageUpdates
        )
        {
            if (packageUpdates == null || !packageUpdates.Any()) 
            {
                return;
            }

            // 將傳入的更新清單轉為 Dictionary，方便在迴圈中快速查找 (O(1))
            var updateLookup = packageUpdates.ToDictionary(p => p.Name, p => p.Version, StringComparer.OrdinalIgnoreCase);

            // 1. 載入專案 (僅一次)
            var project = new Project(filePath);
            bool isModified = false;

            try
            {
                // 2. 取得所有 PackageReference 項
                var packageItems = project.GetItems("PackageReference").ToList();

                foreach (var item in packageItems)
                {
                    var name = item.EvaluatedInclude;
                    if (updateLookup.TryGetValue(name, out var newVersion))
                    {
                        item.SetMetadataValue("Version", newVersion);
                        isModified = true;
                    }
                }

                // 3. 如果有變動，僅儲存一次
                if (isModified)
                {
                    project.Save();
                }
            }
            finally
            {
                // 4. 確保專案被卸載以釋放資源
                ProjectCollection.GlobalProjectCollection.UnloadProject(project);
            }
        }

        /// <summary>
        /// Get the latest version of <paramref name="currentPackages"/>
        /// </summary>
        /// <param name="currentPackages">packages</param>
        /// <returns></returns>
        public async Task<IEnumerable<PackageReference>> GetLatestPackageUpdatesAsync(IEnumerable<PackageReference> currentPackages)
        {
            // 將 currentPackages 轉為清單以避免重複列舉
            var packageList = currentPackages.ToList();
            
            // 1. 建立所有版本的抓取任務 (併發執行)
            var tasks = packageList.Select(async pkg =>
            {
                string latestVersion = await _versionCache.GetOrAdd(pkg.Name, _ => FetchWithResilienceAsync(pkg.Name));
                
                // 檢查是否需要更新，回傳 PackageReference 或 null
                return pkg.Version != latestVersion 
                    ? new PackageReference(pkg.Name, latestVersion) 
                    : null;
            });

            // 2. 同時等待所有任務完成
            var results = await Task.WhenAll(tasks);

            // 3. 過濾掉不需要更新 (null) 的項目並回傳
            return results.Where(p => p != null)!;
        }

        private async Task<string> FetchWithResilienceAsync(string packageName)
        {
            // 使用 Polly 策略執行 NuGet 抓取
            return await _resiliencePipeline.ExecuteAsync(async token => 
                await _nugetService.GetLatestStableVersionAsync(packageName, token));
        }
    }
}