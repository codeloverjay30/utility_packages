namespace FractionsUtilityServices;
public class InvalidFractionException : Exception
{
    public decimal Denominator { get; }
    public InvalidFractionException(string message) : base(message) { }
    public InvalidFractionException(string message, decimal denominator) : base(message)
    {
        Denominator = denominator;
    }
}
