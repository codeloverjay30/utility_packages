namespace AntiHijackUtilityServices.Abstractions;

public interface IAntiHijackService
{
    public bool ValidateRequest(ReadOnlySpan<char> payload, ReadOnlySpan<byte> secretKey);
}
