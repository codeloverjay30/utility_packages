namespace SolutionUtilityServices
{
    public interface IProjectFileService
    {
        // 修改 TargetFramework
        void SetTargetFramework(string filePath, string targetFramework);
        
        // 獲取所有 PackageReference 的套件名稱與版本
        IEnumerable<PackageReference> GetPackageReferences(string filePath);
        
        // 更新特定套件的版本
        void UpdatePackageVersion(string filePath, string packageName, string newVersion);

        void UpdatePackageVersions(string filePath, IEnumerable<PackageReference> packageUpdates);
        Task<IEnumerable<PackageReference>> GetLatestPackageUpdatesAsync(IEnumerable<PackageReference> currentPackages);        
    }

    public record PackageReference(string Name, string Version);
}