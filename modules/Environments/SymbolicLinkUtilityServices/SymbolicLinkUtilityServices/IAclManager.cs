using System.Security.AccessControl;

namespace SymbolicLinkUtilityServices;

/// <summary>
/// Abstraction for ACL management to allow mocking in unit tests.
/// </summary>
public interface IAclManager
{
    CommonObjectSecurity GetAccessControl(string path, AccessControlSections includeSections);
    void SetAccessControl(string path, CommonObjectSecurity fileSecurity);
}
    