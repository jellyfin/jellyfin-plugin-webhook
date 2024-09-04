using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
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
public class SessionStartNotifier : IEventConsumer<SessionStartedEventArgs>, IDisposable
{
    private readonly IServerApplicationHost _applicationHost;
    private readonly IWebhookSender _webhookSender;
    private static readonly ConcurrentDictionary<string, DateTime> _recentEvents = new();
    private Timer _cleanupTimer;

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
        // Initialize and start the cleanup timer
        _cleanupTimer = new Timer(CleanupOldEntries, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
    }

    /// <inheritdoc />
    public async Task OnEvent(SessionStartedEventArgs eventArgs)
    {
        if (eventArgs.Argument is null)
        {
            return;
        }

        // Generate a unique key for this session event
        string sessionKey = eventArgs.Argument.Id;

        // Check if we've processed a similar event recently
        if (_recentEvents.TryGetValue(sessionKey, out DateTime lastProcessedTime) &&
            DateTime.UtcNow - lastProcessedTime < TimeSpan.FromSeconds(5))
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

    private void CleanupOldEntries(object? state)
    {
        DateTime threshold = DateTime.UtcNow - TimeSpan.FromMinutes(30);
        foreach (var key in _recentEvents.Keys.ToList())
        {
            if (_recentEvents.TryGetValue(key, out DateTime value) && value < threshold)
            {
                _recentEvents.TryRemove(key, out _);
            }
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="SessionStartNotifier"/> and optionally releases the managed resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="SessionStartNotifier"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">A boolean indicating whether to release both managed and unmanaged resources (true) or only unmanaged resources (false).</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose managed resources.
            _cleanupTimer?.Dispose();
        }

        // Dispose unmanaged resources (if any).
    }
}
