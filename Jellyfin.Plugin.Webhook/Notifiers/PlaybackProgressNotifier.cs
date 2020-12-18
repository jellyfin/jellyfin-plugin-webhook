using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Common;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Playback progress notifier.
    /// </summary>
    public class PlaybackProgressNotifier : IEventConsumer<PlaybackProgressEventArgs>
    {
        private readonly IApplicationHost _applicationHost;
        private readonly WebhookSender _webhookSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackProgressNotifier"/> class.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="WebhookSender"/>.</param>
        public PlaybackProgressNotifier(
            IApplicationHost applicationHost,
            WebhookSender webhookSender)
        {
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;
        }

        /// <inheritdoc />
        public async Task OnEvent(PlaybackProgressEventArgs eventArgs)
        {
            if (eventArgs.Item == null)
            {
                // No item.
                return;
            }

            if (eventArgs.Item.IsThemeMedia)
            {
                // Don't report theme song or local trailer playback.
                return;
            }

            if (eventArgs.Users.Count == 0)
            {
                // No users in playback session.
                return;
            }

            var dataObject = DataObjectHelpers
                .GetBaseDataObject(_applicationHost, NotificationType.PlaybackProgress)
                .AddBaseItemData(eventArgs.Item)
                .AddPlaybackProgressData(eventArgs);

            foreach (var user in eventArgs.Users)
            {
                var userDataObject = new Dictionary<string, object>(dataObject)
                {
                    ["Username"] = user.Username,
                    ["UserId"] = user.Id
                };

                await _webhookSender.SendItemNotification(NotificationType.PlaybackProgress, userDataObject, eventArgs.Item.GetType());
            }
        }
    }
}