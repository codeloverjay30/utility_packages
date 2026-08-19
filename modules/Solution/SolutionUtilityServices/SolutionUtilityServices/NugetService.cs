using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System.Collections.Concurrent;

namespace SolutionUtilityServices
{
    
    public class NugetService : INugetService
    {
        private readonly SourceCacheContext _cacheContext = new SourceCacheContext();
        private readonly SourceRepository _repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

        /// <summary>
        /// Get the latest version of package named <paramref name="packageName"/>
        /// </summary>
        /// <param name="packageName">package name</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="Exception">Throws when can't find the latest version of <paramref name="packageName"/></exception>

        public async Task<string> GetLatestStableVersionAsync(
            string packageName, 
            CancellationToken token
        )
        {
            var resource = await _repository.GetResourceAsync<FindPackageByIdResource>(token);
            var versions = await resource.GetAllVersionsAsync(packageName, _cacheContext, NullLogger.Instance, token);
            
            var latestStable = versions?.Where(v => !v.IsPrerelease).Max();
            
            if (latestStable == null) 
            {
                throw new Exception($"Package {packageName} not found.");
            }
            return latestStable.ToNormalizedString();
        }
    }
}