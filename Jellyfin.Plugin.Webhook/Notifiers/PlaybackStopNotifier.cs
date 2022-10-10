using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.Webhook.Notifiers;

/// <summary>
/// Playback stop notifier.
/// </summary>
public class PlaybackStopNotifier : IEventConsumer<PlaybackStopEventArgs>
{
    private readonly IServerApplicationHost _applicationHost;
    private readonly IWebhookSender _webhookSender;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackStopNotifier"/> class.
    /// </summary>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
    public PlaybackStopNotifier(
        IServerApplicationHost applicationHost,
        IWebhookSender webhookSender)
    {
        _applicationHost = applicationHost;
        _webhookSender = webhookSender;
    }

    /// <inheritdoc />
    public async Task OnEvent(PlaybackStopEventArgs eventArgs)
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
            .GetBaseDataObject(_applicationHost, NotificationType.PlaybackStop)
            .AddBaseItemData(eventArgs.Item)
            .AddPlaybackProgressData(eventArgs);
        dataObject[nameof(eventArgs.PlayedToCompletion)] = eventArgs.PlayedToCompletion;

        foreach (var user in eventArgs.Users)
        {
            var userDataObject = new Dictionary<string, object>(dataObject)
            {
                ["NotificationUsername"] = user.Username,
                ["UserId"] = user.Id
            };

            await _webhookSender.SendNotification(NotificationType.PlaybackStop, userDataObject, eventArgs.Item.GetType())
                .ConfigureAwait(false);
        }
    }
}
