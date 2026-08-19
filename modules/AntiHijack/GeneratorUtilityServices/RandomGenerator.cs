using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorUtilityServices;

public class RandomGenerator : IRandomGenerator
{
    public long GetRandomNumber()
    {
        Random random = new Random();
        // 產生一個介於 1000000000 到 9999999999 之間的隨機數字（10 位數）
        return random.NextInt64(1000000000, 10000000000);
    }
}
    
