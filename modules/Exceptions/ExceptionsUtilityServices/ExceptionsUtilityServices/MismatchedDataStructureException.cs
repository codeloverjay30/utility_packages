using System;

namespace System.Exceptions;

/// <summary>
/// Represents errors that occur when the expected data structure type 
/// does not match the actual data structure type provided.
/// </summary>
/// <typeparam name="TExpected">The type expected in the operation.</typeparam>
/// <typeparam name="TActual">The type actually provided.</typeparam>
public class MismatchedDataStructureException<TExpected, TActual> : ArgumentException
{
    private const string DefaultFallbackFormat = "Mismatched structure. Expected: {0}, Actual: {1}";

    /// <summary>
    /// Initializes a new instance of the <see cref="MismatchedDataStructureException{TExpected, TActual}"/> class.
    /// </summary>
    public MismatchedDataStructureException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MismatchedDataStructureException{TExpected, TActual}"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MismatchedDataStructureException(string message) : base(message, nameof(message)) { }

    /// <summary>
    /// Internal constructor to handle parameter naming explicitly.
    /// </summary>
    internal MismatchedDataStructureException(string message, string paramName) : base(message, paramName) { }

    /// <summary>
    /// Creates a new instance by validating type information before construction.
    /// </summary>
    /// <param name="expected">The expected object.</param>
    /// <param name="actual">The actual object.</param>
    /// <param name="format">The format string for the exception message.</param>
    /// <returns>A configured instance of <see cref="MismatchedDataStructureException{TExpected, TActual}"/>.</returns>
    public static MismatchedDataStructureException<TExpected, TActual> Create(
        TExpected expected,
        TActual actual,
        string format
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);

        Type typeOfExpected = expected?.GetType() ?? typeof(TExpected);
        Type typeOfActual = actual?.GetType() ?? typeof(TActual);

        string message;
        try
        {
            message = string.Format(format, typeOfExpected, typeOfActual);
        }
        catch (FormatException)
        {
            // 防禦性降級路徑：防止異常建構期間發生二次崩潰，確保原始錯誤資訊不遺失
            message = string.Format(DefaultFallbackFormat, typeOfExpected, typeOfActual) + $" (Raw Format: {format})";
        }

        return new MismatchedDataStructureException<TExpected, TActual>(message, nameof(expected));
    }

    /// <summary>
    /// Creates a new instance by validating explicit type metadata before construction.
    /// </summary>
    /// <param name="expectedType">The explicit metadata type expected.</param>
    /// <param name="actualType">The explicit metadata type actually provided.</param>
    /// <param name="format">The format string for the exception message.</param>
    /// <returns>A configured instance of <see cref="MismatchedDataStructureException{TExpected, TActual}"/>.</returns>
    public static MismatchedDataStructureException<TExpected, TActual> Create(
        Type expectedType,
        Type actualType,
        string format
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);
        
        string message;
        try
        {
            message = string.Format(format, expectedType?.Name ?? "Unknown", actualType?.Name ?? "Unknown");
        }
        catch (FormatException)
        {
            message = string.Format(DefaultFallbackFormat, expectedType?.Name ?? "Unknown", actualType?.Name ?? "Unknown") + $" (Raw Format: {format})";
        }
        
        return new MismatchedDataStructureException<TExpected, TActual>(message, "expected");
    }
}