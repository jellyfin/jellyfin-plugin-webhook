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

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemDeletedNotifier;

/// <inheritdoc />
public class ItemDeletedManager : IItemDeletedManager
{
    private readonly ILogger<ItemDeletedManager> _logger;
    private readonly ILibraryManager _libraryManager;
    private readonly IServerApplicationHost _applicationHost;
    private readonly ConcurrentDictionary<Guid, BaseItem> _itemProcessQueue;

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
        _itemProcessQueue = new ConcurrentDictionary<Guid, BaseItem>();
    }

    /// <inheritdoc />
    public async Task ProcessItemsAsync()
    {
        _logger.LogDebug("ProcessItemsAsync");
        // Attempt to process all items in queue.
        if (!_itemProcessQueue.IsEmpty)
        {
            var scope = _applicationHost.ServiceProvider!.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                var webhookSender = scope.ServiceProvider.GetRequiredService<IWebhookSender>();
                foreach (var (key, item) in _itemProcessQueue)
                {
                    if (item != null)
                    {
                        _logger.LogDebug("Item {ItemName}", item.Name);

                        // Skip notification if item type is Studio
                        if (item.GetType().Name == "Studio")
                        {
                            _logger.LogDebug("Skipping notification for item type Studio");
                            _itemProcessQueue.TryRemove(key, out _);
                            continue;
                        }

                        _logger.LogDebug("Notifying for {ItemName}", item.Name);

                        // Send notification to each configured destination.
                        var dataObject = DataObjectHelpers
                            .GetBaseDataObject(_applicationHost, NotificationType.ItemDeleted)
                            .AddBaseItemData(item);

                        var itemType = item.GetType();
                        await webhookSender.SendNotification(NotificationType.ItemDeleted, dataObject, itemType)
                            .ConfigureAwait(false);

                        // Remove item from queue.
                        _itemProcessQueue.TryRemove(key, out _);
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    public void AddItem(BaseItem item)
    {
        _itemProcessQueue.TryAdd(item.Id, item);
        _logger.LogDebug("Queued {ItemName} for notification", item.Name);
    }
}
