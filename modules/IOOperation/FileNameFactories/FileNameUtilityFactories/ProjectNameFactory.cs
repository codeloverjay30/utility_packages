using AssemblyUtilityServices;
using System.Reflection;

namespace FileNameUtilityFactories
{
    public class ProjectNameFactory : IProjectNameFactory
    {
        private Assembly? _assembly = Assembly.GetEntryAssembly();
        public Assembly? Assembly => _assembly;

        public (string,string) Create()
        {
            if(Assembly == null)
            {
                return (string.Empty , string.Empty);
            }
            string shortname = Assembly.GetShortName();
            string version = Assembly.GetInformationalVersion();
            return (shortname,version);
        }
    }
}
