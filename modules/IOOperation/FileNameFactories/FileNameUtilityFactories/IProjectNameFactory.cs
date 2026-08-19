using AssemblyUtilityServices;
using System.Reflection;

namespace FileNameUtilityFactories
{
    public interface IProjectNameFactory
    {
        Assembly? Assembly { get; }
        
        (string,string) Create();
    }
}
