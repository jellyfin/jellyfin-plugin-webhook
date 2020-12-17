using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Configuration;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using Jellyfin.Plugin.Webhook.Models;
using MediaBrowser.Common;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Notifier when a library item is added.
    /// </summary>
    public class ItemAddedNotifier : IServerEntryPoint
    {
        private readonly ILogger<ItemAddedNotifier> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly IApplicationHost _applicationHost;
        private readonly WebhookSender _webhookSender;

        private readonly ConcurrentDictionary<Guid, QueuedItemContainer> _itemProcessQueue;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _itemAddedPollTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemAddedNotifier"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{EventNotifier}"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="WebhookSender"/>.</param>
        public ItemAddedNotifier(
            ILogger<ItemAddedNotifier> logger,
            ILibraryManager libraryManager,
            IApplicationHost applicationHost,
            WebhookSender webhookSender)
        {
            _logger = logger;
            _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;

            _itemProcessQueue = new ConcurrentDictionary<Guid, QueuedItemContainer>();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public Task RunAsync()
        {
            _libraryManager.ItemAdded += ItemAddedHandler;

            HandlebarsFunctionHelpers.RegisterHelpers();
            _cancellationTokenSource = new CancellationTokenSource();
            _itemAddedPollTask = PeriodicAsyncHelper.PeriodicAsync(
                async () =>
                {
                    try
                    {
                        await ProcessItemsAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error");
                    }
                }, TimeSpan.FromMilliseconds(Constants.RecheckIntervalMs),
                _cancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Dispose all assets.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _libraryManager.ItemAdded -= ItemAddedHandler;
                _cancellationTokenSource?.Cancel();
                _itemAddedPollTask?.GetAwaiter().GetResult();
                _cancellationTokenSource?.Dispose();
            }
        }

        private void ItemAddedHandler(object? sender, ItemChangeEventArgs itemChangeEventArgs)
        {
            // Never notify on virtual items.
            if (itemChangeEventArgs.Item.IsVirtualItem)
            {
                return;
            }

            _itemProcessQueue.TryAdd(itemChangeEventArgs.Item.Id, new QueuedItemContainer(itemChangeEventArgs.Item.Id));
            _logger.LogDebug("Queued {itemName} for notification.", itemChangeEventArgs.Item.Name);
        }

        private async Task ProcessItemsAsync()
        {
            _logger.LogDebug("ProcessItemsAsync");
            // Attempt to process all items in queue.
            var currentItems = _itemProcessQueue.ToArray();
            foreach (var (key, container) in currentItems)
            {
                var item = _libraryManager.GetItemById(key);
                _logger.LogDebug("Item {itemName}", item.Name);

                // Metadata not refreshed yet and under retry limit.
                if (item.ProviderIds.Keys.Count == 0 && container.RetryCount < Constants.MaxRetries)
                {
                    _logger.LogDebug("Requeue {itemName}, no provider ids.", item.Name);
                    container.RetryCount++;
                    _itemProcessQueue.AddOrUpdate(key, container, (_, _) => container);
                    continue;
                }

                _logger.LogDebug("Notifying for {itemName}", item.Name);

                // Send notification to each configured destination.
                var dataObject = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    .AddBaseItemData(_applicationHost, item);

                var itemType = item.GetType();
                await _webhookSender.SendItemNotification(NotificationType.ItemAdded, dataObject, itemType);

                // Remove item from queue.
                _itemProcessQueue.TryRemove(key, out _);
            }
        }
    }
}