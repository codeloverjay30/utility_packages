using ListItemUitilityServices.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ListItemUitilityServices
{
    public class ListItemFactory(
        IListItemsFactoryBuilder listItemsFactoryBuilder = null
    ) : IListItemFactory
    {
        private readonly IListItemsFactoryBuilder _listItemsFactoryBuilder = listItemsFactoryBuilder ?? new ListItemsFactoryBuilder(null); // 內部實作中預設使用一個沒被設定過的Model

        private ListItemsFactoriesModel _listItemsFactoriesModel => _listItemsFactoryBuilder.ListItemsFactoriesModel;
        public void Configure()
        {
            _listItemsFactoryBuilder.Build();
        }

        public string CreateListItems(string sep , IEnumerable<string> items , ListItemsType options = ListItemsType.NUMBER)
        {
            string content = string.Empty;
            switch(options)
            {
                case ListItemsType.ALPHABET:
                    content = InternalCreateListItems_Alphabet(sep , items);
                    break;
                case ListItemsType.NUMBER:
                    content = InternalCreateListItems_Number(sep , items);
                    break;
                default:
                    throw new ArgumentException($"Invalid argument ListItemsType with value {options}");
            }

            return content;
        }
        private string InternalCreateListItems_Alphabet(string sep , IEnumerable<string> items)
        {
            var startPoint = 'A';
            var endPoint = (char)(startPoint + items.Count());
            var bulletinItems = _listItemsFactoriesModel.StringUtilityServices.RangeFrom(startPoint , endPoint).ToList();

            var menuItems = items.ToList();
            StringBuilder stringBuilder = new StringBuilder();
            for(int i = 0; i < bulletinItems.Count(); i++)
            {
                stringBuilder.Append(bulletinItems [ i ]).Append(sep).Append(menuItems [ i ]);
            }
            return stringBuilder.ToString();
        }
        private string InternalCreateListItems_Number(string sep , IEnumerable<string> items)
        {
            var startPoint = 1;
            var endPoint = startPoint + items.Count();
            var bulletinItems = _listItemsFactoriesModel.MathUtilityServices.RangeFrom(startPoint , endPoint).ToList();

            var menuItems = items.ToList();
            StringBuilder stringBuilder = new StringBuilder();
            for(int i = 0; i < bulletinItems.Count(); i++)
            {
                stringBuilder.Append(bulletinItems [ i ]).Append(sep).Append(menuItems [ i ]);
            }
            return stringBuilder.ToString();
        }
    }
}
