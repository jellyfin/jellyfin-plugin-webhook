using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Common;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Updates;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Plugin installing notifier.
    /// </summary>
    public class PluginInstallingNotifier : IEventConsumer<PluginInstallingEventArgs>
    {
        private readonly IApplicationHost _applicationHost;
        private readonly IWebhookSender _webhookSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginInstallingNotifier"/> class.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
        public PluginInstallingNotifier(
            IApplicationHost applicationHost,
            IWebhookSender webhookSender)
        {
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;
        }

        /// <inheritdoc />
        public async Task OnEvent(PluginInstallingEventArgs eventArgs)
        {
            var dataObject = DataObjectHelpers
                .GetBaseDataObject(_applicationHost, NotificationType.PluginInstalling)
                .AddPluginInstallationInfo(eventArgs.Argument);

            await _webhookSender.SendNotification(NotificationType.PluginInstalling, dataObject);
        }
    }
}