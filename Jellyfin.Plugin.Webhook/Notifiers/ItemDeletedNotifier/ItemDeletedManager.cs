using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using Jellyfin.Plugin.Webhook.Models;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemDeletedNotifier;

/// <inheritdoc />
public class ItemDeletedManager : IItemDeletedManager
{
    private readonly ILogger<ItemDeletedManager> _logger;
    private readonly ILibraryManager _libraryManager;
    private readonly IServerApplicationHost _applicationHost;
    private readonly ConcurrentDictionary<Guid, QueuedItemContainer> _itemProcessQueue;
    private readonly ConcurrentDictionary<Guid, BaseItem> _deletedItems;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemDeletedManager"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{ItemDeletedManager}"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    public ItemDeletedManager(
        ILogger<ItemDeletedManager> logger,
        ILibraryManager libraryManager,
        IServerApplicationHost applicationHost)
    {
        _logger = logger;
        _libraryManager = libraryManager;
        _applicationHost = applicationHost;
        _itemProcessQueue = new ConcurrentDictionary<Guid, QueuedItemContainer>();
        _deletedItems = new ConcurrentDictionary<Guid, BaseItem>();
    }

    /// <inheritdoc />
    public async Task ProcessItemsAsync()
    {
        _logger.LogDebug("ProcessItemsAsync");
        // Attempt to process all items in queue.
        var currentItems = _itemProcessQueue.ToArray();
        if (currentItems.Length != 0)
        {
            var scope = _applicationHost.ServiceProvider!.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                var webhookSender = scope.ServiceProvider.GetRequiredService<IWebhookSender>();
                foreach (var (key, container) in currentItems)
                {
                    if (_deletedItems.TryGetValue(key, out var item) && item != null)
                    {
                        // Skip notification if item type is Studio
                        if (container.ItemType == "Studio")
                        {
                            _logger.LogDebug("Skipping notification for item type Studio");
                            _itemProcessQueue.TryRemove(key, out _);
                            _deletedItems.TryRemove(key, out _);
                            continue;
                        }

                        _logger.LogDebug("Notifying for {ItemName}", container.Name);

                        // Send notification to each configured destination.
                        var dataObject = DataObjectHelpers
                            .GetBaseDataObject(_applicationHost, NotificationType.ItemDeleted)
                            .AddBaseItemData(item);

                        var itemType = Type.GetType($"MediaBrowser.Controller.Entities.{container.ItemType}");
                        await webhookSender.SendNotification(NotificationType.ItemDeleted, dataObject, itemType)
                            .ConfigureAwait(false);

                        // Remove item from queue.
                        _itemProcessQueue.TryRemove(key, out _);
                        _deletedItems.TryRemove(key, out _);
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    public void AddItem(BaseItem item)
    {
        var itemType = item.GetType().Name;
        _itemProcessQueue.TryAdd(item.Id, new QueuedItemContainer(item.Id, item.Name, itemType));
        _deletedItems.TryAdd(item.Id, item);
        _logger.LogDebug("Queued {ItemName} for notification", item.Name);
    }
}
