using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AssemblyUtilityServices
{
    public class AssembliesUtilityService(
        string solutionPath,
        string globFilter
    ) : IAssembliesUtilityService
    {
        private readonly string _solutionPath = solutionPath;
        private readonly string _globFilter = globFilter;

        public IEnumerable<string> ListAllAssemblies()
        {
            var dllFiles = Directory.GetFiles(_solutionPath , _globFilter);

            return dllFiles;
        }

        public List<Assembly> LoadAllAssemblies(IEnumerable<string> dllFiles)
        {
            var assemblyList = new List<Assembly>();
            foreach(var dllFile in dllFiles)
            {
                assemblyList.Add(Assembly.Load(dllFile));
            }

            return assemblyList;
        }
    }
}
