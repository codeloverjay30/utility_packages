using System;
using System.Collections.Generic;

namespace ExceptionsUtilityServices;

/// <summary>
/// Exception thrown when one or more members of a class fail validation rules defined by Data Annotations.
/// </summary>
public class InvalidMembersException : ArgumentException, IInvalidMembersException
{
    /// <summary>
    /// Gets the collection of invalid member names and their respective error messages.
    /// </summary>
    public IReadOnlyDictionary<string, string> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidMembersException"/> class.
    /// </summary>
    /// <param name="message">The summary error message.</param>
    /// <param name="validationErrors">A dictionary containing property names and their validation failure details.</param>
    public InvalidMembersException(string message, IReadOnlyDictionary<string, string> validationErrors) 
        : base(message)
    {
        ValidationErrors = validationErrors;
    }
}