using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Extensions;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using Jellyfin.Plugin.Webhook.Models;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemAddedNotifier;

/// <inheritdoc />
public class ItemAddedManager : IItemAddedManager
{
    private const int MaxRetries = 10;
    private readonly ILogger<ItemAddedManager> _logger;
    private readonly ILibraryManager _libraryManager;
    private readonly IServerApplicationHost _applicationHost;
    private readonly ConcurrentDictionary<Guid, QueuedItemContainer> _itemProcessQueue;
    private static Semaphore _lock = new(initialCount: 0, maximumCount: 1);

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
        _itemProcessQueue = new ConcurrentDictionary<Guid, QueuedItemContainer>();
    }

    /// <inheritdoc />
    public async Task ProcessItemsAsync()
    {
        // Wait until a semaphore task is available
        // meaning our queue isn't waiting for a task.
        _lock.WaitOne();

        _logger.LogDebug("ProcessItemsAsync");
        // Attempt to process all items in queue.
        var currentItems = _itemProcessQueue.ToArray();

        var itemsIds = currentItems.
            Select((item) => _libraryManager.GetItemById(item.Key)).
            Where((item) => item is not null).
            Select((item) => item!.Id).
            ToList();

        if (currentItems.Length != 0)
        {
            var scope = _applicationHost.ServiceProvider!.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                var webhookSender = scope.ServiceProvider.GetRequiredService<IWebhookSender>();
                foreach (var (key, container) in currentItems)
                {
                    var item = _libraryManager.GetItemById(key);
                    if (item is null)
                    {
                        // Remove item from queue.
                        _itemProcessQueue.TryRemove(key, out _);
                        return;
                    }

                    var parentInterect = false;
                    var parent = _libraryManager.GetItemById(item.ParentId);
                    while (parent is not null)
                    {
                        if (itemsIds.Contains(parent.Id))
                        {
                            parentInterect = true;
                            break;
                        }

                        if (parent!.ParentId.IsEmpty())
                        {
                            parent = _libraryManager.GetItemById(parent.ParentId);
                        }
                    }

                    // Remove the item if the parent is in the queue
                    if (parentInterect)
                    {
                        _itemProcessQueue.TryRemove(key, out _);
                        continue;
                    }

                    _logger.LogDebug("Item {ItemName}", item.Name);

                    // Metadata not refreshed yet and under retry limit.
                    if (item.ProviderIds.Keys.Count == 0 && container.RetryCount < MaxRetries)
                    {
                        _logger.LogDebug("Requeue {ItemName}, no provider ids", item.Name);
                        container.RetryCount++;
                        _itemProcessQueue.AddOrUpdate(key, container, (_, _) => container);
                        continue;
                    }

                    _logger.LogDebug("Notifying for {ItemName}", item.Name);

                    // Send notification to each configured destination.
                    var dataObject = DataObjectHelpers
                        .GetBaseDataObject(_applicationHost, NotificationType.ItemAdded)
                        .AddBaseItemData(item);

                    var itemType = item.GetType();
                    await webhookSender.SendNotification(NotificationType.ItemAdded, dataObject, itemType)
                        .ConfigureAwait(false);

                    // Remove item from queue.
                    _itemProcessQueue.TryRemove(key, out _);
                }
            }
        }

        _lock.Release();
    }

    /// <inheritdoc />
    public void AddItem(BaseItem item)
    {
        // Once 2s is elapsed, we release a lock from the semaphore.
        _ = Task.Run(static () =>
        {
            try
            {
                _lock.WaitOne(500);
            }
            catch (Exception)
            {
                return;
            }

            Thread.Sleep(500);
            _lock.Release();
        });

        _itemProcessQueue.TryAdd(item.Id, new QueuedItemContainer(item.Id));
        _logger.LogDebug("Queued {ItemName} for notification", item.Name);
    }
}
