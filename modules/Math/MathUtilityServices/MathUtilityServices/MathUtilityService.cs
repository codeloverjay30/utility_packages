using SortingUtilityServices;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MathUtilityServices
{
    public class MathUtilityService(ISortingUtilityService sortingUtilityService) : IMathUtilityService
    {
        private readonly ISortingUtilityService _sortingUtilityService = sortingUtilityService;

        public IEnumerable<T> RangeFrom<T>(T startPoint , T endPoint) where T : INumber<T>
        {
            return RangeFrom(startPoint , endPoint , T.One);
        }
        public IEnumerable<T> RangeFrom<T>(T startPoint , T endPoint , T step) where T : INumber<T>
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(step , nameof(step));
            (startPoint , endPoint) = _sortingUtilityService.GetSortedPair(startPoint , endPoint);

            for(T n = startPoint; n <= endPoint; n += step)
            {
                yield return n;
            }
        }
    }
}
