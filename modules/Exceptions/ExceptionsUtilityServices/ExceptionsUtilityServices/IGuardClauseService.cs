namespace ExceptionsUtilityServices;

using System;
using System.Buffers;

/// <summary>
/// Defines architectural defense guard clauses for asynchronous and heap-allocated memory segments.
/// </summary>
public interface IGuardClauseService
{
    void ThrowIfEmpty<T>(ReadOnlyMemory<T> argument, string? paramName = null);
    void ThrowIfEmpty<T>(in ReadOnlySequence<T> argument, string? paramName = null);
    void ThrowIfWhitespace(ReadOnlyMemory<char> argument, string? paramName = null);
    void ThrowIfWhitespace(in ReadOnlySequence<char> argument, string? paramName = null);
}