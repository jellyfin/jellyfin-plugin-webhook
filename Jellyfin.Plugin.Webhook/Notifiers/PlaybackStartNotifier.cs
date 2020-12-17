using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Common;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Playback start notifier.
    /// </summary>
    public class PlaybackStartNotifier : IEventConsumer<PlaybackStartEventArgs>
    {
        private readonly IApplicationHost _applicationHost;
        private readonly WebhookSender _webhookSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackStartNotifier"/> class.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="WebhookSender"/>.</param>
        public PlaybackStartNotifier(
            IApplicationHost applicationHost,
            WebhookSender webhookSender)
        {
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;
        }

        /// <inheritdoc />
        public async Task OnEvent(PlaybackStartEventArgs eventArgs)
        {
            if (eventArgs.Item == null)
            {
                // No item.
                return;
            }

            if (eventArgs.Item.IsThemeMedia)
            {
                // Don't report theme sng or local trailer playback.
                return;
            }

            if (eventArgs.Users.Count == 0)
            {
                // No users in playback session.
                return;
            }

            var data = DataObjectHelpers.GetBaseItemDataObject(_applicationHost, eventArgs.Item);
            data["Usernames"] = string.Join(',', eventArgs.Users.Select(u => u.Username));
            data["UserIds"] = string.Join(',', eventArgs.Users.Select(u => u.Id));
            await _webhookSender.SendItemNotification(NotificationType.PlaybackStart, data, eventArgs.Item.GetType());
        }
    }
}