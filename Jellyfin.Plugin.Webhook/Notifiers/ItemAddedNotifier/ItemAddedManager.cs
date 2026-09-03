using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemAddedNotifier;

/// <inheritdoc />
public class ItemAddedManager : IItemAddedManager
{
    private readonly ILogger<ItemAddedManager> _logger;
    private readonly ILibraryManager _libraryManager;
    private readonly IServerApplicationHost _applicationHost;
    private readonly ConcurrentDictionary<Guid, byte> _pendingItems;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemAddedManager"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{ItemAddedManager}"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    public ItemAddedManager(
        ILogger<ItemAddedManager> logger,
        ILibraryManager libraryManager,
        IServerApplicationHost applicationHost)
    {
        _logger = logger;
        _libraryManager = libraryManager;
        _applicationHost = applicationHost;
        _pendingItems = new ConcurrentDictionary<Guid, byte>();
    }

    /// <inheritdoc />
    public void AddItem(BaseItem item)
    {
        _pendingItems.TryAdd(item.Id, 0);
        _logger.LogDebug("Queued {ItemName} for ItemAdded notification", item.Name);
    }

    /// <inheritdoc />
    public void HandleItemUpdated(BaseItem item)
    {
        if (!_pendingItems.TryRemove(item.Id, out _))
        {
            return;
        }

        _logger.LogDebug("Item {ItemName} updated and was in pending queue, firing webhook after delay", item.Name);

        // Fire after a short delay on a background thread.
        _ = Task.Run(async () =>
        {
            try
            {
                var delayMs = (WebhookPlugin.Instance?.Configuration.ItemNotificationDelay ?? 5) * 1000;
                await Task.Delay(delayMs).ConfigureAwait(false);
                await SendNotificationAsync(item.Id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ItemAdded notification for {ItemName}", item.Name);
            }
        });
    }

    private async Task SendNotificationAsync(Guid itemId)
    {
        var item = _libraryManager.GetItemById(itemId);
        if (item is null)
        {
            _logger.LogDebug("Item {ItemId} no longer exists, skipping notification", itemId);
            return;
        }

        _logger.LogDebug("Notifying for {ItemName}", item.Name);

        var scope = _applicationHost.ServiceProvider!.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var webhookSender = scope.ServiceProvider.GetRequiredService<IWebhookSender>();

            var dataObject = DataObjectHelpers
                .GetBaseDataObject(_applicationHost, NotificationType.ItemAdded)
                .AddBaseItemData(item);

            var itemType = item.GetType();
            await webhookSender.SendNotification(NotificationType.ItemAdded, dataObject, itemType)
                .ConfigureAwait(false);
        }
    }
}
