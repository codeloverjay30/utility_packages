using System;
using TypeUtilityServices;

namespace JsonUtilityServices
{
    public class JsonUtilityService:IJsonUtilityService
    {

        private readonly ITypeUtilityService _typeUtilityService;

        public JsonUtilityService(ITypeUtilityService typeUtilityService)
        {
            _typeUtilityService = typeUtilityService;
        }
        public string GetJsonType(Type type)
        {
            if(_typeUtilityService.IsNumericType(type)) return "number";
            if(type == typeof(bool)) return "boolean";
            if(type == typeof(string)) return "string";
            return "other";
        }
    }
}
