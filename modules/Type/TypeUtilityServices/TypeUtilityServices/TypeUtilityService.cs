using System;
using System.Collections.Generic;
using System.Globalization;

namespace TypeUtilityServices
{
    public class TypeUtilityService: ITypeUtilityService
    {
        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(ushort), typeof(uint), typeof(ulong),
            typeof(byte), typeof(short), typeof(int),
            typeof(long), typeof(float), typeof(double), typeof(decimal)
        };

        public bool IsNumericType(Type type)
        {
            return NumericTypes.Contains(type);
        }

        public bool IsNullableType(Type type)
        {
            if(Nullable.GetUnderlyingType(type) != null)
            {
                return true;
            }

            if(!type.IsValueType)
            {
                return true;
            }
            return false;
        }

#if NETCOREAPP3_0_OR_GREATER
        /// <summary>
        /// Safely convert value from <see cref="object?"/> type to <see cref="T?"/> type. If the conversion fails, it returns default value of <see cref="T?"/> type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// 1. For better performance, consider using <see cref="SafeConvertQuickly{T}(object?)"/> method (if possible).
        /// The requirments are said on the comment of <see cref="SafeConvertQuickly{T}(object?)"/> method.
        ///
        /// 2. For detail implementation, see <see cref="SafeConvert(object?, Type)"/> method.
        /// </remarks>
        public T? SafeConvert<T>(object? value)
        {
            var result = SafeConvert(value , typeof(T));
            return result == null ? default : (T)result;
        }

        public TOut? SafeConvert<TOut>(object? value , Type targetType)
        {
            // 1. 直接呼叫您已經寫好的非泛型優化版本
            object? result = SafeConvert(value , targetType);

            return result == null ? default : (TOut)result;
        }

        /// <summary>
        /// Non-generic method of <see cref="SafeConvert{T}(object?)"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public object? SafeConvert(object? value , Type targetType)
        {
            if(value == null || value == DBNull.Value)
            {
                return default;
            }

            Type actualTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // 優化
            // 如果輸入值已經符合目標型別，直接回傳避免轉換損耗
            if(actualTargetType.IsInstanceOfType(value))
            {
                return value;
            }
            try
            {
                // 處理 Enum
                if(actualTargetType.IsEnum)
                {
                    if(value is string str)
                    {
                        return Enum.Parse(actualTargetType , str);
                    }
                    return Enum.ToObject(actualTargetType , value);
                }

                // 處理 Guid
                if(actualTargetType == typeof(Guid) && value is string guidStr)
                {
                    return (object)Guid.Parse(guidStr);
                }

                // 處理 TimeSpan
                if(actualTargetType == typeof(TimeSpan))
                {
                    if(value is string tsStr)
                    {
                        return (object)TimeSpan.Parse(tsStr);
                    }
                    if(value is double or int or long)
                    {
                        return (object)TimeSpan.FromMilliseconds(Convert.ToDouble(value));
                    }
                }

                // 處理 DateTime
                if(actualTargetType == typeof(DateTime))
                {
                    if(value is string dateStr)
                    {
                        // 優先嘗試 ISO 8601 或標準格式，並允許空白
                        if(DateTime.TryParse(dateStr , System.Globalization.CultureInfo.InvariantCulture , System.Globalization.DateTimeStyles.AllowWhiteSpaces , out var result))
                        {
                            return (object)result;
                        }
                    }
                }

                // 處理布林值 (針對 "1" 和 "0")
                if(actualTargetType == typeof(bool))
                {
                    if(value.ToString() == "1")
                    {
                        return (object)true;
                    }
                    if(value.ToString() == "0")
                    {
                        return (object)false;
                    }
                }

                return Convert.ChangeType(value , actualTargetType);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Better performance version of <see cref="SafeConvert{T}(object?)"/> method. It uses TypeCode for faster type checking and conversion, and includes optimizations for common types like bool, DateTime, Guid, TimeSpan, and Enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// 1. However, this method uses different techniques for type checking and conversion,
        /// so it it may not cover all edge cases that <see cref="SafeConvert{T}"/> can handle,
        /// or have slightly different behaviour for complex type (e.g. convert <seealso cref="DateTime"/> to <seealso cref="string"/> by <seealso cref="DateTime.ToString()"/> method, then convert <seealso cref="string"/> to <seealso cref="DateTime"/> by this method.
        ///
        /// ONLY use this method when you are sure that the input values are in expected formats.
        ///
        /// 2. For detail implementation, see <see cref="SafeConvertQuickly(object?, Type)"/> method.
        /// </remarks>
        public T? SafeConvertQuickly<T>(object? value)
        {
            var result = SafeConvertQuickly(value , typeof(T));
            return result == null ? default : (T)result;
        }

        public TOut? SafeConvertQuickly<TOut>(object? value , Type targetType)
        {
            // 1. 直接呼叫您已經寫好的非泛型優化版本
            object? result = SafeConvertQuickly(value , targetType);

            return result == null ? default : (TOut)result;
        }

        /// <summary>
        /// Non-generic method of <see cref="SafeConvertQuickly{T}(object?)"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public object? SafeConvertQuickly(object? value , Type targetType)
        {
            // 快速判斷空值
            if(value == null || value == DBNull.Value)
            {
                return default;
            }

            Type actualTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // 如果輸入值已經符合目標型別，直接回傳避免轉換損耗
            if(actualTargetType.IsInstanceOfType(value))
            {
                return value;
            }
            try
            {
                // 使用 TypeCode 進行快速分支判斷，減少 typeof 比較次數
                var typeCode = Type.GetTypeCode(actualTargetType);

                switch(typeCode)
                {
                    case TypeCode.Boolean:
                        // 優化：直接判斷數值或字串，避免 ToString() 產生記憶體配置
                        if(value is "1" or 1 or 1L)
                        {
                            return (object)true;
                        }
                        if(value is "0" or 0 or 0L)
                        {
                            return (object)false;
                        }
                        break;

                    case TypeCode.DateTime:
                        if(value is string dateStr)
                        {
                            if(DateTime.TryParse(dateStr , CultureInfo.InvariantCulture , DateTimeStyles.AllowWhiteSpaces , out var dt))
                                return (object)dt;
                        }
                        break;

                    case TypeCode.Object:
                        // 處理非 TypeCode 涵蓋的特殊型別
                        if(actualTargetType == typeof(Guid) && value is string gStr)
                        {
                            return (object)Guid.Parse(gStr);
                        }
                        if(actualTargetType == typeof(TimeSpan))
                        {
                            if(value is string tsStr)
                            {
                                return (object)TimeSpan.Parse(tsStr);
                            }
                            if(value is double or int or long)
                            {
                                return (object)TimeSpan.FromMilliseconds(Convert.ToDouble(value));
                            }
                        }

                        if(actualTargetType.IsEnum)
                        {
                            if(value is string eStr)
                            {
                                return Enum.Parse(actualTargetType , eStr);
                            }
                            return Enum.ToObject(actualTargetType , value);
                        }
                        break;
                }

                // 基礎型別的通用轉換
                return Convert.ChangeType(value , actualTargetType , CultureInfo.InvariantCulture);
            }
            catch
            {
                // 如果是實體型別 (ValueType) 且非 Nullable，轉換失敗應回傳 Activator.CreateInstance 
                // 但為了簡單與一致性，通常回傳該型別的 default 值
                if(targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                {
                    return Activator.CreateInstance(targetType);
                }
                return null;
            }
        }
#endif
    }
}
