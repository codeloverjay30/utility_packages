using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AssemblyUtilityServices
{
    public interface IAssembliesUtilityService
    {
        IEnumerable<string> ListAllAssemblies();
        List<Assembly> LoadAllAssemblies(IEnumerable<string> dllFiles);
    }
}
