using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations; // 支援 DisplayAttribute
using System.Linq;
using System.Reflection;
using System.Text;

namespace EnumUtilityServices
{
    public class AttributeEnumProvider : IEnumProvider
    {
        // 雙層快取：第一層是 Type，第二層是用於快速查找描述的 Dictionary
        private readonly ConcurrentDictionary<Type , EnumData> _cache = new();

        // 內部私有結構，同時保存清單與查找字典
        

        public IEnumerable<EnumItem> GetEnumItems<T>() where T : struct, Enum
        {
            return GetOrAddCache<T>().Items;
        }

        public string GetDescription<T>(int value) where T : struct, Enum
        {
            var data = GetOrAddCache<T>();
            // O(1) 查找，效能極致
            return data.DescriptionMap.TryGetValue(value , out var desc) ? desc : value.ToString();
        }

        public EnumData GetOrAddCache<T>() where T : struct, Enum
        {
            return _cache.GetOrAdd(typeof(T) , type =>
            {
                var rawValues = (T [ ])Enum.GetValues(type);
                var items = new EnumItem [ rawValues.Length ];
                var nameMap = new (string Name , string Description , byte [ ] Utf8Bytes) [ rawValues.Length ];
                var descMap = new Dictionary<int , string>(rawValues.Length);

                for(int i = 0; i < rawValues.Length; i++)
                {
                    var e = rawValues [ i ];
                    var name = e.ToString();
                    var desc = ExtractDescription(e);
                    var val = Convert.ToInt32(e);

                    items [ i ] = new EnumItem { Value = val , Name = name , Description = desc };

                    // 關鍵：在此預存 UTF8 位元組，之後比對才不需要轉型
                    nameMap [ i ] = (name , desc , Encoding.UTF8.GetBytes(name));
                    descMap [ val ] = desc;
                }

                return new EnumData { Items = items , NameMap = nameMap , DescriptionMap = descMap };
            });
        }

        public string ExtractDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if(field == null) return value.ToString();

            var descAttr = field.GetCustomAttribute<DescriptionAttribute>();
            if(descAttr != null) return descAttr.Description;

            var displayAttr = field.GetCustomAttribute<DisplayAttribute>();
            if(displayAttr != null) return displayAttr.GetName();

            return value.ToString();
        }

        public void OverrideDescription<T>(int value , string newDescription) where T : struct, Enum
        {
            var data = GetOrAddCache<T>();
            // 因為我們內部使用了 Dictionary，可以直接覆蓋掉快取的描述
            // 這樣全專案只要透過 Provider 讀取的描述都會變，但 Enum 本身沒變
            data.DescriptionMap [ value ] = newDescription;

            // 同步更新 Items 陣列中的資料
            var item = data.Items.FirstOrDefault(i => i.Value == value);
            if(item != null) item.Description = newDescription;
        }

        /// <summary>
        /// 根據 ReadOnlySpan<char> 獲取描述 (零分配版本)
        /// </summary>
        public string GetDescriptionBySpan<T>(ReadOnlySpan<char> nameSpan) where T : struct, Enum
        {
            var data = GetOrAddCache<T>();

            // 在預存的 NameMap 中進行比對
            foreach(var mapping in data.NameMap)
            {
                // 使用 Span 的 Equals 方法，避免將 Span 轉回 string
                if(nameSpan.Equals(mapping.Name.AsSpan() , StringComparison.Ordinal))
                {
                    return mapping.Description;
                }
            }

            return nameSpan.ToString(); // 若完全找不到才轉字串回傳
        }

        public string GetDescriptionByUtf8Span<T>(ReadOnlySpan<byte> utf8Name) where T : struct, Enum
        {
            var data = GetOrAddCache<T>();

            // .NET 核心優化：直接在 UTF8 緩衝區上比對，不需要先轉成字串
            foreach(var mapping in data.NameMap)
            {
                // 假設我們在 NameMap 中預存了 UTF8 位元組陣列 (需在初始化時增加)
                if(utf8Name.SequenceEqual(mapping.Utf8Bytes))
                {
                    return mapping.Description;
                }
            }
            return Encoding.UTF8.GetString(utf8Name);
        }
    }
}
