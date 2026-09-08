using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemDeletedNotifier;

/// <inheritdoc />
public class ItemDeletedManager : IItemDeletedManager
{
    private readonly ILogger<ItemDeletedManager> _logger;
    private readonly IServerApplicationHost _applicationHost;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemDeletedManager"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{ItemDeletedManager}"/> interface.</param>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    public ItemDeletedManager(
        ILogger<ItemDeletedManager> logger,
        IServerApplicationHost applicationHost)
    {
        _logger = logger;
        _applicationHost = applicationHost;
    }

    /// <inheritdoc />
    public void AddItem(BaseItem item)
    {
        // Skip notification if item type is Studio.
        if (item.GetType().Name == "Studio")
        {
            _logger.LogDebug("Skipping notification for item type Studio");
            return;
        }

        _logger.LogDebug("Queued {ItemName} for ItemDeleted notification", item.Name);

        // Deleted items already have metadata — capture the data now
        // (the item won't be queryable later) and fire after a short delay.
        var dataObject = DataObjectHelpers
            .GetBaseDataObject(_applicationHost, NotificationType.ItemDeleted)
            .AddBaseItemData(item);
        var itemType = item.GetType();

        _ = Task.Run(async () =>
        {
            try
            {
                var delayMs = (WebhookPlugin.Instance?.Configuration.ItemNotificationDelay ?? 5) * 1000;
                await Task.Delay(delayMs).ConfigureAwait(false);

                var scope = _applicationHost.ServiceProvider!.CreateAsyncScope();
                await using (scope.ConfigureAwait(false))
                {
                    var webhookSender = scope.ServiceProvider.GetRequiredService<IWebhookSender>();
                    await webhookSender.SendNotification(NotificationType.ItemDeleted, dataObject, itemType)
                        .ConfigureAwait(false);
                }

                _logger.LogDebug("Notified for deleted item {ItemName}", dataObject["Name"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ItemDeleted notification");
            }
        });
    }
}
