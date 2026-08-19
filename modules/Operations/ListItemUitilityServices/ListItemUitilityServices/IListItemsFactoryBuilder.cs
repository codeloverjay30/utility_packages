using ListItemUitilityServices.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ListItemUitilityServices
{
    public interface IListItemsFactoryBuilder
    {
        ListItemsFactoriesModel ListItemsFactoriesModel { get; }
        void Build();
    }
}
