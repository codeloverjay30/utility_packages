using System.Buffers;

namespace ExceptionsUtilityServices;

/// <summary>
/// Production implementation of defensive guard operations.
/// </summary>
public class GuardClauseService : IGuardClauseService
{
    public void ThrowIfEmpty<T>(ReadOnlyMemory<T> argument, string? paramName = null) 
        => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrEmpty(argument, paramName);

    public void ThrowIfEmpty<T>(in ReadOnlySequence<T> argument, string? paramName = null) 
        => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrEmpty(argument, paramName);

    public void ThrowIfWhitespace(ReadOnlyMemory<char> argument, string? paramName = null) 
        => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(argument, paramName);

    public void ThrowIfWhitespace(in ReadOnlySequence<char> argument, string? paramName = null) 
        => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(argument, paramName);
}