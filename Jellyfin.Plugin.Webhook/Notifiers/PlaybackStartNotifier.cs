using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.Webhook.Notifiers;

/// <summary>
/// Playback start notifier.
/// </summary>
public class PlaybackStartNotifier : IEventConsumer<PlaybackStartEventArgs>
{
    private readonly IServerApplicationHost _applicationHost;
    private readonly IWebhookSender _webhookSender;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackStartNotifier"/> class.
    /// </summary>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
    public PlaybackStartNotifier(
        IServerApplicationHost applicationHost,
        IWebhookSender webhookSender)
    {
        _applicationHost = applicationHost;
        _webhookSender = webhookSender;
    }

    /// <inheritdoc />
    public async Task OnEvent(PlaybackStartEventArgs eventArgs)
    {
        if (eventArgs.Item is null)
        {
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
            .GetBaseDataObject(_applicationHost, NotificationType.PlaybackStart)
            .AddBaseItemData(eventArgs.Item)
            .AddPlaybackProgressData(eventArgs);

        foreach (var user in eventArgs.Users)
        {
            var userDataObject = new Dictionary<string, object>(dataObject)
            {
                ["NotificationUsername"] = user.Username,
                ["UserId"] = user.Id
            };

            await _webhookSender.SendNotification(NotificationType.PlaybackStart, userDataObject, eventArgs.Item.GetType())
                .ConfigureAwait(false);
        }
    }
}
