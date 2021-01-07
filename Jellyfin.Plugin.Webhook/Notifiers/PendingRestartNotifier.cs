using System.Threading.Tasks;
using Jellyfin.Data.Events.System;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Common;
using MediaBrowser.Controller.Events;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Pending restart notifier.
    /// </summary>
    public class PendingRestartNotifier : IEventConsumer<PendingRestartEventArgs>
    {
        private readonly IApplicationHost _applicationHost;
        private readonly IWebhookSender _webhookSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="PendingRestartNotifier"/> class.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
        public PendingRestartNotifier(
            IApplicationHost applicationHost,
            IWebhookSender webhookSender)
        {
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;
        }

        /// <inheritdoc />
        public async Task OnEvent(PendingRestartEventArgs eventArgs)
        {
            var dataObject = DataObjectHelpers
                .GetBaseDataObject(_applicationHost, NotificationType.PendingRestart);

            await _webhookSender.SendNotification(NotificationType.PendingRestart, dataObject);
        }
    }
}