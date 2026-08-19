using MathUtilityServices;
using SortingUtilityServices;
using StringUtilityServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace ListItemUitilityServices.Models
{
    public class ListItemsFactoriesModel
    {
        public ISortingUtilityService SortingUtilityService { get; set; }
        public IStringUtilityService StringUtilityServices { get; set; }
        public IMathUtilityService MathUtilityServices { get; set; }
    }
}
