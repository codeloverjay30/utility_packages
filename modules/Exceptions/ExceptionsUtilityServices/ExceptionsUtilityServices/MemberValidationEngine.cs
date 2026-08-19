using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ExceptionsUtilityServices;

/// <summary>
/// Engine responsible for reflecting over objects and validating them against Data Annotations.
/// </summary>
public static class MemberValidationEngine
{
    /// <summary>
    /// Validates an object instance based on its Data Annotations.
    /// </summary>
    /// <typeparam name="T">The type of the object to validate.</typeparam>
    /// <param name="instance">The object instance.</param>
    /// <exception cref="InvalidMembersException">Thrown when validation fails.</exception>
    public static void Validate<T>(T instance)
    {
        var errors = new Dictionary<string, string>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(instance);
            var context = new ValidationContext(instance!) { MemberName = prop.Name };
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateProperty(value, context, results))
            {
                errors[prop.Name] = results[0].ErrorMessage ?? "Invalid value.";
            }
        }

        if (errors.Count > 0)
        {
            throw new InvalidMembersException($"Validation failed for type {typeof(T).Name}.", errors);
        }
    }
}