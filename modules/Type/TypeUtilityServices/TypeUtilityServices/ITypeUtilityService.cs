using System;
using System.Collections.Generic;
using System.Text;

namespace TypeUtilityServices
{
    public interface ITypeUtilityService
    {
        bool IsNumericType(Type type);

        bool IsNullableType(Type type);

#if NETCOREAPP3_0_OR_GREATER
        T? SafeConvert<T>(object? value);
        TOut? SafeConvert<TOut>(object? value , Type targetType);
        object? SafeConvert(object? value , Type targetType);


        T? SafeConvertQuickly<T>(object? value);

        TOut? SafeConvertQuickly<TOut>(object? value , Type targetType);
        object? SafeConvertQuickly(object? value , Type targetType);
#endif
    }
}
