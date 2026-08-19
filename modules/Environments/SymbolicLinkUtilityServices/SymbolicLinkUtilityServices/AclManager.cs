namespace SymbolicLinkUtilityServices;

using System.IO.Abstractions;
    using System.Security.AccessControl;

public class AclManager : IAclManager
{
    private readonly IFileSystem _fileSystem;

    public AclManager(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public CommonObjectSecurity GetAccessControl(string path, AccessControlSections includeSections)
    {
        // 直接呼叫 IFileInfo 的實例方法，而非靜態擴充方法
        // IFileSystemAclSupport 是 System.IO.Abstractions 的核心介面
        var fileInfo = _fileSystem.FileInfo.New(path);
        if (fileInfo is IFileSystemAclSupport aclSupport)
        {
            return (CommonObjectSecurity)aclSupport.GetAccessControl((IFileSystemAclSupport.AccessControlSections)includeSections);
        }
        throw new NotSupportedException("Current file system does not support ACL.");
    }

    public void SetAccessControl(string path, CommonObjectSecurity fileSecurity)
    {
        var fileInfo = _fileSystem.FileInfo.New(path);
        if (fileInfo is IFileSystemAclSupport aclSupport)
        {
            aclSupport.SetAccessControl(fileSecurity);
        }
    }
}
    