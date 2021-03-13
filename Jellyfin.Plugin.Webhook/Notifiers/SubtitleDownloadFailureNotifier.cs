using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Common;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Subtitles;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Subtitle download failure notifier.
    /// </summary>
    public class SubtitleDownloadFailureNotifier : IEventConsumer<SubtitleDownloadFailureEventArgs>
    {
        private readonly IApplicationHost _applicationHost;
        private readonly IWebhookSender _webhookSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubtitleDownloadFailureNotifier"/> class.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
        public SubtitleDownloadFailureNotifier(
            IApplicationHost applicationHost,
            IWebhookSender webhookSender)
        {
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;
        }

        /// <inheritdoc />
        public async Task OnEvent(SubtitleDownloadFailureEventArgs eventArgs)
        {
            if (eventArgs.Item == null)
            {
                return;
            }

            var dataObject = DataObjectHelpers
                .GetBaseDataObject(_applicationHost, NotificationType.SubtitleDownloadFailure)
                .AddBaseItemData(eventArgs.Item);
            await _webhookSender.SendNotification(NotificationType.SubtitleDownloadFailure, dataObject, eventArgs.Item.GetType())
                .ConfigureAwait(false);
        }
    }
}