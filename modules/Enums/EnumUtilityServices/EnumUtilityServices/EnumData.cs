using System;
using System.Collections.Generic;
using System.Text;

namespace EnumUtilityServices
{
    public class EnumData
    {
        public required EnumItem [ ] Items { get; init; }
        public required Dictionary<int , string> DescriptionMap { get; init; }
        // 統一使用包含 Utf8Bytes 的元組
        public required (string Name , string Description , byte [ ] Utf8Bytes) [ ] NameMap { get; init; }
    }
}
