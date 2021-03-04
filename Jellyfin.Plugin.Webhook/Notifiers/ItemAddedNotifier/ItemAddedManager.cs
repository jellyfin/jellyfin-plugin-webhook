using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using Jellyfin.Plugin.Webhook.Models;
using MediaBrowser.Common;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemAddedNotifier
{
    /// <inheritdoc />
    public class ItemAddedManager : IItemAddedManager
    {
        private const int MaxRetries = 10;
        private readonly ILogger<ItemAddedManager> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly IApplicationHost _applicationHost;
        private readonly IWebhookSender _webhookSender;
        private readonly ConcurrentDictionary<Guid, QueuedItemContainer> _itemProcessQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemAddedManager"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{ItemAddedManager}"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
        public ItemAddedManager(
            ILogger<ItemAddedManager> logger,
            ILibraryManager libraryManager,
            IApplicationHost applicationHost,
            IWebhookSender webhookSender)
        {
            _logger = logger;
            _libraryManager = libraryManager;
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;
            _itemProcessQueue = new ConcurrentDictionary<Guid, QueuedItemContainer>();
        }

        /// <inheritdoc />
        public async Task ProcessItemsAsync()
        {
            _logger.LogDebug("ProcessItemsAsync");
            // Attempt to process all items in queue.
            var currentItems = _itemProcessQueue.ToArray();
            foreach (var (key, container) in currentItems)
            {
                var item = _libraryManager.GetItemById(key);
                if (item == null)
                {
                    // Remove item from queue.
                    _itemProcessQueue.TryRemove(key, out _);
                    return;
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
                await _webhookSender.SendNotification(NotificationType.ItemAdded, dataObject, itemType);

                // Remove item from queue.
                _itemProcessQueue.TryRemove(key, out _);
            }
        }

        /// <inheritdoc />
        public void AddItem(BaseItem item)
        {
            _itemProcessQueue.TryAdd(item.Id, new QueuedItemContainer(item.Id));
            _logger.LogDebug("Queued {ItemName} for notification", item.Name);
        }
    }
}