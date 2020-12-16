using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.Webhook.Destinations
{
    /// <summary>
    /// Destination interface.
    /// </summary>
    /// <typeparam name="TDestinationOptions">The type of options.</typeparam>
    public interface IWebhookClient<in TDestinationOptions>
     where TDestinationOptions : BaseOption
    {
        /// <summary>
        /// Send message to destination.
        /// </summary>
        /// <param name="options">The destination options.</param>
        /// <param name="data">The message to send.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendItemAddedAsync(TDestinationOptions options, Dictionary<string, object> data);
    }
}