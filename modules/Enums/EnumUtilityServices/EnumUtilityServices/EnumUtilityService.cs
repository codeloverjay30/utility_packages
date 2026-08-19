using System;
using System.Collections.Generic;
using System.Text;

namespace EnumUtilityServices
{
    public class EnumUtilityService : IEnumUtilityService
    {
        public string [ ] GetEnumNames<T>() => GetEnumNames(typeof(T));
        public string [ ] GetEnumNames(Type type)
        {
            Type? targetType = type.IsEnum ? type : Nullable.GetUnderlyingType(type);

            return (targetType?.IsEnum == true)
                ? Enum.GetNames(targetType)
                : Array.Empty<string>();
        }
    }
}
