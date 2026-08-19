namespace SolutionUtilityServices
{
    public interface INugetService
    {
        Task<string> GetLatestStableVersionAsync(string packageName, CancellationToken token);
    }
}