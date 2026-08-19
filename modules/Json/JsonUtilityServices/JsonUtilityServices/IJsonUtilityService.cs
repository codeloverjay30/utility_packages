using System;
using System.Collections.Generic;
using System.Text;
using TypeUtilityServices;

namespace JsonUtilityServices
{
    public interface IJsonUtilityService
    {
        string GetJsonType(Type type);
    }
}
