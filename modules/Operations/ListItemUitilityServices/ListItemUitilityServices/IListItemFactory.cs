using System;
using System.Collections.Generic;
using System.Text;

namespace ListItemUitilityServices
{
    public interface IListItemFactory
    {
        void Configure();
        string CreateListItems(string sep , IEnumerable<string> items , ListItemsType options = ListItemsType.NUMBER);
    }
}
