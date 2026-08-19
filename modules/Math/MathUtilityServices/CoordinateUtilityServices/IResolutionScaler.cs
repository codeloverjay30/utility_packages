using System;
using System.Collections.Generic;
using System.Text;

namespace CoordinateUtilityServices
{
    public interface IResolutionScaler
    {
        Point Transform(Point basePoint);
        Point InverseTransform(Point actualPoint);
    }
}
