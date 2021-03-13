using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Controller.Events;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Plugin installation failed notifier.
    /// </summary>
    public class PluginInstallationFailedNotifier : IEventConsumer<InstallationFailedEventArgs>
    {
        private readonly IApplicationHost _applicationHost;
        private readonly IWebhookSender _webhookSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginInstallationFailedNotifier"/> class.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
        public PluginInstallationFailedNotifier(
            IApplicationHost applicationHost,
            IWebhookSender webhookSender)
        {
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;
        }

        /// <inheritdoc />
        public async Task OnEvent(InstallationFailedEventArgs eventArgs)
        {
            var dataObject = DataObjectHelpers
                .GetBaseDataObject(_applicationHost, NotificationType.PluginInstallationFailed)
                .AddPluginInstallationInfo(eventArgs.InstallationInfo)
                .AddExceptionInfo(eventArgs.Exception);

            await _webhookSender.SendNotification(NotificationType.PluginInstallationFailed, dataObject)
                .ConfigureAwait(false);
        }
    }
}