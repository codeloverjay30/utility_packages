using System;
using System.Numerics;
using FractionsUtilityServices; // 使用 BigInteger 避免溢位

public ref struct Fraction
{
    public decimal Numerator { get; }
    public decimal Denominator { get; }

    public Fraction(
        decimal numerator,
        decimal denominator
    )
    {
        if (denominator == 0)
        {
            throw new InvalidFractionException("denominator can't be zero", denominator);
        }
        
        decimal gcd = GetGCD(numerator, denominator);
        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
    }

    /// <summary>
    /// convert <paramref name="value"/> to <see cref="global::Fraction"/>
    /// </summary>
    /// <param name="value">a float number with <see cref="global::System.Decimal"/> type</param>
    /// <returns></returns>
    public static Fraction FromDecimal(decimal value)
    {
        // decimal.GetBits 回傳一個 int[4]
        // [0], [1], [2] 為整數部分，[3] 包含符號與縮放比例 (scale)
        int[] bits = decimal.GetBits(value);

        // 獲取小數點後的位數 (Scale)
        // 透過位元運算取得第 3 個整數的 16-23 位元 (Scale)
        int scale = (bits[3] >> 16) & 0x7F;

        // 如果沒有小數，直接回傳
        if (scale == 0)
        {
            return new Fraction((decimal)value, 1);
        }

        // 計算分母 10^scale
        decimal denominator = (decimal)Math.Pow(10, scale);

        // 計算分子 (將 value 轉為無小數狀態的整數)
        decimal numerator = (value * denominator);

        return new Fraction(numerator, denominator);
    }

    /// <summary>
    /// GCD of <paramref name="a"/> and <paramref name="b"/>
    /// </summary>
    /// <param name="a">A number</param>
    /// <param name="b">Other number</param>
    /// <returns></returns>
    private static decimal GetGCD(decimal a, decimal b)
    {
        // 使用迴圈替代遞迴，避免潛在的 StackOverflow 風險
        while (b != 0)
        {
            a %= b;
            // 交換 a 與 b
            (a, b) = (b, a);
        }
        return Math.Abs(a);
    }
}