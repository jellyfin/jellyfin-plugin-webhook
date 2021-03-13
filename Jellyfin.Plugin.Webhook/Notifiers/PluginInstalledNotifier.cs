using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Common;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Updates;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Plugin installed notifier.
    /// </summary>
    public class PluginInstalledNotifier : IEventConsumer<PluginInstalledEventArgs>
    {
        private readonly IApplicationHost _applicationHost;
        private readonly IWebhookSender _webhookSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginInstalledNotifier"/> class.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
        public PluginInstalledNotifier(
            IApplicationHost applicationHost,
            IWebhookSender webhookSender)
        {
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;
        }

        /// <inheritdoc />
        public async Task OnEvent(PluginInstalledEventArgs eventArgs)
        {
            var dataObject = DataObjectHelpers
                .GetBaseDataObject(_applicationHost, NotificationType.PluginInstalled)
                .AddPluginInstallationInfo(eventArgs.Argument);

            await _webhookSender.SendNotification(NotificationType.PluginInstalled, dataObject)
                .ConfigureAwait(false);
        }
    }
}