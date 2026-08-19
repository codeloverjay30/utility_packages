using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ReflectionUtilityServices
{
    public interface IReflectionUtilityService
    {
        List<Func<object , object? [ ]? , object?>?> FastDelegates { get; }
        Func<object , object? [ ]? , object?>? FastInvoke { get; }
        void AddFastDelegates(IEnumerable<MethodInfo> methodInfos);
        void AddFastDelegate(MethodInfo methodInfo);

        object? [ ] BindArguments(
            ParameterInfo [ ] methodParams ,
            Dictionary<string , JsonElement> arguments
        );
    }
}
