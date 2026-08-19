using SortingUtilityServices;
using System.Collections;

namespace StringUtilityServices
{
    public class StringUtilityService(ISortingUtilityService sortingUtilityService) : IStringUtilityService
    {
        private readonly ISortingUtilityService _sortingUtilityService = sortingUtilityService;
        public IEnumerable<char> RangeFrom(char startPoint , char endPoint)
        {
            (startPoint, endPoint) = _sortingUtilityService.GetSortedPair(startPoint, endPoint);
            for(char c = startPoint; c <= endPoint; c++)
            {
                yield return c;
            }
        }
    }
}
