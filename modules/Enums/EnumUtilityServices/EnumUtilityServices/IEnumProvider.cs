using System;
using System.Collections.Generic;
using System.Text;

namespace EnumUtilityServices
{
    public interface IEnumProvider
    {
        EnumData GetOrAddCache<T>() where T : struct, Enum;

        IEnumerable<EnumItem> GetEnumItems<T>() where T : struct, Enum;
        string GetDescription<T>(int value) where T : struct, Enum;
        string GetDescriptionBySpan<T>(ReadOnlySpan<char> nameSpan) where T : struct, Enum;

        string GetDescriptionByUtf8Span<T>(ReadOnlySpan<byte> utf8Name) where T : struct, Enum;

        void OverrideDescription<T>(int value , string newDescription) where T : struct, Enum;

    }
}
