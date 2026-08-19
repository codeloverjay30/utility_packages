using VscUtilityServices.Core.Models;


namespace VscUtilityServices.Validators;

/// <summary>
/// Contract for validating localized runtime SDK dependencies before invoking high-risk dynamic hooks.
/// </summary>
public interface IRuntimeEnvironmentValidator
{
    /// <summary>
    /// Validates whether the required SDK version is available on the local operating system host.
    /// </summary>
    /// <param name="requirement">The runtime requirement to validate.</param>
    /// <returns>True if the environment satisfies the constraint; otherwise, false.</returns>
    Task<bool> ValidateRuntimeExistsAsync(RuntimeRequirement requirement);
}
