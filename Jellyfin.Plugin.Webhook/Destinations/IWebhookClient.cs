using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.Webhook.Destinations;

/// <summary>
/// Destination interface.
/// </summary>
/// <typeparam name="TDestinationOption">The type of options.</typeparam>
public interface IWebhookClient<in TDestinationOption>
    where TDestinationOption : BaseOption
{
    /// <summary>
    /// Send message to destination.
    /// </summary>
    /// <param name="option">The destination option.</param>
    /// <param name="data">The message to send.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SendAsync(TDestinationOption option, Dictionary<string, object> data);
}
