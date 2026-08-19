using System.Numerics;

namespace MathUtilityServices
{
    public interface IMathUtilityService
    {
        IEnumerable<T> RangeFrom<T>(T startPoint,T endPoint)
            where T : INumber<T>;
        IEnumerable<T> RangeFrom<T>(T startPoint,T endPoint,T step)
            where T : INumber<T>;
    }
}
