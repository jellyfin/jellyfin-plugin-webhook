using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Session;

namespace Jellyfin.Plugin.Webhook.Notifiers;

/// <summary>
/// Session start notifier.
/// </summary>
public class SessionStartNotifier : IEventConsumer<SessionStartedEventArgs>
{
    private readonly IServerApplicationHost _applicationHost;
    private readonly IWebhookSender _webhookSender;
    private static readonly ConcurrentDictionary<string, DateTime> _recentEvents = new();
    private static readonly TimeSpan RecentEventThreshold = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan CleanupThreshold = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionStartNotifier"/> class.
    /// </summary>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
    public SessionStartNotifier(
        IServerApplicationHost applicationHost,
        IWebhookSender webhookSender)
    {
        _applicationHost = applicationHost;
        _webhookSender = webhookSender;
    }

    /// <inheritdoc />
    public async Task OnEvent(SessionStartedEventArgs eventArgs)
    {
        if (eventArgs.Argument is null)
        {
            return;
        }

        // Clean up old session entries when a new session event is triggered
        CleanupOldEntries();

        // Generate a unique key for this session event
        string sessionKey = eventArgs.Argument.Id;

        // Check if we've processed a similar event recently
        if (_recentEvents.TryGetValue(sessionKey, out DateTime lastProcessedTime) &&
            DateTime.UtcNow - lastProcessedTime < RecentEventThreshold)
        {
            return;
        }

        // Update the cache with the latest event time
        _recentEvents[sessionKey] = DateTime.UtcNow;

        var dataObject = DataObjectHelpers
            .GetBaseDataObject(_applicationHost, NotificationType.SessionStart)
            .AddSessionInfoData(eventArgs.Argument)
            .AddBaseItemData(eventArgs.Argument.FullNowPlayingItem);

        await _webhookSender.SendNotification(NotificationType.SessionStart, dataObject)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Cleans up old session entries from the cache.
    /// </summary>
    private static void CleanupOldEntries()
    {
        DateTime threshold = DateTime.UtcNow - CleanupThreshold;
        var keysToRemove = _recentEvents
            .Where(kvp => kvp.Value < threshold)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _recentEvents.TryRemove(key, out _);
        }
    }
}
