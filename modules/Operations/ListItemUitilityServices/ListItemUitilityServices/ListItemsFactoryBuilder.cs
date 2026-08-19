using ListItemUitilityServices.Models;
using MathUtilityServices;
using SortingUtilityServices;
using StringUtilityServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace ListItemUitilityServices
{
    public class ListItemsFactoryBuilder(
        ListItemsFactoriesModel listItemsFactoriesModel = null
    ) : IListItemsFactoryBuilder
    {
        private readonly ListItemsFactoriesModel _listItemsFactoriesModel = listItemsFactoriesModel ?? new ListItemsFactoriesModel(); // 存取預設實體化一個沒被設定過的Model (若為null)
        public ListItemsFactoriesModel ListItemsFactoriesModel => _listItemsFactoriesModel;
        public void Build()
        {
            _listItemsFactoriesModel.SortingUtilityService = _listItemsFactoriesModel.SortingUtilityService ?? new SortingUtilityService(); // 預設使用  SortingUtilityService 類別

            _listItemsFactoriesModel.StringUtilityServices = _listItemsFactoriesModel.StringUtilityServices ?? new StringUtilityService(_listItemsFactoriesModel.SortingUtilityService); // 預設使用  StringUtilityServices 類別

            _listItemsFactoriesModel.MathUtilityServices = _listItemsFactoriesModel.MathUtilityServices ??
                new MathUtilityService(_listItemsFactoriesModel.SortingUtilityService); // 預設使用 MathUtilityServices 類別
        }
    }
}
