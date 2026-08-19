using System.Reflection;
using System.Text;
using FileNameUtilityFactories;

namespace LogNameUtilityFactories
{
    public partial interface ILogFullPathFactory
    {
        string Create();
    }
}
