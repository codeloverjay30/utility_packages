using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using LanguageServerUtilityServices.Infrastructure.Interfaces;

namespace VscPlugins.ExtensionManagement.Services;

/// <summary>
/// Default implementation of <see cref="IExtensionActionExecutor"/> with defensive programming and file system abstraction.
/// </summary>
public class ExtensionActionExecutor : IExtensionActionExecutor
{

    private readonly ILanguageServerUtilityService _languageServerUtilityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionActionExecutor"/> class.
    /// </summary>
    /// <param name="languageServerUtilityService">The language server utility service.</param>
    public ExtensionActionExecutor(
        ILanguageServerUtilityService languageServerUtilityService
    )
    {
        ArgumentNullException.ThrowIfNull(languageServerUtilityService, nameof(languageServerUtilityService));

        _languageServerUtilityService = languageServerUtilityService;
    }

    /// <inheritdoc cref="IExtensionActionExecutor.ExecuteActionAsync"/>
    public async Task ExecuteActionAsync(
        string extensionId,
        ExtensionAction action,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(extensionId))
        {
            throw new ArgumentException("Extension identifier cannot be null or whitespace.", nameof(extensionId));
        }

        // Simulating asynchronous boundary checking and defensive handling
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        switch (action)
        {
            case ExtensionAction.Install:
                // TODO: Implement the installation logic for the extension.
                //await InstallExtensionInternalAsync(extensionId, cancellationToken);
                await _languageServerUtilityService.InstallExtensionAsync(extensionId, cancellationToken);
                break;
            case ExtensionAction.Enable:
                await ToggleExtensionStateAsync(extensionId, true, cancellationToken);
                break;
            case ExtensionAction.Disable:
                await ToggleExtensionStateAsync(extensionId, false, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, "Unsupported extension action specified.");
        }
    }

    private async Task ToggleExtensionStateAsync(
        string extensionId, 
        bool enable, 
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(extensionId))
        {
            throw new ArgumentException("Extension identifier cannot be null or whitespace.", nameof(extensionId));
        }

        // TODO: Integrate with LanguageServerUtilityServices for state updates or extension enablement synchronization
        // Example integration point for LanguageServer communication:
        // await LanguageServerUtilityServices.SendExtensionStateCommandAsync(extensionId, enable, cancellationToken);
        string actionFlag = enable ? "--enable-extension" : "--disable-extension";

        // Delegate state command to language server utility service infrastructure
        var arguments = new[] { actionFlag, extensionId };
        await _languageServerUtilityService.ShowMessageAsync("extension-state-update", arguments, cancellationToken);
    }
}