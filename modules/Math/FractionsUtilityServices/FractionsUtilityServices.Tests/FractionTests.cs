using FluentAssertions;

namespace FractionsUtilityServices.Tests;

public class FractionTests
{
    [Theory]
    [InlineData(1, 2, 1, 2)]     // 基本約分
    [InlineData(2, 4, 1, 2)]     // 約分測試
    [InlineData(10, 5, 2, 1)]    // 整數測試
    [InlineData(-1, 2, -1, 2)]   // 負數測試
    public void Constructor_ShouldSimplifyCorrectly(decimal num, decimal den, decimal expectedNum, decimal expectedDen)
    {
        var fraction = new Fraction(num, den);
        fraction.Numerator.Should().Be(expectedNum);
        fraction.Denominator.Should().Be(expectedDen);
    }

    [Theory]
    [InlineData(0.5, 1, 2)]      // 0.5 -> 1/2
    [InlineData(0.75, 3, 4)]     // 0.75 -> 3/4
    [InlineData(1.2, 6, 5)]      // 1.2 -> 6/5
    [InlineData(0.125, 1, 8)]    // 0.125 -> 1/8
    public void FromDecimal_ShouldConvertCorrectly(decimal input, decimal expectedNum, decimal expectedDen)
    {
        var fraction = Fraction.FromDecimal(input);
        fraction.Numerator.Should().Be(expectedNum);
        fraction.Denominator.Should().Be(expectedDen);
    }
}