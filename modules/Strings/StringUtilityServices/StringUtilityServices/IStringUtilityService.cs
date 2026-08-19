using System;
using System.Collections.Generic;
using System.Text;

namespace StringUtilityServices
{
    public interface IStringUtilityService
    {
        IEnumerable<char> RangeFrom(char startPoint,char endPoint);
    }
}
