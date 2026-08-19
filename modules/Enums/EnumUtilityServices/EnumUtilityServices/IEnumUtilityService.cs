using System;
using System.Collections.Generic;
using System.Text;

namespace EnumUtilityServices
{
    public interface IEnumUtilityService
    {
        string [ ] GetEnumNames<T>();
        string [ ] GetEnumNames(Type type);
    }
}
