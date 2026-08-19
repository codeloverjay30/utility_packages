namespace VscPlugins.ExtensionManagement.Services;

/// <summary>
/// Provides services to execute extension installation or enable/disable logic based on the specified action.
/// </summary>
public interface IExtensionActionExecutor
{
    /// <summary>
    /// Executes the given extension action asynchronously.
    /// </summary>
    /// <param name="extensionId">The target extension identifier.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteActionAsync(string extensionId, ExtensionAction action, CancellationToken cancellationToken = default);
}
    
