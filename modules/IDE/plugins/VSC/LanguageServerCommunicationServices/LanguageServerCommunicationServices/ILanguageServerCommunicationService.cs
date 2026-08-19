using CliUtilityServices;

namespace LanguageServerCommunicationServices;

public interface ILanguageServerCommunicationService
{
    /// <summary>
    /// Utility method to show info with <paramref name="message"/> and <paramref name="pluginInfo"/>
    /// </summary>
    /// <param name="message">message that will shown in popup</param>
    /// <param name="pluginInfo">plugin info</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    Task ShowInfoAsync(
        string message,
        PluginInfo pluginInfo,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Utility method to execute <paramref name="commandLineInput"/> async with <paramref name="cancellationToken"/>
    /// </summary>
    /// <param name="commandLineInput">A <see cref="global::CliUtilityServices.CommandLineInput"/> represents the command that will be executed</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    Task ExecuteAsync(
        CommandLineInput commandLineInput,
        CancellationToken cancellationToken = default
    );
}
